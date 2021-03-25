using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UIBlueprintProp : MonoBehaviour
{
    [SerializeField] private string _keyName;
    [SerializeField] private float _intercalationValue;
    [SerializeField] private float _minValue;
    [SerializeField] private float _currentValue;
    [SerializeField] private float _maxValue;
    [SerializeField] private string _unity;


    public void Initialize(string name, string valueProps)
    {
        _keyName = name;
        string[] splitedValueProps = valueProps.Split(';');
        _intercalationValue = float.Parse(splitedValueProps[0], CultureInfo.InvariantCulture.NumberFormat);
        _minValue = float.Parse(splitedValueProps[1], CultureInfo.InvariantCulture.NumberFormat);
        _maxValue = float.Parse(splitedValueProps[2], CultureInfo.InvariantCulture.NumberFormat);
        _currentValue = float.Parse(splitedValueProps[3], CultureInfo.InvariantCulture.NumberFormat);
        if(splitedValueProps[4].Equals("°"))
            _unity = splitedValueProps[4];
        else
            _unity = "<size=24>" + splitedValueProps[4] + "</size>";

        this.transform.GetChild(2).GetComponent<Text>().text = _currentValue + _unity;

        Translate();
    }

    public void Translate()
    {
        this.transform.GetChild(0).GetComponent<Text>().text = Translator.inst.GetTranslation(_keyName) + ":";
    }

    public void AddValue()
    {
        if(_currentValue + _intercalationValue <= _maxValue)
            UpdateValue(_currentValue + _intercalationValue);
    }
    public void SubValue()
    {
        if (_currentValue - _intercalationValue >= _minValue)
            UpdateValue(_currentValue - _intercalationValue);
    }

    private void UpdateValue(float value)
    {
        _currentValue = value;
        this.transform.GetChild(2).GetComponent<Text>().text = value + _unity;
        UIBlueprint.inst.UpdateValues();
    }

    public string KeyName
    {
        get { return _keyName; }
    }

    public float Value
    {
        get { return _currentValue; }
    }
}
