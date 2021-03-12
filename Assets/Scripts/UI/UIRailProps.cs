using UnityEngine;
using UnityEngine.UI;

public class UIRailProps : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Transform _localPropsParent;
    #pragma warning disable 0649
    [SerializeField] private Transform _globalPropsParent;
    #pragma warning disable 0649
    [SerializeField] private Dropdown _railTypeDropdown;

    public void UpdateValues(RollerCoaster rc)
    {
        RailProps currentrp = rc.GetCurrentGlobalrp();
        RailProps localrp = currentrp - rc.GetLastGlobalrp();

        _globalPropsParent.GetChild(1).GetComponent<Text>().text = AdjustAngle(currentrp.Elevation) + "°";
        _globalPropsParent.GetChild(3).GetComponent<Text>().text = AdjustAngle(currentrp.Rotation) + "°";
        _globalPropsParent.GetChild(5).GetComponent<Text>().text = AdjustAngle(-currentrp.Inclination) + "°";
        _globalPropsParent.GetChild(7).GetComponent<Text>().text = currentrp.Length.ToString("0.0") + "<size=28>m</size>";

        _localPropsParent.GetChild(1).GetComponent<Text>().text = AdjustAngle(localrp.Elevation) + "°";
        _localPropsParent.GetChild(3).GetComponent<Text>().text = AdjustAngle(localrp.Rotation) + "°";
        _localPropsParent.GetChild(5).GetComponent<Text>().text = AdjustAngle(-localrp.Inclination) + "°";
        _localPropsParent.GetChild(7).GetComponent<Text>().text = localrp.Length.ToString("0.0") + "<size=28>m</size>";

        _railTypeDropdown.value = (int) rc.GetLastRail().mp.Type;
    }

    private int AdjustAngle(float angle)
    {
        int angleInt = Mathf.RoundToInt(angle * 180f / Mathf.PI);
        angleInt -= (angleInt / 360) * 360;
        if (angleInt > 180)
            angleInt -= 360;
        if (angleInt < -180)
            angleInt += 360;
        return angleInt;
    }
}
