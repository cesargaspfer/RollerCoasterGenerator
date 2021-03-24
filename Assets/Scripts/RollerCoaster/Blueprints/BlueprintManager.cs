using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintManager : MonoBehaviour
{
    
    private Blueprint _lever;
    private Blueprint _loop;

    void Awake()
    {
        _lever = new BlueprintLever();
        _loop = new BlueprintLoop();
    }

    public List<string> GetTypeNames()
    {
        return new List<string>() { "Lever", "Loop" };
    }

    public Blueprint GetType(string type)
    {
        switch (type)
        {
            case "Lever":
                return _lever;
            case "Loop":
                return _loop;
            default:
                Debug.LogError("Type not found in BlueprintManager.GetType");
                return null;
        }
    }
    
    public Blueprint Lever
    {
        get { return _lever; }
    }

    public Blueprint Loop
    {
        get { return _loop; }
    }
}
