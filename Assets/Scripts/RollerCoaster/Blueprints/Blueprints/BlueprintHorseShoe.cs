using System.Collections.Generic;
using UnityEngine;
using static RailModelProperties;

public class BlueprintHorseShoe : Blueprint
{
    public BlueprintHorseShoe() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {
        if (railPhysics.Final.Velocity >= 17f)
            return 1f;
        return 0f;
    }

    public override List<string> GetSubtypeNames()
    {
        List<string> names = new List<string>() { "HorseShoe" };

        return names;
    }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["HorseShoe"] = new Dictionary<string, string>() {
            {"lengthScale", "0.1;0.6;2;1;"},
            {"orientation", "2;-1;1;1;"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        int orientation = Random.Range(-1, 1) * 2 + 1;
        float MaxLengthScale = 0.6f + (rp.Final.Velocity - 18f) * 0.1f;
        float lengthScale = Random.Range(Mathf.Max(MaxLengthScale, 1f), Mathf.Min(MaxLengthScale, 1.9f));

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
            case "HorseShoe":
                return HorseShoe(dict);
            default:
                Debug.LogError("Type not found in BlueprintHorseShoe.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> HorseShoe(Dictionary<string, float> dict)
    {
        return HorseShoe(dict["lengthScale"], (int)dict["orientation"]);
    }

    public static List<(RailProps, RailType)> HorseShoe(float lengthScale, int orientation)
    {
        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();

        rails.Add((new RailProps(1.047198f, 0f, 0f, 15 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(0.5235988f, 1.570796f * orientation, 0f, 7 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(-1.570796f, 0f, 0f, 7 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(-1.570796f, 0f, 0f, 7 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(0.5235988f, 1.570796f * orientation, 0f, 7 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.047198f, 0f, 0f, 15 * lengthScale), RailType.Normal));

        return rails;
    }

    public override string Name
    {
        get { return "HorseShoe"; }
    }
}
