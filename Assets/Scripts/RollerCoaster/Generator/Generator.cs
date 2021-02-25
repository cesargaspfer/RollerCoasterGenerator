using UnityEngine;
using static Algebra;
using static RailModelProperties;

public class Generator
{
    private RollerCoaster _rc;
    public Generator(RollerCoaster rollerCoaster)
    {
        _rc = rollerCoaster;
        Generate();
        // _rc.GenerateCoaster();
    }

    public void Generate()
    {
        int intencity = 0;
        Vector3 initialPosition = _rc.GetInitialPosition();
        Matrix4x4 initialBasis = _rc.GetInitialBasis(); 
        GeneratePlataform();
        GenerateLever(intencity);
        GenerateFall(intencity);
        AddRail(length:4f);
        GenerateLoop();
        float angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        if(angleToPlane > 0f)
            GenerateCurveMax90(angleToPlane);
        angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        if (angleToPlane > 0f)
            GenerateCurveMax90(angleToPlane);

        AddRail();
        AddRail();
        AddRail();

        angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        Debug.Log(GetProjectedXfromInitialBasis(initialPosition, initialBasis) + " " + _rc.GetFinalPosition());
        
        
        // angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        // angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        // angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        // angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        // angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        // angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        // angleToPlane = GetAngleToInitialBasisPlane(initialPosition, initialBasis);
        AddRail();
        _rc.AddFinalRail();
        // _rc.UpdateLastRail(railType: 3);
        // _rc.AddRail();
        // for(int i = 0; i < 5; i++)
        // {
        //     _rc.AddRail();
        //     _rc.UpdateLastRailAdd(elevation: ((int)Random.Range(-6, 7)) * Mathf.PI / 12f, rotation: ((int) Random.Range(-6, 7)) * Mathf.PI / 12f, inclination: 0f, length: 0, railType: 1);
        // }
        // _rc.AddRail();
        // _rc.AddFinalRail();
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
        float length = (int) Random.Range(5 + piecesOffset, 7) + lengthOffset;

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

        elevation *= Mathf.PI / 12f;
        currentHeight -= Random.Range(0f, currentHeight / 6f);
        currentHeight -= 0.1f;

        // H = sin(elevation) * length * (pieces - 1)
        float maxLenght = currentHeight  / (Mathf.Sin(-elevation));
        int pieces = Random.Range(3, 9);
        if(elevation < -4f)
        {
            pieces = Random.Range(2, 3);
        }
        float length = currentHeight / ((pieces - 1f) * Mathf.Sin(-elevation));
        while(length < 4f)
        {
            pieces--;
            length = currentHeight / ((pieces - 1f) * Mathf.Sin(-elevation));
        }
        int iterations = 0;
        while(length * (pieces-1) * Mathf.Sin(-elevation) > currentHeight - 0.2f)
        {
            length -= 0.1f;
            iterations++;
        }

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
        float lengthScale = 1;

        float elevation = Mathf.PI * 0.5f;
        AddRail(length: 5f * lengthScale, railType: (int)RailType.Normal);
        AddRail(elevation: elevation, length: 6f * lengthScale);
        AddRail(elevation: elevation, length: 5f * lengthScale, rotation: orientation * Mathf.PI / 12f);
        AddRail(elevation: elevation, length: 5f * lengthScale, rotation: -orientation * Mathf.PI / 12f);
        AddRail(elevation: elevation, length: 6f * lengthScale);
        AddRail(length: 5f * lengthScale, railType: (int)RailType.Normal);

    }

    private void GenerateCurveMax90(float rotation)
    {
        rotation = Mathf.Sign(rotation) * Mathf.Min(Mathf.Abs(rotation), Mathf.PI * 0.5f);

        int pieces = Random.Range(1, 4);
        float length = Random.Range(5f, 8f);
        rotation /= (float) pieces;

        if(pieces >= 2)
        {
            AddRail(rotation: rotation, inclination:-rotation, length: length, railType: (int)RailType.Normal);
            for (int i = 1; i < pieces - 1; i++)
                AddRail(rotation: rotation);
            AddRail(rotation: rotation, inclination: rotation);
        }
        else
        {
            AddRail(rotation: rotation, length: length, railType: (int)RailType.Normal);
            AddRail(rotation: rotation);
        }

    }

    private float GetAngleToInitialBasisPlane(Vector3 targetPosition, Matrix4x4 targetBasis)
    {
        Vector3 currentPosition = _rc.GetFinalPosition();
        Matrix4x4 currentBasis = _rc.GetFinalBasis();
        Vector3 cx = currentBasis.GetColumn(0);
        Vector3 tx = targetBasis.GetColumn(0);

        Vector3 pc = new Vector3(currentPosition.x, 0f, currentPosition.z);
        Vector3 pt = new Vector3(targetPosition.x, 0f, targetPosition.z);

        Vector3 pcx = new Vector3(cx.x, 0f, cx.z);
        Vector3 ptx = (new Vector3(tx.x, 0f, tx.z)).normalized;

        Vector3 dir = pt - pc;
        Vector3 projX = ptx * Vector3.Dot(dir, ptx) / dir.magnitude;

        float rotation = Angle(pcx, projX);
        Matrix4x4 tmpRotationMatrix = RotationMatrix(rotation, Vector3.up);
        if ((projX.normalized - tmpRotationMatrix.MultiplyPoint3x4(cx)).magnitude > 0.01f)
        {
            rotation = -rotation;
        }
        Debug.Log(rotation);

        return rotation;
    }

    private float GetProjectedXfromInitialBasis(Vector3 targetPosition, Matrix4x4 targetBasis)
    {
        Vector3 currentPosition = _rc.GetFinalPosition();
        Vector3 tx = targetBasis.GetColumn(0);

        Vector3 pc = new Vector3(currentPosition.x, 0f, currentPosition.z);
        Vector3 pt = new Vector3(targetPosition.x, 0f, targetPosition.z);

        Vector3 ptx = (new Vector3(tx.x, 0f, tx.z)).normalized;

        Vector3 dir = (pt - pc).normalized;
        Vector3 projX = ptx * Vector3.Dot(dir, ptx) / dir.magnitude;

        float signal = Vector3.Dot(dir, projX) >= 1f ? 1f : -1f;

        return signal * (pt - pc).magnitude;
    }

    private void GenerateHill()
    {

    }

    private void AddRail(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
    {
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: elevation, rotation:rotation, inclination:inclination);
        _rc.UpdateLastRail(length: length, railType: railType);
    }

    private void TestCoaster()
    {
        float pi = Mathf.PI;
        _rc.AddRail();
        _rc.UpdateLastRailAdd(railType: 0);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: pi / 6f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: -pi / 6f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: pi / 4f, inclination: -pi / 4f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 3);
        _rc.AddRail();
        _rc.AddFinalRail();
    }
}
