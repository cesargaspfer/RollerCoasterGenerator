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
        RailPhysics.Props rpp = new RailPhysics.Props(0f, Vector3.zero);
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
        
        float k1 = CalculateResultantAceleration(0f, velocity);
        float k2 = CalculateResultantAceleration(deltaTime * 0.5f, velocity + k1 * 0.5f);
        float k3 = CalculateResultantAceleration(deltaTime * 0.5f, velocity + k2 * 0.5f);
        float k4 = CalculateResultantAceleration(deltaTime, velocity + k3);

        float deltaResultantAceleration = (deltaTime / 6f) * (k1 + 2f * k2 + 2f * k3 + k4);

        float newVelocity = velocity + deltaResultantAceleration;

        this.UpdateCars(newVelocity, deltaTime);
    }

    private float CalculateResultantAceleration(float dt, float velocity)
    {
        float aceleration = 0f;
        for(int i = 0; i < _cars.Length; i++)
        {
            (Rail rail, float curveT) = GetCarLocalRailPosition(_cars[i], dt * velocity);
            aceleration += CalculateAceleration(rail, curveT, velocity, dt);
        }
        aceleration /= _cars.Length;
        return aceleration;
    }

    private float CalculateAceleration(Rail rail, float curveT, float velocity, float dt)
    {
        // TODO: Define mass inside the car
        float mass = 1f;
        float aceleration = 0f;
        Bezier curve = rail.sp.Curve;
        Vector3 basisX = rail.GetBasisAt(curveT).GetColumn(0);
        Vector3 basisY = rail.GetBasisAt(curveT).GetColumn(1);

        if (rail.mp.Type != RailModelProperties.RailType.Lever||
           (rail.mp.Type == RailModelProperties.RailType.Lever && velocity > 3f))
            aceleration += Vector3.Dot(-Vector3.up, basisX) * 9.8f;
        
        if (rail.mp.Type == RailModelProperties.RailType.Lever)
        {
            if(velocity < 2f)
            {
                aceleration += 2f;
            }
        }

        if (rail.mp.Type == RailModelProperties.RailType.Platform)
        {
            if (velocity < 1f)
            {
                aceleration += 1f;
            }
        }

        if (rail.mp.Type == RailModelProperties.RailType.Brake)
        {
            if (velocity > 1f)
            {
                aceleration -= 5f;
            }
        }

        if(rail.mp.Type == RailModelProperties.RailType.Brake || rail.mp.Type == RailModelProperties.RailType.Normal ||
           (rail.mp.Type == RailModelProperties.RailType.Lever && velocity > 2f) ||
           (rail.mp.Type == RailModelProperties.RailType.Platform && velocity > 1f))
        {
            if (velocity > 0.07f)
            {
                aceleration -= Mathf.Sign(velocity) * 0.005f * Mathf.Max(Mathf.Abs(velocity), 10f) * (0.5f + 0.5f * Mathf.Abs(Vector3.Dot(Vector3.up, basisY))) * 9.8f;
            }
            else
            {
                float dragAceleration = - Mathf.Sign(velocity) * 0.005f * 10f * (0.5f + 0.5f * Mathf.Abs(Vector3.Dot(Vector3.up, basisY))) * 9.8f;
                if(Mathf.Abs(2f * dragAceleration) > Mathf.Abs(aceleration))
                {
                    aceleration = 0.5f * dragAceleration;
                }
                else
                {
                    aceleration += dragAceleration;
                }
            }
        }

        return aceleration / mass;
    }

    public void StopCarSimulation()
    {
        for (int i = 0; i < _cars.Length; i++)
        {
            Car tmpCar = _cars[i];
            _cars[i] = null;
            GameObject.Destroy(tmpCar.instantiatedObject);
        }

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

    public void AddRail(Rail rail)
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

        RailPhysics railPhysics = SimulateRail(lastrp, rail);

        _rails.Add((rail, railPhysics));
    }

    public void UpdateLastRail()
    {
        if (_rails.Count == 0)
            return;

        (Rail rail, _) = _rails[_rails.Count - 1];
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
        while(true)
        {
            RailPhysics.Props currentrpp;
            (currentrpp, curveT) = StepSimulateRail(rail, curveT, velocity, _dtres);
            velocity = currentrpp.Velocity;
            scalarPosition += velocity * _dtres;

            if(velocity <= 0 || scalarPosition > rail.rp.Length || curveT > 1f)
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
        }

        return currentRailPhysics;
    }

    private (RailPhysics.Props, float) StepSimulateRail(Rail rail, float curveT, float velocity, float deltaTime)
    {

        float k1 = CalculateAcelerationRail(rail, curveT, 0f, velocity);
        float k2 = CalculateAcelerationRail(rail, curveT, deltaTime * 0.5f, velocity + k1 * 0.5f);
        float k3 = CalculateAcelerationRail(rail, curveT, deltaTime * 0.5f, velocity + k2 * 0.5f);
        float k4 = CalculateAcelerationRail(rail, curveT, deltaTime, velocity + k3);

        float deltaResultantAceleration = (deltaTime / 6f) * (k1 + 2f * k2 + 2f * k3 + k4);

        float newVelocity = velocity + deltaResultantAceleration;

        curveT = rail.sp.Curve.GetNextT(curveT, deltaTime * deltaResultantAceleration);

        // TODO: Calculate G-Force
        return (new RailPhysics.Props(velocity, Vector3.zero), curveT);
    }

    private float CalculateAcelerationRail(Rail rail, float curveT, float dt, float velocity)
    {
        float newCurveT = rail.sp.Curve.GetNextT(curveT, dt * velocity);
        float aceleration = CalculateAceleration(rail, newCurveT, velocity, dt) * 0.5f;
        return aceleration;
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
}
