using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Translator : MonoBehaviour
{
    private static Translator _inst;

    public static Translator inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<Translator>();
            return _inst;
        }
    }

    // UI components

    #pragma warning disable 0649
    [SerializeField] private Transform _topPannel;
    #pragma warning disable 0649
    [SerializeField] private Transform _railProps;
    #pragma warning disable 0649
    [SerializeField] private Transform _simulatorProps;
    #pragma warning disable 0649
    [SerializeField] private Transform _constructorButtons;
    #pragma warning disable 0649
    [SerializeField] private Transform _simulatorButtons;
    #pragma warning disable 0649
    [SerializeField] private Transform _blueprintsTypeSelector;
    #pragma warning disable 0649
    [SerializeField] private Transform _blueprintsButtons;
    #pragma warning disable 0649
    [SerializeField] private Transform _generatorButton;
    #pragma warning disable 0649
    [SerializeField] private Transform _terrain;
    #pragma warning disable 0649
    [SerializeField] private Transform _menuButtons;
    #pragma warning disable 0649
    [SerializeField] private Transform _pauseButtons;
    #pragma warning disable 0649
    [SerializeField] private Transform _menuBackButton;
    #pragma warning disable 0649
    [SerializeField] private Transform _menuSelect;
    #pragma warning disable 0649
    [SerializeField] private Transform _menuOptions;
    #pragma warning disable 0649
    [SerializeField] private Transform _menuCredits;
    #pragma warning disable 0649
    [SerializeField] private Transform _menuSave;
    #pragma warning disable 0649
    [SerializeField] private Transform _menuLoad;



    // Translations Available
    private string[] _availableLanguages = new string[] { "English", "Portuguese" };
    private Dictionary<string, string> _translations;

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
        _translations = ParseToDictionary(language);

        // ------------------------------------- Main Pannel ------------------------------------- //

        UIManager.inst.TranslateLegend();

        _topPannel.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["constructor"];
        _topPannel.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["terrain"];

        _railProps.GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["railType"] + ":";
        _railProps.GetChild(0).GetChild(1).GetComponent<Dropdown>().options[0].text = _translations["rtPlataform"];
        _railProps.GetChild(0).GetChild(1).GetComponent<Dropdown>().options[1].text = _translations["rtNormal"];
        _railProps.GetChild(0).GetChild(1).GetComponent<Dropdown>().options[2].text = _translations["rtLever"];
        _railProps.GetChild(0).GetChild(1).GetComponent<Dropdown>().options[3].text = _translations["rtBrakes"];

        _railProps.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["global"];
        _railProps.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["local"];

        for(int i = 0; i < 2; i++)
        {
            _railProps.GetChild(1).GetChild(2 + i).GetChild(0).GetComponent<Text>().text = _translations["elevation"] + ":";
            _railProps.GetChild(1).GetChild(2 + i).GetChild(2).GetComponent<Text>().text = _translations["rotation"] + ":";
            _railProps.GetChild(1).GetChild(2 + i).GetChild(4).GetComponent<Text>().text = _translations["inclination"] + ":";
            _railProps.GetChild(1).GetChild(2 + i).GetChild(6).GetComponent<Text>().text = _translations["length"] + ":";
        }

        _simulatorProps.GetChild(0).GetComponent<UIRailPhysics>().Translate();

        _simulatorProps.GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["heatmap"] + ":";

        // TODO: Translate heatmap's options
        _simulatorProps.GetChild(1).GetChild(1).GetComponent<Dropdown>().options[0].text = _translations["none"];
        _simulatorProps.GetChild(1).GetChild(1).GetComponent<Dropdown>().options[1].text = _translations["velocity"];
        _simulatorProps.GetChild(1).GetChild(1).GetComponent<Dropdown>().options[2].text = _translations["GVertical"];
        _simulatorProps.GetChild(1).GetChild(1).GetComponent<Dropdown>().options[3].text = _translations["GFrontal"];
        _simulatorProps.GetChild(1).GetChild(1).GetComponent<Dropdown>().options[4].text = _translations["GLateral"];
        _simulatorProps.GetChild(1).GetChild(1).GetComponent<Dropdown>().options[5].text = _translations["height"];

        _constructorButtons.GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["addRail"];
        _constructorButtons.GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["blueprints"];
        _constructorButtons.GetChild(2).GetChild(0).GetComponent<Text>().text = _translations["autocomplete"];
        _constructorButtons.GetChild(3).GetChild(0).GetComponent<Text>().text = _translations["removeRail"];

        _simulatorButtons.GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["simulate"];

        _simulatorButtons.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["changeCamera"];
        _simulatorButtons.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["stopSimulation"];

        _blueprintsButtons.GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["addBlueprint"];
        _blueprintsButtons.GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["cancelBlueprint"];

        _blueprintsTypeSelector.GetChild(0).GetComponent<Text>().text = _translations["type"] + ":";
        _blueprintsTypeSelector.GetChild(2).GetComponent<Text>().text = _translations["subtype"] + ":";

        UIBlueprint.inst.Translate();

        _generatorButton.GetChild(0).GetComponent<Text>().text = _translations["generate"];

        _terrain.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["relief"];
        _terrain.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["decoration"];

        _terrain.GetChild(2).GetChild(0).GetComponent<Text>().text = _translations["brushProps"];
        _terrain.GetChild(2).GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["radius"];
        _terrain.GetChild(2).GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["intensity"];
        _terrain.GetChild(2).GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = _translations["opacity"];
        _terrain.GetChild(2).GetChild(2).GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["elevate"];
        _terrain.GetChild(2).GetChild(2).GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["lower"];

        _terrain.GetChild(3).GetChild(0).GetComponent<Text>().text = _translations["selectAnObject"];
        _terrain.GetChild(3).GetChild(3).GetChild(0).GetComponent<Text>().text = _translations["deselect"];

        // ------------------------------------- Menu / Pause ------------------------------------- //

        _menuButtons.GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["menuConstruct"];
        _menuButtons.GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["menuGenerate"];
        _menuButtons.GetChild(2).GetChild(0).GetComponent<Text>().text = _translations["menuLoad"];
        _menuButtons.GetChild(3).GetChild(0).GetComponent<Text>().text = _translations["options"];
        _menuButtons.GetChild(4).GetChild(0).GetComponent<Text>().text = _translations["credits"];
        _menuButtons.GetChild(5).GetChild(0).GetComponent<Text>().text = _translations["exit"];

        _pauseButtons.GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["pauseReturn"];
        _pauseButtons.GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["menuSave"];
        _pauseButtons.GetChild(2).GetChild(0).GetComponent<Text>().text = _translations["menuLoad"];
        _pauseButtons.GetChild(3).GetChild(0).GetComponent<Text>().text = _translations["options"];
        _pauseButtons.GetChild(4).GetChild(0).GetComponent<Text>().text = _translations["credits"];
        _pauseButtons.GetChild(5).GetChild(0).GetComponent<Text>().text = _translations["returnToMenu"];
        
        _menuBackButton.GetChild(0).GetComponent<Text>().text = _translations["back"];
        
        _menuSelect.GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["continue"];
        _menuSelect.GetChild(1).GetComponent<Text>().text = _translations["selectCoasterType"];

        //TODO: Translate roller coaster types
        _menuSelect.GetChild(2).GetComponent<Dropdown>().options[0].text = _translations["coasterTypeNormal"];


        _menuOptions.GetChild(0).GetComponent<Text>().text = _translations["options"];
        _menuOptions.GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["vSync"];
        _menuOptions.GetChild(2).GetChild(0).GetComponent<Text>().text = _translations["enableSound"];
        _menuOptions.GetChild(3).GetChild(0).GetComponent<Text>().text = _translations["dynamicArrows"];
        _menuOptions.GetChild(4).GetChild(0).GetComponent<Text>().text = _translations["selectLanguage"];

        _menuCredits.GetChild(0).GetComponent<Text>().text = _translations["creditsText"].Replace("<br>", "\n");

        _menuSave.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = _translations["continue"];
        _menuSave.GetChild(0).GetChild(1).GetComponent<Text>().text = _translations["save"];
        _menuSave.GetChild(0).GetChild(2).GetComponent<Text>().text = _translations["typeCoasterName"];
        _menuSave.GetChild(0).GetChild(4).GetComponent<Text>().text = _translations["coasterExists"];
        
        _menuSave.GetChild(1).GetChild(0).GetComponent<Text>().text = _translations["saveSuccess"];
        _menuSave.GetChild(2).GetChild(0).GetComponent<Text>().text = _translations["saveError"];

        _menuLoad.GetChild(0).GetComponent<Text>().text = _translations["load"];
        _menuLoad.GetChild(1).GetChild(3).GetChild(0).GetComponent<Text>().text = _translations["load"];
    }

    public string GetTranslation(string key)
    {
        return _translations[key];
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
