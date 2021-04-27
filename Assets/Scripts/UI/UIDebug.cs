using UnityEngine;
using UnityEngine.UI;

public class UIDebug : MonoBehaviour
{
    private static UIDebug _inst;
    public static UIDebug inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<UIDebug>();
            return _inst;
        }
    }

    public void ShowDebugMessage(string message)
    {
        this.transform.GetChild(0).GetComponent<Text>().text = message;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if ((type == LogType.Error || type == LogType.Exception || type == LogType.Warning))
        {
            ShowDebugMessage(type + "\n" + logString + "\n" + stackTrace);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            this.transform.GetChild(0).GetComponent<Text>().text = "";
        }
    }
    void Start()
    {
        // this.transform.GetChild(0).GetComponent<Text>().text = Application.dataPath;
        // this.transform.GetChild(1).GetComponent<Text>().text = Application.persistentDataPath;
    }
}
