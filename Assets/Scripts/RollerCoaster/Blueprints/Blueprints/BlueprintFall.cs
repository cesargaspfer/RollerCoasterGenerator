﻿using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static RailModelProperties;

public class BlueprintFall : Blueprint
{
    public BlueprintFall() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {

        if (spaceProps.Position.y >= 10f && railPhysics.Final.Velocity <= 6f)
            return 1000f;
        if (spaceProps.Position.y >= 10f && railPhysics.Final.Velocity <= 10f)
            return 1f;
        if(spaceProps.Position.y >= 5f && railPhysics.Final.Velocity <= 4f)
            return 1f;
        return 0f;
    }

    public override List<string> GetSubtypeNames()
    {
        List<string> names = new List<string>() { "Straight", "Rotate" };

        return names;
    }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["Straight"] = new Dictionary<string, string>() {
            {"elevation", "15;-90;-15;-45;°"},
            {"height", "1;5;100;10;m"},
            {"pieces", "1;2;7;3;"},
        };

        dict["Rotate"] = new Dictionary<string, string>() {
            {"elevation", "15;-60;-15;-45;°"},
            {"rotation", "15;-90;90;45;°"},
            {"height", "1;5;100;10;m"},
            {"pieces", "1;2;7;3;"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        float elevation = -15f * (int)Random.Range(3, 7);
        if(subtype.Equals("Rotate"))
        {
            elevation = -15f * (int)Random.Range(3, 5);
        }
        float rotation = 15f * (int)Random.Range(4, 7);
        if ((int)Random.Range(-1, 1) == 0)
        {
            rotation = -rotation;
        }
        float maxHeight = sp.Position.y;
        float height = Random.Range(maxHeight * 0.75f, maxHeight);
        float pieces = (int)Random.Range(2 + ((int)height - 10) / 10, 8);

        Dictionary<string, float> paramsDict = new Dictionary<string, float>() {
            {"elevation", elevation},
            {"rotation", rotation},
            {"height", height},
            {"pieces", pieces},
        };

        return paramsDict;
    }

    public override List<(RailProps, RailType)> GetBlueprint(string type, Dictionary<string, float> dict)
    {
        switch (type)
        {
            case "Straight":
                return Straight(dict);
            case "Rotate":
                return Rotate(dict);
            default:
                Debug.LogError("Type not found in BlueprintLever.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> Straight(Dictionary<string, float> dict)
    {
        return Straight(dict["elevation"], dict["height"], (int)dict["pieces"]);
    }

    public static List<(RailProps, RailType)> Straight(float elevation, float height, int pieces)
    {
        return Rotate(elevation, 0f,height , pieces);
    }

    public static List<(RailProps, RailType)> Rotate(Dictionary<string, float> dict)
    {
        if (!dict.ContainsKey("orientation"))
            dict["orientation"] = 1f;
        return Rotate(dict["elevation"], dict["rotation"], dict["height"], (int)dict["pieces"], (int)dict["orientation"]);
    }

    public static List<(RailProps, RailType)> Rotate(float elevation, float rotation, float height, int pieces, int orientation = 1)
    {
        elevation *= Mathf.PI / 180f;
        rotation *= Mathf.PI / 180f * orientation;

        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();
        float inclination = -rotation * 0.5f;

        float initialRailHeight = RollerCoaster.MeasureRail(new RailProps(-elevation, rotation, 0f, 1f)).y;
        float middleRailHeight = RollerCoaster.MeasureRail
        (
            new RailProps(0f, rotation, 0f, 1f), 
            new SpaceProps(Vector3.zero, ThreeRotationMatrix(Matrix4x4.identity, new RailProps(-elevation, 0f, 0f, 1f)))
        ).y;

        float length = height / (2f * initialRailHeight + (pieces - 2) * middleRailHeight);

        rails.Add((new RailProps(elevation, rotation, inclination, length), RailType.Normal));
        for (int i = 2; i < pieces; i++)
            rails.Add((new RailProps(0f, rotation, 0f, length), RailType.Normal));
        rails.Add((new RailProps(-elevation, rotation, -inclination, length), RailType.Normal));

        return rails;
    }
    
    public override string Name
    {
        get { return "Fall"; }
    }
}
