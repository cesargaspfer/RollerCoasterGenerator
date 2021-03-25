using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RailModelProperties;

public class UIBlueprint : MonoBehaviour
{
    private static UIBlueprint _inst;
    public static UIBlueprint inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<UIBlueprint>();
            return _inst;
        }
    }

    #pragma warning disable 0649
    [SerializeField] private Transform _blueprintsPropTransform;

    #pragma warning disable 0649
    [SerializeField] private Dropdown _typeDropdown;
    #pragma warning disable 0649
    [SerializeField] private Dropdown _subtypeDropdown;
    #pragma warning disable 0649
    [SerializeField] private Transform _propsContent;

    [SerializeField] private BlueprintManager _blueprintManager;
    [SerializeField] private RollerCoaster _rollerCoaster;

    [SerializeField] private List<string> _typeNames;
    [SerializeField] private List<string> _subtypeNames;
    [SerializeField] private Dictionary<string, Dictionary<string, string>> _blueprintParams;

    [SerializeField] private List<UIBlueprintProp> _UIBlueprintProps;
    [SerializeField] private Blueprint _currentBlueprint;
    [SerializeField] private int _currentType;
    [SerializeField] private int _currentSubtype;
    [SerializeField] private int _currentRailPreviewCount;


    [SerializeField] private RailProps _previousRailProps;
    [SerializeField] private int _previousRailType;


    [SerializeField] private bool _addedBlueprint;
    [SerializeField] private bool _isActive;

    public void Initialize(RollerCoaster rollerCoaster)
    {
        _isActive = true;
        _addedBlueprint = false;
        ClearProps();

        _rollerCoaster = rollerCoaster;
        _blueprintManager = _rollerCoaster.GetBlueprintManager();

        _currentType = 0;
        _currentSubtype = 0;
        _currentRailPreviewCount = 0;

        _typeNames = _blueprintManager.GetTypeNames();
        _currentBlueprint = _blueprintManager.GetType(_typeNames[0]);
        _subtypeNames = _currentBlueprint.GetSubtypeNames();
        _blueprintParams = _currentBlueprint.GetParams();

        Rail rail = _rollerCoaster.GetLastRail();
        _previousRailProps = _rollerCoaster.GetCurrentGlobalrp();
        _previousRailType = (int) rail.mp.Type;

        _rollerCoaster.RemoveLastRail(false);

        Translate();

        _currentType = -1;
        _currentSubtype = -1;
        ChangeType(0);

        UpdateValues();
    }

    public void Close()
    {
        RemoveRails();
        _rollerCoaster.AddRail(true);
        if(!_addedBlueprint)
        {
            _rollerCoaster.UpdateLastRail(
                elevation:_previousRailProps.Inclination, 
                rotation:_previousRailProps.Rotation,
                inclination:_previousRailProps.Inclination,
                length:_previousRailProps.Length,
                railType:_previousRailType
            );
        }
        else
        {
            _rollerCoaster.UpdateLastRail(
                length: _previousRailProps.Length,
                railType: 1
            );
        }
        UIManager.inst.UpdateUIValues();
        _isActive = false;
    }

    public void AddBlueprint()
    {
        int railsCount = _rollerCoaster.GetRailsCount();
        for (int i = 0; i < _currentRailPreviewCount; i++)
        {
            _rollerCoaster.SetRailPreview(railsCount - i - 1, false);
            _rollerCoaster.GenerateSupports(railsCount - i - 1);
        }
        _addedBlueprint = true;
        _currentRailPreviewCount = 0;
        UpdateValues();
    }

    public void Translate()
    {
        if(!_isActive) return;

        int currentType = _currentType;
        int currentSubtype = _currentSubtype;

        _typeDropdown.ClearOptions();
        _subtypeDropdown.ClearOptions();

        List<string> translatedtypeNames = new List<string>();
        for(int i = 0; i < _typeNames.Count; i++)
            translatedtypeNames.Add(Translator.inst.GetTranslation(_typeNames[i]));

        List<string> translatedsubtypeNames = new List<string>();
        for (int i = 0; i < _typeNames.Count; i++)
            translatedsubtypeNames.Add(Translator.inst.GetTranslation(_subtypeNames[i]));

        _typeDropdown.AddOptions(translatedtypeNames);
        _subtypeDropdown.AddOptions(translatedsubtypeNames);
        
        _typeDropdown.value = currentType;
        _subtypeDropdown.value = currentSubtype;

        TranslateProperties();
    }

    private void TranslateSubtypes()
    {
        int currentSubtype = _currentSubtype;
        _subtypeDropdown.ClearOptions();

        List<string> translatedsubtypeNames = new List<string>();
        for (int i = 0; i < _subtypeNames.Count; i++)
            translatedsubtypeNames.Add(Translator.inst.GetTranslation(_subtypeNames[i]));

        _subtypeDropdown.AddOptions(translatedsubtypeNames);
        _subtypeDropdown.value = currentSubtype;
    }

    private void TranslateProperties()
    {
        foreach(UIBlueprintProp prop in _UIBlueprintProps)
            prop.Translate();
    }

    public void ChangeType(int type)
    {
        if (_currentType == type) return;
        _currentType = type;
        _typeDropdown.value = type;

        _currentBlueprint = _blueprintManager.GetType(_typeNames[type]);
        _subtypeNames = _currentBlueprint.GetSubtypeNames();
        _blueprintParams = _currentBlueprint.GetParams();
        
        _currentSubtype = 0;
        TranslateSubtypes();
        _currentSubtype = -1;

        ChangeSubtype(0);
    }

    public void ChangeSubtype(int subtype)
    {
        if (_currentSubtype == subtype) return;
        _currentSubtype = subtype;
        _subtypeDropdown.value = subtype;
        InstantiateProps();
        UpdateValues();
    }

    public void InstantiateProps()
    {
        ClearProps();

        Dictionary<string, string> subTypeParams = _blueprintParams[_subtypeNames[_currentSubtype]];

        List<string> paramNames = new List<string>(subTypeParams.Keys);

        _propsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, 10f + paramNames.Count * 30.0f);
        for (int i = 0; i < paramNames.Count; i++)
        {
            Transform instantiated = Instantiate(_blueprintsPropTransform, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            instantiated.transform.SetParent(_propsContent);
            instantiated.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            instantiated.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
            instantiated.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, -i * 30 - 20.0f, 0.0f);

            UIBlueprintProp currentUIBlueprintProp = instantiated.GetComponent<UIBlueprintProp>();

            currentUIBlueprintProp.Initialize(paramNames[i], subTypeParams[paramNames[i]]);
            _UIBlueprintProps.Add(currentUIBlueprintProp);
        }
    }

    public void ClearProps()
    {
        if (_UIBlueprintProps == null)
        {
            _UIBlueprintProps = new List<UIBlueprintProp>();
        }
        else if (_UIBlueprintProps.Count > 0)
        {
            int count = _UIBlueprintProps.Count;
            for (int i = 0; i < count; i++)
            {
                GameObject.Destroy(_UIBlueprintProps[i].gameObject);
            }
            _UIBlueprintProps.Clear();
        }
    }

    public void UpdateValues()
    {
        RemoveRails();

        Dictionary<string, float> dict = new Dictionary<string, float>();
        foreach(UIBlueprintProp prop in _UIBlueprintProps)
        {
            dict.Add(prop.KeyName, prop.Value);
        }

        List<(RailProps, RailType)> rails = _currentBlueprint.GetBlueprint(_subtypeNames[_currentSubtype], dict);
        _currentRailPreviewCount = rails.Count;

        _rollerCoaster.AddBlueprint(rails, true);

        UIManager.inst.UpdateUIValues();
    }

    private void RemoveRails()
    {
        for (int i = 0; i < _currentRailPreviewCount; i++)
            _rollerCoaster.RemoveLastRail(false);
    }
}
