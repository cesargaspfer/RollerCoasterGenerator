using UnityEngine;

[System.Serializable]
public class CarsManager : MonoBehaviour
{
    private static CarsManager _inst;
    public static CarsManager inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<CarsManager>();
            return _inst;
        }
    }

    public enum CarType
    {
        First,
        Middle,
        Last,
    };

    #pragma warning disable 0649
    [SerializeField] private Transform[] _cars;
    private Car.CarProps[] _carsProps;

    public Transform GetCarPrefab(int id, CarType type)
    {
        return _cars[id * 3 + (int) type];
    }

    public Car.CarProps GetCarProps(int id)
    {
        if(_carsProps == null || _carsProps.Length == 0)
        {
            _carsProps = new Car.CarProps[1]
            {
                new Car.CarProps(0, 1.3f),
            };
        }
        return _carsProps[id];
    }
}
