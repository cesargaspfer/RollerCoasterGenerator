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
            { "Plataform", new BlueprintPlataform() },
            
        };

        _straitBps = new List<(string, string, string)>()
        {
            {("Loop", "Straight", "")},
            {("Hill", "StraightLength", "")},
            // {("Hill", "StraightLength", "")},
            {("Corkscrew", "Straight", "")},
            {("Straight", "Straight", "")},
            {("Lever", "Straight", "")},
            {("Fall", "Straight", "")},

        };

        _curveBps = new List<(string, string, string)>()
        {
            {("Curve", "Curve", "rotation=90")},
            {("Hill", "RotateLength", "rotation=90")},
            // {("Hill", "RotateLength", "rotation=90")},
            {("Sidewinder", "Sidewinder", "")},
            {("Lever", "Rotate", "rotation=30;pieces=3")},
            {("Fall", "Rotate", "rotation=30;pieces=3")},

        };

        _turnBps = new List<(string, string, string)>()
        {
            {("Curve", "Curve", "rotation=180")},
            {("Hill", "RotateLength", "rotation=180")},
            // {("Hill", "RotateLength", "rotation=180")},
            {("CobraRoll", "CobraRoll", "")},
            {("HorseShoe", "HorseShoe", "")},
            {("PretzelKnot", "PretzelKnot", "")},
            {("PretzelCurve", "PretzelCurve", "")},
            {("Lever", "Rotate", "rotation=60;pieces=3")},
            {("Fall", "Rotate", "rotation=60;pieces=3")},

        };
    }

    public List<string> GetElementNames()
    {
        return new List<string>(_blueprintDict.Keys);
    }

    public Blueprint GetElement(string element)
    {
        return _blueprintDict[element];
    }

    public List<(string, string, string)> GetElementsByType(string type)
    {
        if(type.Equals("Straight"))
            return _straitBps;
        else if (type.Equals("Curve"))
            return _curveBps;
        else
            return _turnBps;
    }
}
