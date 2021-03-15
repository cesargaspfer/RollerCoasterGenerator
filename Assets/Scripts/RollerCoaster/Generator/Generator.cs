using UnityEngine;
using static Algebra;
using static ImaginarySphere;
using static RailModelProperties;

public class Generator
{
    private struct Status
    {
        public RailPhysics rp;
        public Vector3 finalPos;
        public Matrix4x4 finalBasis;
        public float height
        {
            get { return finalPos.y; }
        }

        public Status(RailPhysics rpValue, Vector3 finalPosValue, Matrix4x4 finalBasisValue)
        {
            rp = rpValue;
            finalPos = finalPosValue;
            finalBasis = finalBasisValue;
        }
    }

    public Vector3 initialPosition;
    public Matrix4x4 initialBasis;

    private RollerCoaster _rc;
    private Status _status;

    public Generator(RollerCoaster rollerCoaster)
    {
        _rc = rollerCoaster;
        // Generate();
        // _rc.GenerateCoaster();
    }

    public void Generate()
    {
        int intencity = 0;
        initialPosition = _rc.GetInitialPosition();
        initialBasis = _rc.GetInitialBasis();
        GeneratePlataform();
        GenerateLever(intencity);
        GenerateFall(intencity);
        AddRail(length:4f);
        int loops = Random.Range(2, 3);
        for (int i = 0; i < loops; i++)
        {
            if(_status.rp.Final.Velocity / 8.5f >= 1f)
                GenerateLoop();
        }
        float angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        if(Mathf.Abs(angleToPlane) > 0f)
            GenerateCurveMax90(angleToPlane);
        angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        if (Mathf.Abs(angleToPlane) > 0f)
            GenerateCurveMax90(angleToPlane);

        int hills = Random.Range(0, 4);
        for(int i = 0; i < hills; i++)
        {
            GenerateHill();
        }


        angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        float signedDistance = GetSignedDistanceFromBasis(initialPosition, initialBasis, 0);
        // Debug.Log(angleToPlane + " " + signedDistance);
        if(signedDistance > 0f)
        {
            if (Mathf.Abs(angleToPlane) > Mathf.PI)
                GenerateCurveMax90(angleToPlane);
            angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
            if (Mathf.Abs(angleToPlane) > 0f)
                GenerateCurveMax90(angleToPlane);
            int iterations = 0;
            while(GetSignedDistanceFromBasis(initialPosition, initialBasis, 0) > 0f)
            {
                AddRail();
                iterations++;
                if(iterations > 20)
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
            GenerateCurveMax90(GetBasisAngle(initialPosition, initialBasis));
        }


        // if (Mathf.Abs(angleToPlane) > 0f)
        //     GenerateCurveMax90(angleToPlane);

        
        
        
        AddRail();
        _rc.AddFinalRail();
        // _rc.UpdateLastRail(railType: 3);
        // _rc.AddRail(false, true);
        // for(int i = 0; i < 5; i++)
        // {
        //     _rc.AddRail(false, true);
        //     _rc.UpdateLastRailAdd(elevation: ((int)Random.Range(-6, 7)) * Mathf.PI / 12f, rotation: ((int) Random.Range(-6, 7)) * Mathf.PI / 12f, inclination: 0f, length: 0, railType: 1);
        // }
        // _rc.AddRail(false, true);
        // _rc.AddFinalRail();
    }

    private void UpdateStatus()
    {
        _status = new Status(_rc.GetLastRailPhysics(), _rc.GetFinalPosition(), _rc.GetFinalBasis());
    }

    private void GeneratePlataform()
    {
        AddRail(length: 5f, railType: (int)RailType.Platform);
    }

    private void GenerateLever(int intencity)
    {
        // TODO: Use intencity

        float elevation = ((int)Random.Range(2+intencity/2, 5));
        float rotation = 0f;
        if((int)Random.Range(-1, 2) > 0)
        {
            int rotationRand = Random.Range(-2, 3);
            rotation = Mathf.Sign(rotationRand) * (Mathf.Abs(rotationRand) + 3 - elevation) * Mathf.PI / 12f;
        }

        int piecesOffset = 0;
        if(elevation == 4f)
            piecesOffset = 1;
        int pieces = (int)Random.Range(3, 6);
        int lengthOffset = (4 - pieces) + (3 - (int) elevation);
        float length = (int) Random.Range(6 + piecesOffset, 9) + lengthOffset;

        elevation *= Mathf.PI / 12f;

        AddRail(elevation:elevation, rotation:rotation, length:length, railType: (int) RailType.Lever);
        for(int i = 0; i < pieces - 2; i++)
            AddRail(rotation: rotation);
        AddRail(elevation: -elevation, rotation: rotation);
    }

    private void GenerateFall(int intencity)
    {
        float currentHeight = _rc.GetFinalPosition().y - 1f;
        float initialCurrentHeight = currentHeight;

        // float elevation = ((int)Random.Range(-6 - (intencity - 2), -2 - intencity)) * Mathf.PI / 12f;
        int elevationOffset = ((int)Random.Range(0, 2));
        float elevation = ((int)Random.Range(-6 + elevationOffset, -1));

        float rotationOffset = 4 - (int) elevation / 3;
        float rotation = (int)Random.Range(-rotationOffset, rotationOffset + 1) * Mathf.PI / 12f;  
        if(elevation < -4)
            rotation = 0f;
               
        float inclination = -rotation;


        int pieces = Random.Range(3, 9);
        if(elevation < -4f)
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
            if(!changed)
            {
                if((int)Random.Range(-(3 * pieces), 1) == 0)
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

        float maxHeight = _status.rp.Final.Velocity * ( _status.rp.Final.Velocity / (19.6f * 0.9f) );

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
        
        float length = ( (_status.rp.Final.Velocity * 2f) / ( (float) pieces) ) * ( Mathf.Abs(rotation) * 2f / Mathf.PI );
        length = Mathf.Max(length, 1f);

        rotation /= (float) pieces;

        AddRail(rotation: rotation, inclination:-rotation, length: length, railType: (int)RailType.Normal);
        for (int i = 1; i < pieces - 1; i++)
            AddRail(rotation: rotation);
        AddRail(rotation: rotation, inclination: rotation);
    }

    private void AddRail(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
    {
        _rc.AddRail(false, false);
        _rc.UpdateLastRailAdd(elevation: elevation, rotation: rotation, inclination: inclination, simulateRail: false);
        _rc.UpdateLastRail(length: length, railType: railType, simulateRail: true);
        UpdateStatus();
    }

    private float GetAngleToInitialBasisPlane(Vector3 targetPosition, Matrix4x4 targetBasis)
    {
        Vector3 currentPosition = _rc.GetFinalPosition();
        Matrix4x4 currentBasis = _rc.GetFinalBasis();

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

    private float GetSignedDistanceFromBasis(Vector3 targetPosition, Matrix4x4 targetBasis, int orientation)
    {
        Vector3 currentPosition = _rc.GetFinalPosition();

        Vector3 tx = targetBasis.GetColumn(orientation);
        Vector3 pc;
        Vector3 pt;
        Vector3 ptx;
        
        pc = new Vector3(currentPosition.x, 0f, currentPosition.z);
        pt = new Vector3(targetPosition.x, 0f, targetPosition.z);
        ptx = (new Vector3(tx.x, 0f, tx.z)).normalized;

        // if (orientation == 0)
        // {
        //     pc = new Vector3(0f, currentPosition.y, currentPosition.z);
        //     pt = new Vector3(0f, targetPosition.y, targetPosition.z);
        //     ptx = (new Vector3(0f, tx.y, tx.z)).normalized;
        // }
        // else if (orientation == 1)
        // {
        // }
        // else
        // {
        //     pc = new Vector3(currentPosition.x, currentPosition.y, 0f);
        //     pt = new Vector3(targetPosition.x, targetPosition.y, 0f);
        //     ptx = (new Vector3(tx.x, tx.y, 0f)).normalized;
        // }

        Vector3 dir = (pc - pt).normalized;

        float signal = Vector3.Dot(dir, ptx) >= 0f ? 1f : -1f;

        return signal * (pc - pt).magnitude;
    }

    private float GetBasisAngle(Vector3 targetPosition, Matrix4x4 targetBasis)
    {
        Matrix4x4 currentBasis = _rc.GetFinalBasis();
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

        if(rotation > 3.1415f && GetSignedDistanceFromBasis(targetPosition, targetBasis, 2) > 0f)
        {
            rotation = -rotation;
        }

        return rotation;
    }

    private void TestCoaster()
    {
        float pi = Mathf.PI;
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(railType: 0);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: pi / 6f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: -pi / 6f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail(false, true);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: pi / 4f, inclination: -pi / 4f, length: 0, railType: 1);
        _rc.AddRail(false, true);
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 3);
        _rc.AddRail(false, true);
        _rc.AddFinalRail();
    }
}
