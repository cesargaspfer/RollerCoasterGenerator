using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static CarsManager;
using static RailModelProperties;

[System.Serializable]
public class Simulator
{

    // Simulator Properties
    private RollerCoaster _rollerCoaster;
    [SerializeField] private List<(Rail, RailPhysics)> _rails;
    [SerializeField] private Car[] _cars;
    [SerializeField] private float _dtres;
    [SerializeField] private RailPhysics _initialrp;

    // Car Properties
    [SerializeField] private int _carId;
    [SerializeField] private int _carQuantity;

    // Simulation Properties
    [SerializeField] private bool _isSimulating;

    public Simulator(RollerCoaster rollerCoaster,float deltaTimeResolution, int carId, int carQuantity)
    {
        _rollerCoaster = rollerCoaster;
        _dtres = deltaTimeResolution;
        _rails = new List<(Rail, RailPhysics)>();
        _carId = carId;
        _carQuantity = carQuantity;
        _isSimulating = false;
        RailPhysics.Props rpp = new RailPhysics.Props(0.01f, Vector3.zero);
        _initialrp = new RailPhysics(rpp);
        _initialrp.Max = rpp;
        _initialrp.Final = rpp;
    }

    // TODO
    public void ChangeCarId(int carId)
    {

    }

    // TODO
    public void ChangeCarQuantity(int carQuantity)
    {
        
    }

    // ------------------- Car Simulation ------------------- //

    public bool CanSimulateCar()
    {
        return (_rails.Count > 0 && _carQuantity > 0 && !_isSimulating);
    }

    public void StartCarSimulation()
    {
        _cars = new Car[_carQuantity];
        for (int i = 0; i < _cars.Length; i++)
        {
            CarType type = CarType.Middle;
            if (i == 0) type = CarType.First;
            else if (i == _cars.Length - 1) type = CarType.Last;

            _cars[i] = _rollerCoaster.InstantiateCar(_carId, type);
        }

        for (int i = 0; i < _cars.Length; i++)
        {
            float scalarPosition = _cars[i].CarProperties.Length * 0.5f + ((_cars.Length - 1) - i) * _cars[i].CarProperties.Length;
            (Rail lastRail, _) = _rails[0];
            (int currentSegment, float currentPos, float lastT, float distance) = CalculateSegment(0, scalarPosition, 0f, 0f);
            (Rail currentRail, _) = _rails[currentSegment];
            int currentLap = _cars[i].CurrentLap;
            if (currentRail.mp.Type == RailType.Platform && lastRail.mp.Type != RailType.Platform)
                currentLap++;

            Bezier curve = currentRail.sp.Curve;
            float currentCurveT = curve.GetNextT(lastT, distance);
            Vector3 vectorPos = curve.Sample(currentCurveT);
            Quaternion rotation = currentRail.GetQuaternionAt(currentCurveT);

            _cars[i].UpdatePhysics(currentPos, 0f, currentSegment, currentLap, currentCurveT);
            _cars[i].Transform(vectorPos, rotation);
        }

        _isSimulating = true;
    }

    public void SetpCarSimulation(float deltaTime)
    {
        float velocity = _cars[0].Velocity;
        
        float k1 = CalculateResultantAcceleration(0f, velocity);
        float k2 = CalculateResultantAcceleration(deltaTime * 0.5f, velocity + k1 * 0.5f);
        float k3 = CalculateResultantAcceleration(deltaTime * 0.5f, velocity + k2 * 0.5f);
        float k4 = CalculateResultantAcceleration(deltaTime, velocity + k3);

        float deltaResultantAcceleration = (deltaTime / 6f) * (k1 + 2f * k2 + 2f * k3 + k4);

        float newVelocity = velocity + deltaResultantAcceleration;

        this.UpdateCars(newVelocity, deltaTime);
    }

    private float CalculateResultantAcceleration(float dt, float velocity)
    {
        float acceleration = 0f;
        for(int i = 0; i < _cars.Length; i++)
        {
            (Rail rail, float curveT) = GetCarLocalRailPosition(_cars[i], dt * velocity);
            acceleration += CalculateAcceleration(rail, curveT, velocity, dt);
        }
        acceleration /= _cars.Length;
        return acceleration;
    }

    private float CalculateAcceleration(Rail rail, float curveT, float velocity, float dt)
    {
        // TODO: Define mass inside the car
        float mass = 1f;
        float acceleration = 0f;
        Bezier curve = rail.sp.Curve;
        Vector3 basisX = rail.GetBasisAt(curveT).GetColumn(0);
        Vector3 basisY = rail.GetBasisAt(curveT).GetColumn(1);

        if (rail.mp.Type != RailModelProperties.RailType.Lever||
           (rail.mp.Type == RailModelProperties.RailType.Lever && velocity > 3f))
            acceleration += Vector3.Dot(-Vector3.up, basisX) * 9.8f;

        if (rail.mp.Type == RailModelProperties.RailType.Lever)
        {
            if(velocity < 2f)
            {
                acceleration += 2f;
            }
        }

        if (rail.mp.Type == RailModelProperties.RailType.Platform)
        {
            if (velocity < 1f)
            {
                acceleration += 1f;
            }
        }

        if (rail.mp.Type == RailModelProperties.RailType.Brake)
        {
            if (velocity > 1f)
            {
                acceleration -= 5f;
            }
        }

        if(rail.mp.Type == RailModelProperties.RailType.Brake || rail.mp.Type == RailModelProperties.RailType.Normal ||
           (rail.mp.Type == RailModelProperties.RailType.Lever && velocity > 2f) ||
           (rail.mp.Type == RailModelProperties.RailType.Platform && velocity > 1f))
        {
            if (velocity > 0.07f)
            {
                acceleration -= Mathf.Sign(velocity) * 0.004f * Mathf.Max(Mathf.Abs(velocity), 10f) * (0.7f + 0.3f * Mathf.Abs(Vector3.Dot(Vector3.up, basisY))) * 9.8f;
            }
            else
            {
                float dragAcceleration = - Mathf.Sign(velocity) * 0.004f * 10f * (0.7f + 0.3f * Mathf.Abs(Vector3.Dot(Vector3.up, basisY))) * 9.8f;
                if(Mathf.Abs(2f * dragAcceleration) > Mathf.Abs(acceleration))
                {
                    acceleration = 0.5f * dragAcceleration;
                }
                else
                {
                    acceleration += dragAcceleration;
                }
            }
        }

        return acceleration / mass;
    }

    public void StopCarSimulation()
    {
        for (int i = 0; i < _cars.Length; i++)
        {
            Car tmpCar = _cars[i];
            _cars[i] = null;
            GameObject.Destroy(tmpCar.instantiatedObject);
        }
        _cars = new Car[0];
        _isSimulating = false;
    }

    private (Rail, float) GetCarLocalRailPosition(Car car, float distance)
    {
        float pos = car.ScalarPosition + distance;
        (Rail lastRail, _) = _rails[car.CurrentSegment];
        (int currentSegment, _, float lastT, float nextTdistance) = CalculateSegment(car.CurrentSegment, pos, car.CurrentCurveT, car.ScalarPosition);
        (Rail currentRail, _) = _rails[currentSegment];
        Bezier curve = currentRail.sp.Curve;
        float currentCurveT = curve.GetNextT(lastT, nextTdistance);

        return (currentRail, currentCurveT);
    }

    private void UpdateCars(float velocity, float deltaTime)
    {
        for(int i = 0; i < _cars.Length; i++)
        {
            float pos = _cars[i].ScalarPosition + velocity * deltaTime;
            (Rail lastRail, _) = _rails[_cars[i].CurrentSegment];
            (int currentSegment, float currentPos, float lastT, float distance) = CalculateSegment(_cars[i].CurrentSegment, pos, _cars[i].CurrentCurveT, _cars[i].ScalarPosition);
            (Rail currentRail, _) = _rails[currentSegment];
            int currentLap = _cars[i].CurrentLap;
            if (currentRail.mp.Type == RailType.Platform && lastRail.mp.Type != RailType.Platform)
                currentLap++;
            
            Bezier curve = currentRail.sp.Curve;


            float currentCurveT = curve.GetNextT(lastT, distance);
            Vector3 vectorPos = curve.Sample(currentCurveT);
            Quaternion rotation = currentRail.GetQuaternionAt(currentCurveT);
            _cars[i].UpdatePhysics(currentPos, velocity, currentSegment, currentLap, currentCurveT);
            _cars[i].Transform(vectorPos, rotation);
        }
    }

    // ------------------- Rail Simulation Props ------------------- //

    public void AddRail(Rail rail, bool simulateRail = false)
    {
        RailPhysics lastrp;
        if(_rails.Count == 0)
        {
            lastrp = _initialrp;
        }
        else
        {
            (_, lastrp) = _rails[_rails.Count - 1];
        }
        
        RailPhysics railPhysics = null;

        if(simulateRail)
            railPhysics = SimulateRail(lastrp, rail);
            
        Debug.Log(simulateRail + " " + railPhysics);

        _rails.Add((rail, railPhysics));
    }

    public void UpdateLastRail(Rail rail)
    {
        if (_rails.Count == 0)
            return;

        // (Rail rail, _) = _rails[_rails.Count - 1];
        RailPhysics lastrp;
        if (_rails.Count == 1)
        {
            lastrp = _initialrp;
        }
        else
        {
            (_, lastrp) = _rails[_rails.Count - 2];
        }
        
        RailPhysics railPhysics = SimulateRail(lastrp, rail);

        _rails[_rails.Count - 1] = (rail, railPhysics);
    }

    public void RemoveLastRail()
    {
        if(_rails.Count <= 0)
            return;
        _rails.RemoveAt(_rails.Count - 1);
    }

    public RailPhysics SimulateRail(RailPhysics lastrp, Rail rail)
    {
        RailPhysics currentRailPhysics = new RailPhysics(lastrp.Final);
        currentRailPhysics.Max = new RailPhysics.Props(0, Vector3.zero);
        float velocity = lastrp.Final.Velocity;
        float curveT = 0f;
        float scalarPosition = 0f;
        int interations = 0;
        while(true)
        {
            RailPhysics.Props currentrpp;
            (currentrpp, curveT) = StepSimulateRail(rail, curveT, velocity, _dtres);
            velocity = currentrpp.Velocity;
            scalarPosition += velocity * _dtres;

            if(velocity <= 0.05)
            {
                interations++;
            }

            if(velocity <= 0 || scalarPosition > rail.rp.Length || curveT > 1f || interations > 25)
            {
                currentRailPhysics.Final = currentrpp;
                currentRailPhysics.CarCompletedSegment = velocity > 0;
                break;
            }

            currentRailPhysics.Final = currentrpp;
            float MaxGForceX = Mathf.Abs(currentRailPhysics.Max.GForce.x) > Mathf.Abs(currentrpp.GForce.x) ? currentRailPhysics.Max.GForce.x : currentrpp.GForce.x;
            float MaxGForceY = Mathf.Abs(currentRailPhysics.Max.GForce.y) > Mathf.Abs(currentrpp.GForce.y) ? currentRailPhysics.Max.GForce.y : currentrpp.GForce.y;
            float MaxGForceZ = Mathf.Abs(currentRailPhysics.Max.GForce.y) > Mathf.Abs(currentrpp.GForce.y) ? currentRailPhysics.Max.GForce.y : currentrpp.GForce.y;
            currentRailPhysics.Max.GForce = new Vector3(MaxGForceX, MaxGForceY, MaxGForceZ);
            currentRailPhysics.Max.Velocity = Mathf.Max(currentRailPhysics.Max.Velocity, velocity);
        }

        // Debug.Log(currentRailPhysics);
        return currentRailPhysics;
    }

    private (RailPhysics.Props, float) StepSimulateRail(Rail rail, float curveT, float velocity, float deltaTime)
    {

        float k1 = CalculateAccelerationRail(rail, curveT, 0f, velocity);
        float k2 = CalculateAccelerationRail(rail, curveT, deltaTime * 0.5f, velocity + k1 * 0.5f);
        float k3 = CalculateAccelerationRail(rail, curveT, deltaTime * 0.5f, velocity + k2 * 0.5f);
        float k4 = CalculateAccelerationRail(rail, curveT, deltaTime, velocity + k3);

        float deltaResultantAcceleration = (deltaTime / 6f) * (k1 + 2f * k2 + 2f * k3 + k4);

        Vector3 basisX = rail.GetBasisAt(curveT).GetColumn(0);

        float newVelocity = velocity + deltaResultantAcceleration;

        curveT = rail.sp.Curve.GetNextT(curveT, deltaTime * newVelocity);

        // TODO: Calculate G-Force
        return (new RailPhysics.Props(newVelocity, Vector3.zero), curveT);
    }

    private float CalculateAccelerationRail(Rail rail, float curveT, float dt, float velocity)
    {
        float newCurveT = rail.sp.Curve.GetNextT(curveT, dt * velocity);
        float acceleration = CalculateAcceleration(rail, newCurveT, velocity, dt) * 0.5f;
        return acceleration;
    }

    private (int, float, float, float) CalculateSegment(int segment, float pos, float t, float lastPos)
    {
        (Rail rail, _) = _rails[segment];

        Rail.CarStatus status = rail.IsInRail(pos);
        while(status != Rail.CarStatus.In)
        {
            if (status == Rail.CarStatus.Foward)
            {
                pos -= rail.rp.Length;
                segment = (segment + 1) % _rails.Count;
                (rail, _) = _rails[segment];
                t = 0f;
                lastPos = 0f;
            }
            else
            {
                segment--;
                if (segment < 0)
                    segment = _rails.Count - 1;
                (rail, _) = _rails[segment];
                pos += rail.rp.Length;
                t = 1f;
                lastPos = rail.rp.Length;
            }
            status = rail.IsInRail(pos);
        }
        
        Mathf.Clamp01(t);
        return (segment, pos, t, pos - lastPos);
    }

    public float DeltaTimeResolution
    {
        get { return _dtres; }
        set { _dtres = value; }
    }

    public bool IsSimulating
    {
        get { return _isSimulating; }
    }

    public Transform FirstCar
    {
        get
        { 
            if(_cars != null && _cars.Length > 0)
            {
                return _cars[0].instantiatedObject.transform;
            }
            else
            {
                return null;
            }
        }
    }

    public RailPhysics LastRailPhysics
    {
        get
        {
            if(_rails.Count == 0) return _initialrp;
            (_,  RailPhysics lastRailPhysics) = _rails[_rails.Count - 1];
            return lastRailPhysics;
        }
    }
}
