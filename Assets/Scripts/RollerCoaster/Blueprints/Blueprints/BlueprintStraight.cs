using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static RailModelProperties;

public class BlueprintStraight : Blueprint
{
    public BlueprintStraight() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {
        if (railPhysics.Final.Velocity >= 0f)
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
            {"length", "1;5;50;10;m"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        float length = Random.Range(1f, 50f);

        Dictionary<string, float> paramsDict = new Dictionary<string, float>() {
            {"length", length},
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
                Debug.LogError("Type not found in BlueprintLever.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> Straight(Dictionary<string, float> dict)
    {
        return Straight(dict["length"]);
    }

    public static List<(RailProps, RailType)> Straight(float length)
    {

        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();

        rails.Add((new RailProps(0f, 0f, 0f, length), RailType.Normal));

        return rails;
    }

    public override string Name
    {
        get { return "Straight"; }
    }
}