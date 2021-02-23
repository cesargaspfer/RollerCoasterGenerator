using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] public RollerCoaster _rollerCoaster;
    #pragma warning disable 0649
    [SerializeField] public InputField[] _railProps;
    [SerializeField] public Dropdown _railTypeDropdown;

    public void RemoveRailButtonClicked()
    {
        (RailProps rp, ModelProps mp) = _rollerCoaster.RemoveLastRail();
        if (rp == null)
            // TODO: Warn player that he can't remove rail
            return;
        _railProps[0].text = "" + Rad2DegRound(rp.Elevation);
        _railProps[1].text = "" + Rad2DegRound(rp.Rotation);
        _railProps[2].text = "" + Rad2DegRound(rp.Inclination);
        _railProps[3].text = "" + rp.Length;

        _railTypeDropdown.value = (int) mp.Type;
    }
    
    public void UpdateRailElevation(string elevation)
    {
        float convertedString = -1;
        try
        {
            convertedString = float.Parse(elevation);
        }
        catch
        {
            return;
        }
        _rollerCoaster.UpdateLastRail(elevation: convertedString * Mathf.PI / 180f);
    }

    public void UpdateRailRotation(string rotation)
    {
        float convertedString = -1;
        try
        {
            convertedString = float.Parse(rotation);
        }
        catch
        {
            return;
        }
        _rollerCoaster.UpdateLastRail(rotation: convertedString * Mathf.PI / 180f);
    }

    public void UpdateRailInclination(string inclination)
    {
        float convertedString = -1;
        try
        {
            convertedString = float.Parse(inclination);
        }
        catch
        {
            return;
        }
        _rollerCoaster.UpdateLastRail(inclination: -convertedString * Mathf.PI / 180f);
    }

    public void UpdateRailLength(string length)
    {
        int convertedString = -1;
        try
        {
            convertedString = int.Parse(length);
        }
        catch
        {
            return;
        }
        _rollerCoaster.UpdateLastRail(length: convertedString);
    }

    public void UpdateRailType(int type)
    {
        _rollerCoaster.UpdateLastRail(railType: type);
    }

    private string Rad2DegRound(float angle)
    {
        float rad = angle * 180f / Mathf.PI;
        rad = Mathf.Round(rad);
        return "" + rad;
    }
}
