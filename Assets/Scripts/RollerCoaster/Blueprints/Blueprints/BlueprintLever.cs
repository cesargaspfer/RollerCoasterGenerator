using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static RailModelProperties;

public class BlueprintLever : Blueprint
{
    public BlueprintLever() { }

    public override float GetProbability(RailProps railProps, SpaceProps spaceProps, RailPhysics railPhysics)
    {
        if(spaceProps.Position.y < 5f && railPhysics.Final.Velocity < 4f)
            return 1f;
        return 0f;
    }

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
            {"height", "1;5;50;10;m"},
            {"pieces", "1;2;7;3;"},
        };

        dict["Rotate"] = new Dictionary<string, string>() {
            {"elevation", "15;30;60;45;°"},
            {"rotation", "15;-90;90;45;°"},
            {"height", "1;5;50;10;m"},
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
        return Straight(dict["elevation"], dict["height"], (int) dict["pieces"]);
    }

    public static List<(RailProps, RailType)> Straight(float elevation, float height, int pieces)
    {

        return Rotate(elevation, 0f, height, pieces);
    }

    public static List<(RailProps, RailType)> Rotate(Dictionary<string, float> dict)
    {
        return Rotate(dict["elevation"], dict["rotation"], dict["height"], (int)dict["pieces"]);
    }

    public static List<(RailProps, RailType)> Rotate(float elevation, float rotation, float height, int pieces)
    {
        elevation *= Mathf.PI / 180f;
        rotation *= Mathf.PI / 180f;

        List<(RailProps, RailType)> rails = new List<(RailProps, RailType)>();
        float inclination = -rotation * 0.5f;

        float initialRailHeight = RollerCoaster.MeasureRail(new RailProps(elevation, rotation, 0f, 1f)).y;
        float middleRailHeight = RollerCoaster.MeasureRail
        (
            new RailProps(0f, rotation, 0f, 1f),
            new SpaceProps(Vector3.zero, ThreeRotationMatrix(Matrix4x4.identity, new RailProps(elevation, 0f, 0f, 5f)))
        ).y;

        float length = height / (2f * initialRailHeight + (pieces - 2) * middleRailHeight);

        rails.Add((new RailProps(elevation, rotation, inclination, length), RailType.Lever));
        for (int i = 2; i < pieces; i++)
            rails.Add((new RailProps(0f, rotation, 0f, length), RailType.Lever));
        rails.Add((new RailProps(-elevation, rotation, -inclination, length), RailType.Lever));

        return rails;
    }
}
