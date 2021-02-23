using UnityEngine;
using static CarsManager;

[System.Serializable]
public class Car : MonoBehaviour
{
    [System.Serializable]
    public struct CarProps
    {
        private int _id;
        private float _length;
        private bool _defined;

        public CarProps(int id, float length)
        {
            _id = id;
            _length = length;
            _defined = true;
        }

        public int Id
        {
            get { return _id; }
        }

        public float Length
        {
            get { return _length; }
        }

        public bool IsDefined
        {
            get { return _defined; }
        }
    }

    [SerializeField] protected CarProps _carProps;
    [SerializeField] protected float _scalarPosition;
    [SerializeField] protected float _velocity;
    [SerializeField] protected int _currentSegment;
    [SerializeField] protected int _currentLap;
    [SerializeField] protected float _currentCurveT;

    public void Initialize(float scalarPosition, int currentSegment, CarProps carProps)
    {
        _scalarPosition = scalarPosition;
        _velocity = 0f;
        _currentSegment = currentSegment;
        _currentLap = 0;
        _currentCurveT = 0;
        _carProps = carProps;
    }

    public void UpdatePhysics(float scalarPosition, float velocity, int currentSegment, int currentLap, float currentCurveT)
    {
        _scalarPosition = scalarPosition;
        _velocity = velocity;
        _currentSegment = currentSegment;
        _currentLap = currentLap;
        _currentCurveT = currentCurveT;
    }

    public void Transform(Vector3 position, Quaternion rotation)
    {
        this.transform.position = position;
        this.transform.rotation = rotation;
    }

    public GameObject instantiatedObject
    {
        get { return this.gameObject; }
    }

    public CarProps CarProperties
    {
        get { return _carProps; }
    }

    public float ScalarPosition
    {
        get { return _scalarPosition; }
    }

    public float Velocity
    {
        get { return _velocity; }
    }

    public int CurrentSegment
    {
        get { return _currentSegment; }
    }

    public int CurrentLap
    {
        get { return _currentLap; }
    }

    public float CurrentCurveT
    {
        get { return _currentCurveT; }
    }
}
