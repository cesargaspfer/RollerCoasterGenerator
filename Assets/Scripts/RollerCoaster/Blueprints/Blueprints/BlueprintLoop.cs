using System.Collections.Generic;
using UnityEngine;
using static RailModelProperties;

public class BlueprintLoop : Blueprint
{
    public BlueprintLoop() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {
        if (railPhysics.Final.Velocity >= 15f)
            return 1f;
        return 0f;
    }

    public override List<string> GetSubtypeNames()
    {
        List<string> names = new List<string>() { "Straight" };

        return names;
    }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["Straight"] = new Dictionary<string, string>() {
            {"lengthScale", "0.1;1.3;5;2;"},
            {"orientation", "2;-1;1;1;"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        int orientation = Random.Range(-1, 1) * 2 + 1;
        float MaxLengthScale = 1.5f + (rp.Final.Velocity - 18f) * 3.1f / 12f;
        float lengthScale = Random.Range(Mathf.Max(MaxLengthScale, 1.5f), Mathf.Min(MaxLengthScale, 4.6f));

        Dictionary<string, float> paramsDict = new Dictionary<string, float>() {
            {"lengthScale", lengthScale},
            {"orientation", orientation},
        };

        return paramsDict;
    }

    public override List<(RailProps, RailType)> GetBlueprint(string type, Dictionary<string, float> dict)
    {
        switch (type)
        {
            case "Straight":
                return Straight(dict);
            default:
                Debug.LogError("Type not found in BlueprintLoop.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> Straight(Dictionary<string, float> dict)
    {
        return Straight(dict["lengthScale"], (int)dict["orientation"]);
    }

    public static List<(RailProps, RailType)> Straight(float lengthScale, int orientation)
    {
        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();
        float elevation = Mathf.PI * 0.5f;

        rails.Add((new RailProps(0f, 0f, 0f, 5f * lengthScale), RailType.Normal));

        rails.Add((new RailProps(elevation, 0f, 0f, 6f * lengthScale), RailType.Normal));
        rails.Add((new RailProps(elevation, orientation * Mathf.PI / 12f, 0f, 5f * lengthScale), RailType.Normal));
        rails.Add((new RailProps(elevation, -orientation * Mathf.PI / 12f, 0f, 5f * lengthScale), RailType.Normal));
        rails.Add((new RailProps(elevation, 0f, 0f, 6f * lengthScale), RailType.Normal));

        rails.Add((new RailProps(0f, 0f, 0f, 5f * lengthScale), RailType.Normal));

        return rails;
    }

    public override string Name
    {
        get { return "Loop"; }
    }
}
