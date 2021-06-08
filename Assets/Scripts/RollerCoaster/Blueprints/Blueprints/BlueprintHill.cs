using System.Collections.Generic;
using UnityEngine;
using static RailModelProperties;

public class BlueprintHill : Blueprint
{
    public BlueprintHill() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {
        if (railPhysics.Final.Velocity > 8f)
            return 1f;
        return 0f;
    }

    public override List<string> GetSubtypeNames()
    {
        List<string> names = new List<string>() { "StraightHeight", "StraightLength", "RotateHeight", "RotateLength" };

        return names;
    }

    public override List<string> GetGenerationSubtypeNames()
    {
        List<string> names = new List<string>() { "StraightHeight", "RotateHeight" };

        return names;
    }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["StraightHeight"] = new Dictionary<string, string>() {
            {"elevation", "15;15;90;45;°"},
            {"height", "1;5;25;10;"},
        };

        dict["StraightLength"] = new Dictionary<string, string>() {
            {"elevation", "15;15;90;45;°"},
            {"length", "2;20;80;10;"},
        };

        dict["RotateHeight"] = new Dictionary<string, string>() {
            {"elevation", "15;15;90;45;°"},
            {"rotation", "15;-180;180;45;°"},
            {"height", "1;5;25;10;"},
        };

        dict["RotateLength"] = new Dictionary<string, string>() {
            {"elevation", "15;15;90;45;°"},
            {"rotation", "15;-180;180;45;°"},
            {"length", "2;20;80;10;"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        float elevation = ((int)Random.Range(3, 5));
        float rotation = ((int)Random.Range(-6, 7));
        if ((int)Random.Range(-1, 1) == 0)
        {
            rotation = ((int)Random.Range(-12, 13));
        }

        float maxHeight = rp.Final.Velocity * (rp.Final.Velocity / (19.6f * 0.9f));

        float height = Random.Range(maxHeight * 0.5f, maxHeight);

        elevation *= 15f;
        rotation *= 15f;

        Dictionary<string, float> paramsDict = new Dictionary<string, float>() {
            {"elevation", elevation},
            {"rotation", rotation},
            {"height", height},
            {"length", height},
        };

        return paramsDict;
    }

    public override List<(RailProps, RailType)> GetBlueprint(string type, Dictionary<string, float> dict)
    {
        switch (type)
        {
            case "StraightHeight":
                return StraightHeight(dict);
            case "StraightLength":
                return StraightLength(dict);
            case "RotateHeight":
                return RotateHeight(dict);
            case "RotateLength":
                return RotateLength(dict);
            default:
                Debug.LogError("Type not found in BlueprintHill.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> StraightHeight(Dictionary<string, float> dict)
    {
        return StraightHeight(dict["elevation"], (int)dict["height"]);
    }

    public static List<(RailProps, RailType)> StraightHeight(float elevation, int height)
    {
        return RotateHeight(elevation, 0f, height);
    }

    public static List<(RailProps, RailType)> StraightLength(Dictionary<string, float> dict)
    {
        return StraightLength(dict["elevation"], (int)dict["length"]);
    }

    public static List<(RailProps, RailType)> StraightLength(float elevation, int length)
    {
        return RotateLength(elevation, 0f, length);
    }

    public static List<(RailProps, RailType)> RotateHeight(Dictionary<string, float> dict)
    {
        if (!dict.ContainsKey("orientation"))
            dict["orientation"] = 1f;
        return RotateHeight(dict["elevation"], dict["rotation"], (int)dict["height"], (int)dict["orientation"]);
    }

    public static List<(RailProps, RailType)> RotateHeight(float elevation, float rotation, int height, int orientation = 1)
    {
        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();
        elevation *= Mathf.PI / 180f;
        rotation *= (Mathf.PI / 180f) * 0.25f * orientation;

        float railHeight = Mathf.Abs(RollerCoaster.MeasureRail(new RailProps(elevation, rotation, 0f, 1f)).y);
        float length = height / (2f * railHeight);

        rails.Add((new RailProps(elevation, rotation, -rotation * 0.5f, length), RailType.Normal));
        rails.Add((new RailProps(-elevation, rotation, 0f, length), RailType.Normal));
        rails.Add((new RailProps(-elevation, rotation, 0f, length), RailType.Normal));
        rails.Add((new RailProps(elevation, rotation, rotation * 0.5f, length), RailType.Normal));

        return rails;
    }

    public static List<(RailProps, RailType)> RotateLength(Dictionary<string, float> dict)
    {
        if (!dict.ContainsKey("orientation"))
            dict["orientation"] = 1f;
        return RotateLength(dict["elevation"], dict["rotation"], (int)dict["length"], (int)dict["orientation"]);
    }

    public static List<(RailProps, RailType)> RotateLength(float elevation, float rotation, int length, int orientation = 1)
    {
        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();
        elevation *= Mathf.PI / 180f;
        rotation *= (Mathf.PI / 180f) * 0.25f * orientation;


        float railLength = Mathf.Abs(RollerCoaster.MeasureRail(new RailProps(elevation, rotation, 0f, 1f)).x);
        float finalLength = length / (4f * railLength);

        rails.Add((new RailProps(elevation, rotation, -rotation * 0.5f, finalLength), RailType.Normal));
        rails.Add((new RailProps(-elevation, rotation, 0f, finalLength), RailType.Normal));
        rails.Add((new RailProps(-elevation, rotation, 0f, finalLength), RailType.Normal));
        rails.Add((new RailProps(elevation, rotation, rotation * 0.5f, finalLength), RailType.Normal));

        return rails;
    }

    public override string Name
    {
        get { return "Hill"; }
    }
}
