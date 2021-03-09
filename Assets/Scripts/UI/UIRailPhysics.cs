using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIRailPhysics : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Transform _valuesPannel;
    private bool _isCarSimulating;

    public void UpdateValues(RollerCoaster rc, bool isSimulating)
    {
        if (isSimulating)
        {
            // TODO
            _isCarSimulating = true;
            StartCoroutine(CarSimulationValuesUpdate(rc));
        }
        else
        {
            if(_isCarSimulating)
            {
                _isCarSimulating = false;
                StopCoroutine(CarSimulationValuesUpdate(rc));
            }

            RailPhysics railPhysics = rc.GetLastRailPhysics();

            // TODO: Load Async
            if(railPhysics != null && railPhysics.Final != null)
            {
                _valuesPannel.GetChild(1).GetComponent<Text>().text = Mathf.Round(railPhysics.Final.Velocity * 10f) / 10f + "<size=24>k/h</size>";
                _valuesPannel.GetChild(3).GetComponent<Text>().text = Mathf.Round(railPhysics.Max.GForce.y * 10f) / 10f + "<size=24>g</size>";
                _valuesPannel.GetChild(5).GetComponent<Text>().text = Mathf.Round(railPhysics.Max.GForce.x * 10f) / 10f + "<size=24>g</size>";
                _valuesPannel.GetChild(7).GetComponent<Text>().text = Mathf.Round(railPhysics.Max.GForce.z * 10f) / 10f + "<size=24>g</size>";
                _valuesPannel.GetChild(9).GetComponent<Text>().text = Mathf.Round(rc.GetFinalPosition().y * 10f) / 10f + "<size=28>m</size>";
                _valuesPannel.GetChild(11).GetComponent<Text>().text = Mathf.Round((rc.GetTotalLength() + rc.GetCurrentGlobalrp().Length) * 10f) / 10f + "<size=28>m</size>";
            }
        }
    }

    private IEnumerator CarSimulationValuesUpdate(RollerCoaster rc)
    {
        while(true)
        {
            if(rc.GetFirstCar() == null) break;
            
            Car car = rc.GetFirstCar().GetComponent<Car>();
            float height = rc.GetFirstCar().transform.position.y;

            _valuesPannel.GetChild(1).GetComponent<Text>().text = Mathf.Round(car.Velocity * 10f) / 10f + "<size=24>k/h</size>";
            _valuesPannel.GetChild(3).GetComponent<Text>().text = Mathf.Round(car.GForce.y * 10f) / 10f + "<size=24>g</size>";
            _valuesPannel.GetChild(5).GetComponent<Text>().text = Mathf.Round(car.GForce.x * 10f) / 10f + "<size=24>g</size>";
            _valuesPannel.GetChild(7).GetComponent<Text>().text = Mathf.Round(car.GForce.z * 10f) / 10f + "<size=24>g</size>";
            _valuesPannel.GetChild(9).GetComponent<Text>().text = Mathf.Round(height * 10f) / 10f + "<size=28>m</size>";
            _valuesPannel.GetChild(11).GetComponent<Text>().text = Mathf.Round(car.ScalarPosition * 10f) / 10f + "<size=28>m</size>";
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
