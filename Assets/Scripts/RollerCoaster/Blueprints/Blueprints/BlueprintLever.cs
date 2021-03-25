using System.Collections.Generic;
using UnityEngine;
using static RailModelProperties;

public class BlueprintLever : Blueprint
{
    public BlueprintLever() { }

    public override List<string> GetSubtypeNames()
    {
        List<string> names = new List<string>() { "Straight", "Rotate" };

        return names;
    }

    public override Dictionary<string, Dictionary<string, string>> GetParams()
    {
        Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();

        dict["Straight"] = new Dictionary<string, string> () {
            {"elevation", "15;30;60;45;°"},
            {"railLength", "1;5;10;7;m"},
            {"pieces", "1;2;7;3;"},
        };

        dict["Rotate"] = new Dictionary<string, string>() {
            {"elevation", "15;30;60;45;°"},
            {"rotation", "15;-90;90;45;°"},
            {"railLength", "1;5;10;7;m"},
            {"pieces", "1;2;7;3;"},
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
        elevation *= Mathf.PI / 180f;
        
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
        elevation *= Mathf.PI / 180f;
        rotation *= Mathf.PI / 180f;

        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();
        float inclination = -rotation * 0.5f;

        rails.Add((new RailProps(elevation, rotation, inclination, railLength), RailType.Lever));
        for (int i = 2; i < pieces; i++)
            rails.Add((new RailProps(0f, rotation, 0f, railLength), RailType.Lever));
        rails.Add((new RailProps(-elevation, rotation, -inclination, railLength), RailType.Lever));

        return rails;
    }
}
