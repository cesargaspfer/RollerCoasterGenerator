using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator : MonoBehaviour
{
    private static Translator _inst;

    public static Translator inst
    {
        get
        {
            //If _inst hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.

            if (_inst == null)
                _inst = GameObject.FindObjectOfType<Translator>();
            return _inst;
        }
    }

    // Translations Available
    private string[] _availableLanguages = new string[] { "English", "Portuguese" };

    // Awake is called before Start
    void Awake()
    {
        if (!PlayerPrefs.HasKey("language"))
        {
            string systemLanguage = Application.systemLanguage.ToString();
            bool hasLanguage = false;
            for(int i = 0; i < _availableLanguages.Length; i++)
            {
                if(_availableLanguages[i].Equals(systemLanguage))
                {
                    PlayerPrefs.SetInt("language", i);
                    hasLanguage = true;
                }
            }
            if (!hasLanguage)
            {
                PlayerPrefs.SetInt("language", 0);
            }
        }
        Translate();
    }

    public void Translate()
    {
        string language = _availableLanguages[PlayerPrefs.GetInt("language", 0)];
        Dictionary<string, string> translations = ParseToDictionary(language);
        Debug.Log(translations["teste"]);
    }

    // Read data from file
    private static Dictionary<string, string> ParseToDictionary(string language)
    {

        char lineSeperater = '\n'; // It defines line seperate character
        char fieldSeperator = '|'; // It defines field seperate chracte
        string path = "Translations/" + language; // Translations' path

        // List of dictionaries of the parsed data
        Dictionary<string, string> parsedDataDict = new Dictionary<string, string>();

        // Get all files of directory
        var info = Resources.LoadAll<TextAsset>(path);

        // Foreach file
        foreach (var f in info)
        {

            // Read file content
            string text = f.text;

            // TODO: Remove ''\r'
            text = text.Replace("\r", "");

            // Split the lines and colluns
            string[] records = text.Split(lineSeperater);

            // Foreach Line
            for (int i = 0; i < records.Length; i++)
            {

                // Read Line
                string record = records[i];

                // Ignore empty lines
                if (record.Equals(""))
                {
                    continue;
                }

                // Parse
                string[] fields = record.Split(fieldSeperator);
                parsedDataDict.Add(fields[0], fields[1]);
            }
        }

        // Return translation dictionary
        return (parsedDataDict);
    }
}
