using System.Collections.Generic;
using UnityEngine;
using static RailModelProperties;

public class BlueprintLever : Blueprint
{
    public BlueprintLever() { }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["Straight"] = new Dictionary<string, string> () {
            {"elevation", "float"},
            {"railLength", "float"},
            {"pieces", "int"},
        };

        dict["Rotate"] = new Dictionary<string, string>() {
            {"elevation", "float"},
            {"rotation", "float"},
            {"railLength", "float"},
            {"pieces", "int"},
        };

        return dict;
    }

    public override List<(RailProps, RailType)> GetBlueprint(string type, Dictionary<string, float> dict)
    {
        switch(type)
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
        return Straight(dict["elevation"], dict["railLength"], (int) dict["pieces"]);
    }

    public static List<(RailProps, RailType)> Straight(float elevation, float railLength, int pieces)
    {
        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();

        rails.Add((new RailProps(elevation, 0f, 0f, railLength), RailType.Lever));
        for(int i = 2; i < pieces; i++)
            rails.Add((new RailProps(0f, 0f, 0f, railLength), RailType.Lever));
        rails.Add((new RailProps(-elevation, 0f, 0f, railLength), RailType.Lever));

        return rails;
    }

    public static List<(RailProps, RailType)> Rotate(Dictionary<string, float> dict)
    {
        return Rotate(dict["elevation"], dict["rotation"], dict["railLength"], (int)dict["pieces"]);
    }

    public static List<(RailProps, RailType)> Rotate(float elevation, float rotation, float railLength, int pieces)
    {
        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();
        float inclination = -rotation * 0.5f;

        rails.Add((new RailProps(elevation, rotation, inclination, railLength), RailType.Lever));
        for (int i = 2; i < pieces; i++)
            rails.Add((new RailProps(0f, rotation, 0f, railLength), RailType.Lever));
        rails.Add((new RailProps(-elevation, rotation, -inclination, railLength), RailType.Lever));

        return rails;
    }
}
