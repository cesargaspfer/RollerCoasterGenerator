using System.Collections.Generic;
using UnityEngine;
using static RailModelProperties;

public class BlueprintCobraRoll : Blueprint
{
    public BlueprintCobraRoll() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {
        if (railPhysics.Final.Velocity > 10f)
            return 1f;
        return 0f;
    }

    public override List<string> GetSubtypeNames()
    {
        List<string> names = new List<string>() { "CobraRoll" };

        return names;
    }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["CobraRoll"] = new Dictionary<string, string>() {
            {"lengthScale", "0.1;1;2;1;"},
            {"orientation", "2;-1;1;1;"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        int orientation = Random.Range(-1, 1) * 2 + 1;
        float lengthScale = rp.Final.Velocity / 12f;
        lengthScale = Random.Range(Mathf.Max(lengthScale * 0.9f, 1f), lengthScale);

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
            case "CobraRoll":
                return CobraRoll(dict);
            default:
                Debug.LogError("Type not found in BlueprintCobraRoll.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> CobraRoll(Dictionary<string, float> dict)
    {
        return CobraRoll(dict["lengthScale"], (int)dict["orientation"]);
    }

    public static List<(RailProps, RailType)> CobraRoll(float lengthScale, int orientation)
    {
        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();

        rails.Add((new RailProps(0.7853982f, -0.2617994f * orientation, 0f, 10 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570796f, 0.2617994f * orientation, 0f, 10 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570796f, 0.7853982f * orientation, 0f, 5 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570796f, 1.047198f * orientation, 0f, 7 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(0.7853982f, -0.2617993f * orientation, 0f, 10 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(0.7853982f, -0.2617993f * orientation, 0f, 10 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570796f, 1.047198f * orientation, 0f, 7 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570796f, 0.5235987f * orientation, 0f, 7 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570796f, 0.2617993f * orientation, 0f, 10 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(0.7853982f, 0f, 0f, 10 * lengthScale), RailType.Normal));

        return rails;
    }

    public override string Name
    {
        get { return "CobraRoll"; }
    }
}
