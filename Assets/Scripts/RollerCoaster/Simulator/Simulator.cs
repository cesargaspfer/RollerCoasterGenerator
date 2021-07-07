using System.Collections;
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
    [SerializeField] private LinkedList<IEnumerator> _railPhysicsCoroutines;
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
        _railPhysicsCoroutines = new LinkedList<IEnumerator>();
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
            // float scalarPosition = _cars[i].CarProperties.Length * 0.5f + ((_cars.Length - 1) - i) * _cars[i].CarProperties.Length;
            float scalarPosition = ((_cars.Length - 1) - i) * _cars[i].CarProperties.Length;
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

            _cars[i].UpdatePhysics(currentPos, _initialrp.Final.Velocity, Vector3.up, currentSegment, currentLap, currentCurveT, distance);
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

        this.UpdateCars(newVelocity, deltaTime, deltaResultantAcceleration / deltaTime);
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
        float acceleration = 0f;
        Bezier curve = rail.sp.Curve;
        Vector3 basisX = rail.GetBasisAt(curveT).GetColumn(0);
        Vector3 basisY = rail.GetBasisAt(curveT).GetColumn(1);

        if (rail.mp.Type != RailModelProperties.RailType.Lever||
           (rail.mp.Type == RailModelProperties.RailType.Lever && velocity > 3f))
            acceleration += Vector3.Dot(-Vector3.up, basisX) * 9.8f;

        if (rail.mp.Type == RailModelProperties.RailType.Lever)
        {
            if(velocity < 6f)
            {
                acceleration += 6f;
            }
        }

        if (rail.mp.Type == RailModelProperties.RailType.Platform)
        {
            if (velocity < 4f)
            {
                acceleration += 6f;
            }
        }

        if (rail.mp.Type == RailModelProperties.RailType.Brake)
        {
            if (velocity > 4f)
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

        return acceleration;
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

    private void UpdateCars(float velocity, float deltaTime, float acceleration)
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

            (Vector3 p0, Rail.CarStatus _) = lastRail.GetPositionInRail(_cars[i].CurrentCurveT);
            Matrix4x4 b0 = lastRail.GetBasisAt(_cars[i].CurrentCurveT);
            Vector3 x0 = b0.GetColumn(0);
            (Vector3 p1, Rail.CarStatus _) = currentRail.GetPositionInRail(currentCurveT);
            Matrix4x4 b1 = currentRail.GetBasisAt(currentCurveT);
            Vector3 x1 = b1.GetColumn(0);

            Vector3 centripetalAcceleration = Vector3.zero;
            float angle = Angle(x0, x1);
            float d = (p1 - p0).magnitude;
            if(angle != 0f && Mathf.Abs(angle) < Mathf.PI * 0.5f && d / angle < 1f)
            {
                Vector3 AcDir = -(Vector3.Cross(x0, Vector3.Cross(x0, x1))).normalized;
                float Ac = (velocity * velocity) / lastRail.Radius;
                centripetalAcceleration = Ac * AcDir;
            }

            Vector3 frontalAcceleration = acceleration * b0.GetColumn(0);
            Vector3 finalAcceleration = (frontalAcceleration + centripetalAcceleration - Vector3.down * 9.8f);

            float Afx = Vector3.Dot(finalAcceleration, b0.GetColumn(0));
            float Afy = Vector3.Dot(finalAcceleration, b0.GetColumn(1));
            float Afz = Vector3.Dot(finalAcceleration, b0.GetColumn(2));

            Vector3 gForce = new Vector3(Afx, Afy, Afz) * 0.1020408f;

            _cars[i].UpdatePhysics(currentPos, velocity, gForce, currentSegment, currentLap, currentCurveT, distance);
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
        
        _rails.Add((rail, null));

        _railPhysicsCoroutines.AddLast(SimulateRail(lastrp, rail, _rails.Count - 1));
        _rollerCoaster.StartChildCoroutine(_railPhysicsCoroutines.Last.Value);
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

        _rails[_rails.Count - 1] = (rail, null);
        if (_railPhysicsCoroutines.Count > 0)
        {
            _rollerCoaster.StopChildCoroutine(_railPhysicsCoroutines.Last.Value);
            _railPhysicsCoroutines.RemoveLast();
        }
        _railPhysicsCoroutines.AddLast(SimulateRail(lastrp, rail, _rails.Count - 1));
        _rollerCoaster.StartChildCoroutine(_railPhysicsCoroutines.Last.Value);
    }

    public void RemoveLastRail()
    {
        if(_rails.Count <= 0)
            return;
        if(_railPhysicsCoroutines.Count > 0)
        {
            _rollerCoaster.StopChildCoroutine(_railPhysicsCoroutines.Last.Value);
            _railPhysicsCoroutines.RemoveLast();
        }
        _rails.RemoveAt(_rails.Count - 1);
    }

    private IEnumerator SimulateRail(RailPhysics lastrp, Rail rail, int railId)
    {

        if(lastrp == null || lastrp.Final == null)
        {
            while(lastrp == null || lastrp.Final == null)
            {
                yield return null;
                (_, lastrp) = _rails[railId - 1];
            }
        }

        RailPhysics currentRailPhysics = new RailPhysics(lastrp.Final);
        currentRailPhysics.Max = new RailPhysics.Props(0, Vector3.zero);

        RailPhysics.Props[] physicsAlongRail = new RailPhysics.Props[(int) rail.rp.Length + 1];
        int nextPositionToSaveProps = 0;
        float velocity = lastrp.Final.Velocity;
        float curveT = 0f;
        float scalarPosition = 0f;
        int interations = 0;
        
        int interationsUntilWait = 0;

        while(true)
        {
            interationsUntilWait++;
            RailPhysics.Props currentrpp;
            (currentrpp, curveT) = StepSimulateRail(rail, curveT, velocity, _dtres);
            velocity = currentrpp.Velocity;
            scalarPosition = rail.rp.Length * curveT;
            
            if(scalarPosition >= nextPositionToSaveProps)
            {
                while(nextPositionToSaveProps <= scalarPosition && nextPositionToSaveProps < physicsAlongRail.Length)
                {
                    physicsAlongRail[nextPositionToSaveProps] = currentrpp;
                    nextPositionToSaveProps++;
                }
            }

            if(velocity <= 0.05)
            {
                interations++;
            }
            else
            {
                interations = 0;
            }

            if(velocity <= 0 || curveT > 1f || interations > 25)
            {
                currentRailPhysics.Final = currentrpp;
                currentRailPhysics.CarCompletedSegment = velocity > 0;
                if (scalarPosition < rail.rp.Length)
                {
                    while (nextPositionToSaveProps < physicsAlongRail.Length)
                    {
                        physicsAlongRail[nextPositionToSaveProps] = currentrpp;
                        nextPositionToSaveProps++;
                    }
                }
                physicsAlongRail[physicsAlongRail.Length - 1] = currentrpp;
                break;
            }

            currentRailPhysics.Final = currentrpp;
            float MaxGForceX = Mathf.Abs(currentRailPhysics.Max.GForce.x) > Mathf.Abs(currentrpp.GForce.x) ? currentRailPhysics.Max.GForce.x : currentrpp.GForce.x;
            float MaxGForceY = Mathf.Abs(currentRailPhysics.Max.GForce.y) > Mathf.Abs(currentrpp.GForce.y) ? currentRailPhysics.Max.GForce.y : currentrpp.GForce.y;
            float MaxGForceZ = Mathf.Abs(currentRailPhysics.Max.GForce.y) > Mathf.Abs(currentrpp.GForce.y) ? currentRailPhysics.Max.GForce.y : currentrpp.GForce.y;
            currentRailPhysics.Max.GForce = new Vector3(MaxGForceX, MaxGForceY, MaxGForceZ);
            currentRailPhysics.Max.Velocity = Mathf.Max(currentRailPhysics.Max.Velocity, velocity);

            if(interationsUntilWait >= 50)
            {
                yield return null;
                interationsUntilWait = 0;
            }
        }

        rail.SetPhysicsAlongRail(physicsAlongRail);
        _rails[railId] = (rail, currentRailPhysics);

        _railPhysicsCoroutines.RemoveFirst();
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

        float newCurveT = rail.sp.Curve.GetNextT(curveT, deltaTime * newVelocity);

        
        Bezier curve = rail.sp.Curve;

        (Vector3 p0, Rail.CarStatus _) = rail.GetPositionInRail(curveT);
        Matrix4x4 b0 = rail.GetBasisAt(curveT);
        Vector3 x0 = b0.GetColumn(0);
        (Vector3 p1, Rail.CarStatus _) = rail.GetPositionInRail(newCurveT);
        Matrix4x4 b1 = rail.GetBasisAt(newCurveT);
        Vector3 x1 = b1.GetColumn(0);

        Vector3 centripetalAcceleration = Vector3.zero;
        float angle = Angle(x0, x1);
        float d = (p1 - p0).magnitude;
        if (angle != 0f && Mathf.Abs(angle) < Mathf.PI * 0.5f && d / angle < 1f)
        {
            Vector3 AcDir = -(Vector3.Cross(x0, Vector3.Cross(x0, x1))).normalized;
            float Ac = (velocity * velocity) / rail.Radius;
            centripetalAcceleration = Ac * AcDir;
        }

        Vector3 frontalAcceleration = deltaResultantAcceleration * deltaTime * b0.GetColumn(0);
        Vector3 finalAcceleration = (frontalAcceleration + centripetalAcceleration - Vector3.down * 9.8f);

        float Afx = Vector3.Dot(finalAcceleration, b0.GetColumn(0));
        float Afy = Vector3.Dot(finalAcceleration, b0.GetColumn(1));
        float Afz = Vector3.Dot(finalAcceleration, b0.GetColumn(2));

        Vector3 gForce = new Vector3(Afx, Afy, Afz) * 0.1020408f;

        return (new RailPhysics.Props(newVelocity, gForce), newCurveT);
    }

    private float CalculateAccelerationRail(Rail rail, float curveT, float dt, float velocity)
    {
        float newCurveT = rail.sp.Curve.GetNextT(curveT, dt * velocity);
        float acceleration = CalculateAcceleration(rail, newCurveT, velocity, dt);
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

    public int FirstCarLap
    {
        get
        {
            if (_cars != null && _cars.Length > 0)
            {
                return _cars[0].CurrentLap;
            }
            else
            {
                return 0;
            }
        }
    }

    public float FirstCarVelocity
    {
        get
        {
            if (_cars != null && _cars.Length > 0)
            {
                return _cars[0].Velocity;
            }
            else
            {
                return 0f;
            }
        }
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

    public RailPhysics InitialRailPhysics
    {
        get
        {
            return _initialrp;
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
