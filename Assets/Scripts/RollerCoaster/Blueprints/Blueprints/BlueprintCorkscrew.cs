using System.Collections.Generic;
using UnityEngine;
using static RailModelProperties;

public class BlueprintCorkscrew : Blueprint
{
    public BlueprintCorkscrew() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {
        if (railPhysics.Final.Velocity > 10f)
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
            {"lengthScale", "0.1;1;2;1;"},
            {"orientation", "2;-1;1;1;"},
            {"loops", "1;1;5;3;"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        int orientation = Random.Range(-1, 1) * 2 + 1;
        float lengthScale = rp.Final.Velocity / 12f;
        lengthScale = Random.Range(Mathf.Max(lengthScale * 0.9f, 1f), lengthScale);
        int loops = Random.Range(1, 6);

        Dictionary<string, float> paramsDict = new Dictionary<string, float>() {
            {"lengthScale", lengthScale},
            {"orientation", orientation},
            {"loops", loops},
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
                Debug.LogError("Type not found in BlueprintCorkscrew.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> Straight(Dictionary<string, float> dict)
    {
        return Straight(dict["lengthScale"], (int)dict["orientation"], (int)dict["loops"]);
    }

    public static List<(RailProps, RailType)> Straight(float lengthScale, int orientation, int loop)
    {
        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();

        rails.Add((new RailProps(0f, 0.7853982f * orientation, -0.7853982f * orientation, 8 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570796f, 0.7853982f * orientation, 0.7853982f * orientation, 8 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570796f, 0.5235987f * orientation, 0f, 8 * lengthScale), RailType.Normal));

        for(int i = 1; i < loop; i++)
        {
            rails.Add((new RailProps(1.570796f, -0.5235988f * orientation, 0f, 8 * lengthScale), RailType.Normal));
            rails.Add((new RailProps(1.570796f, -0.5235988f * orientation, 0f, 8 * lengthScale), RailType.Normal));
            rails.Add((new RailProps(1.570796f, 0.5235988f * orientation, 0f, 8 * lengthScale), RailType.Normal));
            rails.Add((new RailProps(1.570796f, 0.5235988f * orientation, 0f, 8 * lengthScale), RailType.Normal));
        }

        rails.Add((new RailProps(1.570796f, -0.5235988f * orientation, 0f, 8 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(1.570797f, -0.7853982f * orientation, 0.7853982f * orientation, 8 * lengthScale), RailType.Normal));
        rails.Add((new RailProps(0f, -0.7853982f * orientation, -0.7853982f * orientation, 8 * lengthScale), RailType.Normal));

        return rails;
    }

    public override string Name
    {
        get { return "Corkscrew"; }
    }
}
