using UnityEngine;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Toggle _vSyncToggle;
    #pragma warning disable 0649
    [SerializeField] private Toggle _soundsToggle;
    #pragma warning disable 0649
    [SerializeField] private Toggle _dynamicArrowsToggle;
    #pragma warning disable 0649
    [SerializeField] private Dropdown _languageDropdown;

    public void Initialize()
    {
        _vSyncToggle.isOn = PlayerPrefs.GetInt("vSync", 1) == 1;
        _soundsToggle.isOn = PlayerPrefs.GetInt("sounds", 1) == 1;
        _dynamicArrowsToggle.isOn = PlayerPrefs.GetInt("dynamicArrows", 1) == 1;
        _languageDropdown.value = PlayerPrefs.GetInt("language", 0);

        SetVSync(_vSyncToggle.isOn);
        SetSounds(_soundsToggle.isOn);
        SetDynamicArrows(_dynamicArrowsToggle.isOn);
        ChangeLanguage(_languageDropdown.value);
    }

    public void SetVSync(bool value)
    {
        if(value)
        {
            QualitySettings.vSyncCount = 1;
            PlayerPrefs.SetInt("vSync", 1);
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            PlayerPrefs.SetInt("vSync", 0);
        }
    }

    public void SetSounds(bool value)
    {
        if (value)
        {
            PlayerPrefs.SetInt("sounds", 1);
        }
        else
        {
            PlayerPrefs.SetInt("sounds", 0);
        }
    }

    public void SetDynamicArrows(bool value)
    {
        if (value)
        {
            PlayerPrefs.SetInt("dynamicArrows", 1);
        }
        else
        {
            PlayerPrefs.SetInt("dynamicArrows", 0);
        }
    }

    public void ChangeLanguage(int value)
    {
        if(value == PlayerPrefs.GetInt("language", 0)) return;
        PlayerPrefs.SetInt("language", value);
        Translator.inst.Translate();
    }
}
