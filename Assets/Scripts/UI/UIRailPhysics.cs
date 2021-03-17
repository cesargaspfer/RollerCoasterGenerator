using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UIRailPhysics : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Transform _valuesPannel;
    #pragma warning disable 0649
    [SerializeField] private Transform _loadingPannel;
    private bool _isCarSimulating;

    private IEnumerator railCoroutine = null;
    private IEnumerator carCoroutine = null;

    public void Translate()
    {
        this.transform.GetChild(0).GetComponent<Text>().text = Translator.inst.GetTranslation("velocity") + ":";
        this.transform.GetChild(8).GetComponent<Text>().text = Translator.inst.GetTranslation("height") + ":";
        
        if(_isCarSimulating)
        {
            this.transform.GetChild(2).GetComponent<Text>().text = Translator.inst.GetTranslation("GVertical") + ":";
            this.transform.GetChild(4).GetComponent<Text>().text = Translator.inst.GetTranslation("GFrontal") + ":";
            this.transform.GetChild(6).GetComponent<Text>().text = Translator.inst.GetTranslation("GLateral") + ":";
            this.transform.GetChild(10).GetComponent<Text>().text = Translator.inst.GetTranslation("totalCarLength") + ":";
        }
        else
        {
            this.transform.GetChild(2).GetComponent<Text>().text = Translator.inst.GetTranslation("maxGVertical") + ":";
            this.transform.GetChild(4).GetComponent<Text>().text = Translator.inst.GetTranslation("maxGFrontal") + ":";
            this.transform.GetChild(6).GetComponent<Text>().text = Translator.inst.GetTranslation("maxGLateral") + ":";
            this.transform.GetChild(10).GetComponent<Text>().text = Translator.inst.GetTranslation("totalLength") + ":";
        }

        _loadingPannel.GetChild(0).GetComponent<Text>().text = Translator.inst.GetTranslation("calculating");
    }

    public void UpdateValues(RollerCoaster rc, bool isSimulating)
    {
        _loadingPannel.gameObject.SetActive(true);
        if (isSimulating)
        {
            if (!_isCarSimulating)
            {
                _isCarSimulating = true;
                if(railCoroutine != null)
                    StopCoroutine(railCoroutine);
            }
            if (carCoroutine != null)
                StopCoroutine(carCoroutine);
            carCoroutine = CarSimulationValuesUpdate(rc);
            StartCoroutine(carCoroutine);
        }
        else
        {
            if(_isCarSimulating)
            {
                _isCarSimulating = false;
                if (carCoroutine != null)
                    StopCoroutine(carCoroutine);
            }
            if(railCoroutine != null)
                StopCoroutine(railCoroutine);
            railCoroutine = RailSimulationValuesUpdate(rc);
            StartCoroutine(railCoroutine);
        }
        
        Translate();
    }

    private IEnumerator RailSimulationValuesUpdate(RollerCoaster rc)
    {
        RailPhysics railPhysics = rc.GetLastRailPhysics();
        while(railPhysics == null || railPhysics.Final == null)
        {
            yield return null;
            railPhysics = rc.GetLastRailPhysics();
        }
        if(!_isCarSimulating)
        {
            _loadingPannel.gameObject.SetActive(false);
            _valuesPannel.GetChild(1).GetComponent<Text>().text = railPhysics.Final.Velocity.ToString("0.0") + "<size=24>m/s</size>";
            _valuesPannel.GetChild(3).GetComponent<Text>().text = railPhysics.Max.GForce.y.ToString("0.0") + "<size=24>g</size>";
            _valuesPannel.GetChild(5).GetComponent<Text>().text = railPhysics.Max.GForce.x.ToString("0.0") + "<size=24>g</size>";
            _valuesPannel.GetChild(7).GetComponent<Text>().text = railPhysics.Max.GForce.z.ToString("0.0") + "<size=24>g</size>";
            _valuesPannel.GetChild(9).GetComponent<Text>().text = rc.GetFinalPosition().y.ToString("0.0") + "<size=28>m</size>";
            _valuesPannel.GetChild(11).GetComponent<Text>().text = (rc.GetTotalLength() + rc.GetCurrentGlobalrp().Length).ToString("0.0") + "<size=28>m</size>";
        }
    }

    private IEnumerator CarSimulationValuesUpdate(RollerCoaster rc)
    {
        _loadingPannel.gameObject.SetActive(false);
        while(true)
        {
            if(rc.GetFirstCar() == null) break;
            
            Car car = rc.GetFirstCar().GetComponent<Car>();
            float height = rc.GetFirstCar().transform.position.y;

            _valuesPannel.GetChild(1).GetComponent<Text>().text = car.Velocity.ToString("0.0") + "<size=24>m/s</size>";
            _valuesPannel.GetChild(3).GetComponent<Text>().text = car.GForce.y.ToString("0.0")  + "<size=24>g</size>";
            _valuesPannel.GetChild(5).GetComponent<Text>().text = car.GForce.x.ToString("0.0")  + "<size=24>g</size>";
            _valuesPannel.GetChild(7).GetComponent<Text>().text = car.GForce.z.ToString("0.0")  + "<size=24>g</size>";
            _valuesPannel.GetChild(9).GetComponent<Text>().text = height.ToString("0.0")  + "<size=28>m</size>";
            _valuesPannel.GetChild(11).GetComponent<Text>().text = car.TotalPosition.ToString("0.0") + "<size=28>m</size>";
            yield return null;
        }
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
