using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static RailModelProperties;

public class BlueprintPlataform : Blueprint
{
    public BlueprintPlataform() { }

    public override float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics)
    {
        return 0f;
    }

    public override List<string> GetSubtypeNames()
    {
        List<string> names = new List<string>() { "Plataform" };

        return names;
    }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["Plataform"] = new Dictionary<string, string>() {
            {"length", "0;5;5;5;m"},
        };

        return dict;
    }

    public override Dictionary<string, float> GenerateParams(string subtype, RollerCoaster rollerCoaster, SpaceProps sp, RailPhysics rp)
    {
        float length = 5;

        Dictionary<string, float> paramsDict = new Dictionary<string, float>() {
            {"length", length},
        };

        return paramsDict;
    }

    public override List<(RailProps, RailType)> GetBlueprint(string type, Dictionary<string, float> dict)
    {
        switch (type)
        {
            case "Plataform":
                return Plataform(dict);
            default:
                Debug.LogError("Type not found in BlueprintLever.GetBlueprint");
                return null;
        }
    }

    public static List<(RailProps, RailType)> Plataform(Dictionary<string, float> dict)
    {
        return Plataform(dict["length"]);
    }

    public static List<(RailProps, RailType)> Plataform(float length)
    {

        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();

        rails.Add((new RailProps(0f, 0f, 0f, length), RailType.Platform));

        return rails;
    }

    public override string Name
    {
        get { return "Plataform"; }
    }
}
