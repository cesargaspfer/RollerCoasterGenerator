using UnityEngine;
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
        GeneratePlataform();
        GenerateLever(intencity);
        GenerateFall(intencity);
        AddRail();
        _rc.AddFinalRail();
        _rc.UpdateLastRail(railType: 3);
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
        float currentHeight = _rc.GetLastPosition().y - 1f;
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
                }
                else
                {
                    AddRail(rotation: rotation);
                }
            }
            else
            {
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
