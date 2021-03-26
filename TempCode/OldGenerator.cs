using System.Collections;
using UnityEngine;
using static Algebra;
using static ImaginarySphere;
using static RailModelProperties;

public class OldGenerator
{
    private struct Status
    {
        public RailPhysics rp;
        public Vector3 finalPos;
        public Matrix4x4 finalBasis;
        public float totalLength;
        public float height
        {
            get { return finalPos.y; }
        }

        public Status(RailPhysics rpValue, Vector3 finalPosValue, Matrix4x4 finalBasisValue, float totalLengthValue)
        {
            rp = rpValue;
            finalPos = finalPosValue;
            finalBasis = finalBasisValue;
            totalLength = totalLengthValue;
        }
    }

    public Vector3 initialPosition;
    public Matrix4x4 initialBasis;

    private RollerCoaster _rollerCoaster;
    private BlueprintManager _blueprintManager;
    private Status _status;
    private bool _isGenerating = false;
    private IEnumerator currentGeneratingCoroutine = null;
    private IEnumerator secondaryCoroutine = null;
    private bool canContinue = true;

    public OldGenerator(RollerCoaster rollerCoaster)
    {
        _rollerCoaster = rollerCoaster;
        _blueprintManager = _rollerCoaster.GetBlueprintManager();
        _isGenerating = false;
        // Generate();
        // _rc.GenerateCoaster();
    }

    public void Generate()
    {
        _isGenerating = true;
        if (secondaryCoroutine != null)
            _rollerCoaster.StopChildCoroutine(secondaryCoroutine);
        if (currentGeneratingCoroutine != null)
            _rollerCoaster.StopChildCoroutine(currentGeneratingCoroutine);
        currentGeneratingCoroutine = GenerateCoroutine();
        secondaryCoroutine = null;
        canContinue = true;

        _rollerCoaster.StartChildCoroutine(currentGeneratingCoroutine);
    }

    private IEnumerator GenerateCoroutine()
    {
        int intencity = 0;
        initialPosition = _rollerCoaster.GetInitialPosition();
        initialBasis = _rollerCoaster.GetInitialBasis();
        GeneratePlataform();
        GenerateLever(intencity);
        yield return new WaitUntil(() => canContinue);
        GenerateFall(intencity);
        yield return new WaitUntil(() => canContinue);
        AddRail(length: 4f);
        yield return new WaitUntil(() => canContinue);
        int loops = Random.Range(2, 3);
        for (int i = 0; i < loops; i++)
        {
            if (_status.rp.Final.Velocity / 8.5f >= 1f)
            {
                yield return new WaitUntil(() => canContinue);
                GenerateLoop();
            }
        }
        yield return new WaitUntil(() => canContinue);
        float angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        if (Mathf.Abs(angleToPlane) > 0f)
            GenerateCurveMax90(angleToPlane);
        yield return new WaitUntil(() => canContinue);
        angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        if (Mathf.Abs(angleToPlane) > 0f)
            GenerateCurveMax90(angleToPlane);

        int hills = Random.Range(0, 4);
        for (int i = 0; i < hills; i++)
        {
            yield return new WaitUntil(() => canContinue);
            GenerateHill();
        }


        angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        float signedDistance = GetSignedDistanceFromPlane(initialBasis.GetColumn(0), initialPosition, _rollerCoaster.GetFinalPosition());
        // Debug.Log(angleToPlane + " " + signedDistance);
        if (signedDistance > 0f)
        {
            yield return new WaitUntil(() => canContinue);
            if (Mathf.Abs(angleToPlane) > Mathf.PI)
                GenerateCurveMax90(angleToPlane);
            angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
            yield return new WaitUntil(() => canContinue);
            if (Mathf.Abs(angleToPlane) > 0f)
                GenerateCurveMax90(angleToPlane);
            int iterations = 0;
            while (GetSignedDistanceFromPlane(initialBasis.GetColumn(0), initialPosition, _rollerCoaster.GetFinalPosition()) > 0f)
            {
                yield return new WaitUntil(() => canContinue);
                AddRail();
                iterations++;
                if (iterations > 20)
                {
                    Debug.LogError("iterations > 20 in Generate");
                    break;
                }
            }
        }
        angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        // Debug.Log(angleToPlane);
        if (Mathf.Abs(angleToPlane) > Mathf.PI * 0.5f)
        {
            yield return new WaitUntil(() => canContinue);
            GenerateCurveMax90(GetBasisAngle(initialPosition, initialBasis));
        }


        // if (Mathf.Abs(angleToPlane) > 0f)
        //     GenerateCurveMax90(angleToPlane);



        yield return new WaitUntil(() => canContinue);
        AddRail();
        yield return new WaitUntil(() => canContinue);
        _rollerCoaster.AddFinalRail();
        // _rc.UpdateLastRail(railType: 3);
        // _rc.AddRail(false, true);
        // for(int i = 0; i < 5; i++)
        // {
        //     _rc.AddRail(false, true);
        //     _rc.UpdateLastRailAdd(elevation: ((int)Random.Range(-6, 7)) * Mathf.PI / 12f, rotation: ((int) Random.Range(-6, 7)) * Mathf.PI / 12f, inclination: 0f, length: 0, railType: 1);
        // }
        // _rc.AddRail(false, true);
        // _rc.AddFinalRail();

        _isGenerating = false;
    }

    private void StartUpdateStatus()
    {
        secondaryCoroutine = UpdateStatus();
        canContinue = false;
        _rollerCoaster.StartChildCoroutine(secondaryCoroutine);
    }

    private IEnumerator UpdateStatus()
    {
        RailPhysics railPhysics = _rollerCoaster.GetLastRailPhysics();
        while (railPhysics == null || railPhysics.Final == null)
        {
            yield return null;
            railPhysics = _rollerCoaster.GetLastRailPhysics();
        }
        _status = new Status(railPhysics, _rollerCoaster.GetFinalPosition(), _rollerCoaster.GetFinalBasis(), _rollerCoaster.GetTotalLength());
        canContinue = true;
    }

    private void GeneratePlataform()
    {
        AddRail(length: 5f, railType: (int)RailType.Platform);
    }

    private void GenerateLever(int intencity)
    {
        // TODO: Use intencity

        float elevation = ((int)Random.Range(2 + intencity / 2, 5));
        float rotation = 0f;
        if ((int)Random.Range(-1, 2) > 0)
        {
            int rotationRand = Random.Range(-2, 3);
            rotation = Mathf.Sign(rotationRand) * (Mathf.Abs(rotationRand) + 3 - elevation) * Mathf.PI / 12f;
        }

        int piecesOffset = 0;
        if (elevation == 4f)
            piecesOffset = 1;
        int pieces = (int)Random.Range(3, 6);
        int lengthOffset = (4 - pieces) + (3 - (int)elevation);
        float length = (int)Random.Range(6 + piecesOffset, 9) + lengthOffset;

        elevation *= Mathf.PI / 12f;

        AddRail(elevation: elevation, rotation: rotation, length: length, railType: (int)RailType.Lever);
        for (int i = 0; i < pieces - 2; i++)
            AddRail(rotation: rotation);
        AddRail(elevation: -elevation, rotation: rotation);
    }

    private void GenerateFall(int intencity)
    {
        float currentHeight = _rollerCoaster.GetFinalPosition().y - 1f;
        float initialCurrentHeight = currentHeight;

        // float elevation = ((int)Random.Range(-6 - (intencity - 2), -2 - intencity)) * Mathf.PI / 12f;
        int elevationOffset = ((int)Random.Range(0, 2));
        float elevation = ((int)Random.Range(-6 + elevationOffset, -1));

        float rotationOffset = 4 - (int)elevation / 3;
        float rotation = (int)Random.Range(-rotationOffset, rotationOffset + 1) * Mathf.PI / 12f;
        if (elevation < -4)
            rotation = 0f;

        float inclination = -rotation;


        int pieces = Random.Range(3, 9);
        if (elevation < -4f)
        {
            pieces = Random.Range(2, 3);
        }

        elevation *= Mathf.PI / 12f;
        currentHeight -= Random.Range(0f, currentHeight / 4f);

        SpaceProps sp = new SpaceProps(_status.finalPos, _status.finalBasis);
        RailProps rp = new RailProps(elevation, rotation, inclination, 1f);
        (_, Matrix4x4 finalBasis, Vector3 finalPosition) = CalculateImaginarySphere(sp, rp);
        float h1 = _status.finalPos.y - finalPosition.y;

        sp = new SpaceProps(_status.finalPos, finalBasis);
        rp = new RailProps(0f, rotation, inclination, 1f);
        (_, _, finalPosition) = CalculateImaginarySphere(sp, rp);
        float h2 = _status.finalPos.y - finalPosition.y;

        float length = currentHeight / (2f * h1 + (pieces - 2) * h2);

        AddRail(elevation: elevation, rotation: rotation, length: length, inclination: inclination, railType: (int)RailType.Normal);
        bool changed = false;
        for (int i = 0; i < pieces - 2; i++)
        {
            if (!changed)
            {
                if ((int)Random.Range(-(3 * pieces), 1) == 0)
                {
                    AddRail(rotation: rotation, inclination: -inclination);
                    changed = true;
                }
                else
                {
                    AddRail(rotation: rotation);
                }
            }
            else
            {
                changed = false;
                rotation = -rotation;
                inclination = -inclination;
                AddRail(rotation: rotation, inclination: inclination);
            }
        }
        if (!changed)
        {
            AddRail(elevation: -elevation, rotation: rotation, inclination: -inclination);
        }
        else
        {
            AddRail(elevation: -elevation, rotation: rotation);
        }
    }

    private void GenerateLoop()
    {
        int orientation = Random.Range(-1, 1) * 2 + 1;
        float lengthScale = _status.rp.Final.Velocity / 8.5f;
        lengthScale = Random.Range(Mathf.Max(lengthScale * 0.75f, 1f), lengthScale);

        float elevation = Mathf.PI * 0.5f;
        AddRail(length: 5f * lengthScale, railType: (int)RailType.Normal);
        AddRail(elevation: elevation, length: 6f * lengthScale);
        AddRail(elevation: elevation, length: 5f * lengthScale, rotation: orientation * Mathf.PI / 12f);
        AddRail(elevation: elevation, length: 5f * lengthScale, rotation: -orientation * Mathf.PI / 12f);
        AddRail(elevation: elevation, length: 6f * lengthScale);
        AddRail(length: 5f * lengthScale, railType: (int)RailType.Normal);
    }

    private void GenerateHill()
    {
        // TODO: Use intencity & velocity

        float elevation = ((int)Random.Range(3, 4));
        float rotation = 0f;
        if ((int)Random.Range(-1, 2) > 0)
        {
            int rotationRand = Random.Range(-2, 3);
            rotation = Mathf.Sign(rotationRand) * (Mathf.Abs(rotationRand) + 3 - elevation) * Mathf.PI / 12f;
        }
        elevation *= Mathf.PI / 12f;

        float maxHeight = _status.rp.Final.Velocity * (_status.rp.Final.Velocity / (19.6f * 0.9f));

        SpaceProps sp = new SpaceProps(_status.finalPos, _status.finalBasis);
        RailProps rp = new RailProps(elevation, rotation, -rotation, 1f);
        (_, Matrix4x4 finalBasis, Vector3 finalPosition) = CalculateImaginarySphere(sp, rp);
        float heigth = finalPosition.y - _status.finalPos.y;

        float length = maxHeight / (2f * heigth);

        length = Random.Range(length * 0.75f, length);


        AddRail(elevation: elevation, rotation: rotation, inclination: -rotation, length: length, railType: (int)RailType.Normal);
        AddRail(elevation: -elevation, rotation: rotation);
        AddRail(elevation: -elevation, rotation: rotation);
        AddRail(elevation: elevation, rotation: rotation, inclination: rotation);
    }

    private void GenerateCurveMax90(float rotation)
    {
        rotation = Mathf.Sign(rotation) * Mathf.Min(Mathf.Abs(rotation), Mathf.PI * 0.5f);

        int pieces = Random.Range(2, 4);

        float length = ((_status.rp.Final.Velocity * 2f) / ((float)pieces)) * (Mathf.Abs(rotation) * 2f / Mathf.PI);
        length = Mathf.Max(length, 1f);

        rotation /= (float)pieces;

        AddRail(rotation: rotation, inclination: -rotation, length: length, railType: (int)RailType.Normal);
        for (int i = 1; i < pieces - 1; i++)
            AddRail(rotation: rotation);
        AddRail(rotation: rotation, inclination: rotation);
    }

    private void AddRail(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
    {
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: elevation, rotation: rotation, inclination: inclination);
        _rollerCoaster.UpdateLastRail(length: length, railType: railType);
        secondaryCoroutine = UpdateStatus();
        canContinue = false;
        _rollerCoaster.StartChildCoroutine(secondaryCoroutine);
    }

    private float GetAngleToInitialBasisPlane(Vector3 targetPosition, Matrix4x4 targetBasis)
    {
        Vector3 currentPosition = _rollerCoaster.GetFinalPosition();
        Matrix4x4 currentBasis = _rollerCoaster.GetFinalBasis();

        Vector3 cx = currentBasis.GetColumn(0);
        Vector3 tx = targetBasis.GetColumn(0);

        Vector3 pc = new Vector3(currentPosition.x, 0f, currentPosition.z);
        Vector3 pt = new Vector3(targetPosition.x, 0f, targetPosition.z);

        Vector3 pcx = (new Vector3(cx.x, 0f, cx.z)).normalized;
        Vector3 ptx = (new Vector3(tx.x, 0f, tx.z)).normalized;

        Vector3 dir = pt - pc;
        Vector3 projX = ptx * Vector3.Dot(dir, ptx) / dir.magnitude;

        float rotation = Angle(pcx, projX);
        Matrix4x4 tmpRotationMatrix = RotationMatrix(rotation, Vector3.up);
        if ((projX.normalized - tmpRotationMatrix.MultiplyPoint3x4(cx)).magnitude > 0.01f)
        {
            rotation = -rotation;
        }

        return rotation;
    }

    private float GetBasisAngle(Vector3 targetPosition, Matrix4x4 targetBasis)
    {
        Matrix4x4 currentBasis = _rollerCoaster.GetFinalBasis();
        Vector3 cx = currentBasis.GetColumn(0);
        Vector3 tx = targetBasis.GetColumn(0);

        Vector3 pcx = (new Vector3(cx.x, 0f, cx.z)).normalized;
        Vector3 ptx = (new Vector3(tx.x, 0f, tx.z)).normalized;

        float rotation = Angle(pcx, ptx);
        Matrix4x4 tmpRotationMatrix = RotationMatrix(rotation, Vector3.up);
        if ((pcx - tmpRotationMatrix.MultiplyPoint3x4(ptx)).magnitude > 0.01f)
        {
            rotation = -rotation;
        }
        // Debug.Log(GetSignedDistanceFromBasis(targetPosition, targetBasis, 2));

        if (rotation > 3.1415f && GetSignedDistanceFromPlane(targetBasis.GetColumn(2), targetPosition, _rollerCoaster.GetFinalPosition()) > 0f)
        {
            rotation = -rotation;
        }

        return rotation;
    }

    private void TestCoaster()
    {
        float pi = Mathf.PI;
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(railType: 0);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: pi / 6f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: -pi / 6f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: pi / 4f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 1);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: pi / 4f, rotation: pi / 4f, inclination: -pi / 4f, length: 0, railType: 1);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 3);
        _rollerCoaster.AddRail(false);
        _rollerCoaster.AddFinalRail();
    }

    public bool IsGenerating
    {
        get { return _isGenerating; }
    }
}
