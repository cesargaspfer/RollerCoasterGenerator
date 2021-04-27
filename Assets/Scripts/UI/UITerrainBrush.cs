using UnityEngine;
using UnityEngine.UI;

public class UITerrainBrush : MonoBehaviour
{
    private static UITerrainBrush _inst;
    public static UITerrainBrush inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<UITerrainBrush>();
            return _inst;
        }
    }

    #pragma warning disable 0649
    [SerializeField] private Text[] _valueText;
    #pragma warning disable 0649
    [SerializeField] private Animator _elevateStateButtons;

    // min, intercalation, max, current
    [SerializeField] private float[] _radius;
    [SerializeField] private float[] _intencity;
    [SerializeField] private float[] _opacity;
    [SerializeField] private bool _elevate;

    void Start()
    {
        _radius = new float[4]{5f, 1f, 20f, 5f};
        _intencity = new float[4]{1f, 1f, 10f, 2f};
        _opacity = new float[4]{0f, 0.1f, 1f, 1f};
        _elevate = true;

        _valueText[0].text = "" + _radius[3] + "<size=24>m</size>";
        _valueText[1].text = "" + _intencity[3];
        if(_opacity[3] == 1)
            _valueText[2].text = "" + _opacity[3];
        else
            _valueText[2].text = "0." + (int)(_opacity[3] * 10f) / 10;

        TerrainBrush.inst.UpdateValues(_radius[3], (_elevate ? _intencity[3] : - _intencity[3]) * 20f, _opacity[3]);
    }

    public void IncreaseValue(int id)
    {
        if (id == 0)
        {
            _radius[3] += _radius[1];
            if (_radius[3] > _radius[2]) _radius[3] = _radius[2];
            if (_radius[3] < _radius[0]) _radius[3] = _radius[0];
            _valueText[0].text = "" + _radius[3] + "<size=24>m</size>";
        }
        else if (id == 1)
        {
            _intencity[3] += _intencity[1];
            if (_intencity[3] > _intencity[2]) _intencity[3] = _intencity[2];
            if (_intencity[3] < _intencity[0]) _intencity[3] = _intencity[0];
            _valueText[1].text = "" + _intencity[3];
        }
        else
        {
            _opacity[3] += _opacity[1];
            if (_opacity[3] > _opacity[2]) _opacity[3] = _opacity[2];
            if (_opacity[3] < _opacity[0]) _opacity[3] = _opacity[0];
            if(_opacity[3] == 1)
                _valueText[2].text = "" + _opacity[3];
            else
                _valueText[2].text = "0." + Mathf.RoundToInt(_opacity[3] * 10f);
        }
        TerrainBrush.inst.UpdateValues(_radius[3], (_elevate ? _intencity[3] : - _intencity[3]) * 20f, _opacity[3]);
    }

    public void DecreaseValue(int id)
    {
        if (id == 0)
        {
            _radius[3] -= _radius[1];
            if (_radius[3] > _radius[2]) _radius[3] = _radius[2];
            if (_radius[3] < _radius[0]) _radius[3] = _radius[0];
            _valueText[0].text = "" + _radius[3] + "<size=24>m</size>";
        }
        else if (id == 1)
        {
            _intencity[3] -= _intencity[1];
            if (_intencity[3] > _intencity[2]) _intencity[3] = _intencity[2];
            if (_intencity[3] < _intencity[0]) _intencity[3] = _intencity[0];
            _valueText[1].text = "" + _intencity[3];
        }
        else
        {
            _opacity[3] -= _opacity[1];
            if (_opacity[3] > _opacity[2]) _opacity[3] = _opacity[2];
            if (_opacity[3] < _opacity[0]) _opacity[3] = _opacity[0];
            if(_opacity[3] == 1)
                _valueText[2].text = "" + _opacity[3];
            else
                _valueText[2].text = "0." + Mathf.RoundToInt(_opacity[3] * 10f);
        }
        TerrainBrush.inst.UpdateValues(_radius[3], (_elevate ? _intencity[3] : - _intencity[3]) * 20f, _opacity[3]);
    }

    public void ChangeElevateState(bool newState)
    {
        if(_elevate == newState)
            return;
        _elevate = newState;
        if(_elevate)
            _elevateStateButtons.Play("ToElevate");
        else
            _elevateStateButtons.Play("ToLower");
        TerrainBrush.inst.UpdateValues(_radius[3], (_elevate ? _intencity[3] : -_intencity[3]) * 20f, _opacity[3]);
    }

    public void Active()
    {
        TerrainBrush.inst.Active();
    }

    public void Deactivate()
    {
        TerrainBrush.inst.Deactivate();
    }
}
