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
        };
    }

    public List<string> GetTypeNames()
    {
        return new List<string>() { "Lever", "Fall", "Loop", "Hill" };
    }

    public Blueprint GetType(string type)
    {
        return _blueprintDict[type];
    }
}
