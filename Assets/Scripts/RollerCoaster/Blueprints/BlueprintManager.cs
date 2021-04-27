using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintManager
{
    
    private Dictionary<string, Blueprint> _blueprintDict;
    public BlueprintManager()
    {
        _blueprintDict = new Dictionary<string, Blueprint>()
        {
            { "Lever", new BlueprintLever() },
            { "Fall", new BlueprintFall() },
            { "Loop", new BlueprintLoop() },
            { "Hill", new BlueprintHill() },
            { "Curve", new BlueprintCurve() },
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
}
