using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintManager
{
    
    private Dictionary<string, Blueprint> _blueprintDict;
    private List<(string, string, string)> _straitBps;
    private List<(string, string, string)> _curveBps;
    private List<(string, string, string)> _turnBps;
    public BlueprintManager()
    {
        _blueprintDict = new Dictionary<string, Blueprint>()
        {
            { "Lever", new BlueprintLever() },
            { "Fall", new BlueprintFall() },
            { "Loop", new BlueprintLoop() },
            { "Hill", new BlueprintHill() },
            { "Curve", new BlueprintCurve() },
            { "Corkscrew", new BlueprintCorkscrew() },
            { "Sidewinder", new BlueprintSidewinder() },
            { "CobraRoll", new BlueprintCobraRoll() },
            { "HorseShoe", new BlueprintHorseShoe() },
            { "PretzelKnot", new BlueprintPretzelKnot() },
            { "PretzelCurve", new BlueprintPretzelCurve() },
            { "Straight", new BlueprintStraight() },
            
        };

        _straitBps = new List<(string, string, string)>()
        {
            {("Loop", "Straight", "")},
            {("Hill", "StraightHeight", "")},
            {("Hill", "StraightLength", "")},
            {("Corkscrew", "Corkscrew", "")},
            {("Straight", "Straight", "")},

        };

        _curveBps = new List<(string, string, string)>()
        {
            {("Curve", "Curve", "rotation=90")},
            {("Hill", "RotateHeight", "rotation=90")},
            {("Hill", "RotateLength", "rotation=90")},
            {("Sidewinder", "Sidewinder", "")},

        };

        _turnBps = new List<(string, string, string)>()
        {
            {("Curve", "Curve", "rotation=180")},
            {("Hill", "RotateHeight", "rotation=180")},
            {("Hill", "RotateLength", "rotation=180")},
            {("CobraRoll", "CobraRoll", "")},
            {("HorseShoe", "HorseShoe", "")},
            {("PretzelKnot", "PretzelKnot", "")},
            {("PretzelCurve", "PretzelCurve", "")},

        };
    }

    public List<string> GetTypeNames()
    {
        return new List<string>(_blueprintDict.Keys);
    }

    public Blueprint GetType(string type)
    {
        return _blueprintDict[type];
    }

    public List<(string, string, string)> GetStraitBps()
    {
        return _straitBps;
    }

    public List<(string, string, string)> GetCurveBps()
    {
        return _curveBps;
    }

    public List<(string, string, string)> GetTurnBps()
    {
        return _turnBps;
    }
}
