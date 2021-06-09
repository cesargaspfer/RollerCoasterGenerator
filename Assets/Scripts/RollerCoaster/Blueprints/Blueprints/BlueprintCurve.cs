using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static RailModelProperties;

public class BlueprintCurve : Blueprint
{
    public BlueprintCurve() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {
        if (railPhysics.Final.Velocity >= 3f)
            return 1f;
        return 0f;
    }

    public override List<string> GetSubtypeNames()
    {
        List<string> names = new List<string>() { "Curve" };

        return names;
    }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["Curve"] = new Dictionary<string, string>() {
            {"rotation", "15;-180;90;180;°"},
            {"length", "1;5;50;10;m"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        float rotation = 15f * (int)Random.Range(5, 13);
        if ((int)Random.Range(-1, 1) == 0)
        {
            rotation = -rotation;
        }
        float length = ((rp.Final.Velocity * 2f)) * (Mathf.Abs(rotation) / 90f);
        length = Mathf.Max(length * 0.75f, 5f);

        Dictionary<string, float> paramsDict = new Dictionary<string, float>() {
            {"rotation", rotation},
            {"length", length},
        };

        return paramsDict;
    }

    public override List<(RailProps, RailType)> GetBlueprint(string type, Dictionary<string, float> dict)
    {
        switch (type)
        {
            case "Curve":
                return Curve(dict);
            default:
                Debug.LogError("Type not found in BlueprintLever.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> Curve(Dictionary<string, float> dict)
    {
        if(!dict.ContainsKey("orientation"))
            dict["orientation"] = 1f;
        return Curve(dict["rotation"], dict["length"], dict["orientation"]);
    }

    public static List<(RailProps, RailType)> Curve(float rotation, float length, float orientation = 1f)
    {
        rotation *= (Mathf.PI / 180f) * 0.25f * orientation;
        length *= 0.25f;
        float inclination = -rotation * 0.5f;

        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();

        rails.Add((new RailProps(0f, rotation, inclination, length), RailType.Normal));
        rails.Add((new RailProps(0f, rotation, 0f, length), RailType.Normal));
        rails.Add((new RailProps(0f, rotation, 0f, length), RailType.Normal));
        rails.Add((new RailProps(0f, rotation, -inclination, length), RailType.Normal));

        return rails;
    }

    public override string Name
    {
        get { return "Curve"; }
    }
}
