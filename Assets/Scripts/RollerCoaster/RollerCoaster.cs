using System.Collections;
using UnityEngine;
using static RailModelProperties;
using static SaveManager;

public class RollerCoaster : MonoBehaviour
{

    #pragma warning disable 0649
    [SerializeField] private bool _debug;
    #pragma warning disable 0649
    [SerializeField] private bool _debugAddFinalRail;
    #pragma warning disable 0649
    [SerializeField] public Transform _railPrefab;
    #pragma warning disable 0649
    [SerializeField] public Transform _railsParent;
    #pragma warning disable 0649
    [SerializeField] public Transform _carsParent;
    #pragma warning disable 0649
    [SerializeField] private Transform _carManagerPrefab;
    #pragma warning disable 0649
    [SerializeField] public Transform _finalRailDebugger;
    [SerializeField] private Constructor _constructor;
    [SerializeField] private Generator _generator;
    [SerializeField] private Simulator _simulator;
    [SerializeField] private bool _isComplete;
    [SerializeField] private bool _carSimulationPause;

    private IEnumerator _carSimulation = null;

    // TODO: Add arguments
    public void Initialize()
    {
        RailProps rp = new RailProps(0f, 0f, 0f, 5); 
        ModelProps mp = new ModelProps(1, RailType.Platform, 10);
        SpaceProps sp = new SpaceProps(Vector3.up, Matrix4x4.identity);
        _constructor = new Constructor(this, rp, mp, sp);
        _carSimulation = null;
        _simulator = new Simulator(this, 0.01f, 0, 1);
        _isComplete = false;

        if (CarsManager.inst == null)
        {
            Instantiate(_carManagerPrefab, Vector3.zero, Quaternion.identity);
        }
        
        _generator = new Generator(this);
        if(_debugAddFinalRail)
        {
            var finalRailDebuggerTransform = Instantiate(_finalRailDebugger, -Vector3.right, Quaternion.identity);
            finalRailDebuggerTransform.GetComponent<FinalRailDebugger>().constructor = _constructor;
        }
    }

    public void AddRail(bool simulateRail = false)
    {
        if(!_simulator.IsSimulating && !_isComplete)
        {
            Rail rail = _constructor.AddRail();
            _simulator.AddRail(rail, simulateRail);
        }
        else
        {
            // TODO: Warn player? *Restart simulation?* *Restart simulation if cars are in final rail?*
        }
    }

    public void AddFinalRail()
    {
        if (_isComplete) return;
        (Rail rail1, Rail rail2) = _constructor.AddFinalRail();
        _simulator.UpdateLastRail(rail1);
        _simulator.AddRail(rail2);
        _isComplete = true;
    }

    public (RailProps, ModelProps) RemoveLastRail(bool secure = true)
    {
        if(secure && _constructor.Rails.Count <= 1)
            return (null, null);
        _simulator.RemoveLastRail();
        _isComplete = false;
        return _constructor.RemoveLastRail();
    }

    public void UpdateLastRail(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999, bool simulateRail = false)
    {
        if (!_simulator.IsSimulating && !_isComplete)
        {
            Rail rail = _constructor.UpdateLastRail(elevation, rotation, inclination, length, railType);
            if(simulateRail)
                _simulator.UpdateLastRail(rail);
        }
        else
        {
            // TODO: Warn player? *Restart simulation?* *Restart simulation if cars are in final rail?*
        }
    }

    public void UpdateLastRailAdd(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999, bool simulateRail = false)
    {
        if (!_simulator.IsSimulating && !_isComplete)
        {
            Rail rail = _constructor.UpdateLastRailAdd(elevation, rotation, inclination, length, railType);
            if(simulateRail)
                _simulator.UpdateLastRail(rail);
        }
        else
        {
            // TODO: Warn player? *Restart simulation?* *Restart simulation if cars are in final rail?*
        }
    }

    public void SimulateLastRail()
    {
        if(_constructor.CurrentRail != null)
            _simulator.UpdateLastRail(_constructor.CurrentRail);
    }

    public void StartCarSimulation()
    {
        if (!_simulator.CanSimulateCar())
            return;
        _carSimulationPause = false;
        _carSimulation = CarSimulation();
        StartCoroutine(_carSimulation);
    }

    private IEnumerator CarSimulation()
    {
        _simulator.StartCarSimulation();
        yield return null;

        while(_simulator.IsSimulating)
        {
            if(!_carSimulationPause)
                _simulator.SetpCarSimulation(Time.deltaTime);
            yield return null;
        }

        StopCarSimulation();
    }

    public void StopCarSimulation()
    {
        if(_carSimulation == null)
            return;
        StopCoroutine(_carSimulation);

        _simulator.StopCarSimulation();
    }

    public void SetPauseCarSimulation(bool value)
    {
        _carSimulationPause = value;
    }

    public RailProps GetCurrentGlobalrp()
    {
        return _constructor.CurrentGlobalrp;
    }

    public RailProps GetLastGlobalrp()
    {
        return _constructor.LastGlobalrp;
    }

    public void GenerateCoaster()
    {
        if(_simulator.IsSimulating)
            return;
        float railsCount = _constructor.Rails.Count;
        for(int i = 0; i < railsCount; i++)
        {
            this.RemoveLastRail(false);
        }
        _generator.Generate();
    }

    public bool SaveCoaster(string coasterName)
    {
        return SaveManager.SaveCoaster(coasterName, _constructor.Rails.ToArray());
    }

    public void LoadCoaster(string coasterName)
    {
        if (_simulator.IsSimulating)
            StopCarSimulation();

        _isComplete = false;
        float railsCount = _constructor.Rails.Count;
        for (int i = 0; i < railsCount; i++)
        {
            this.RemoveLastRail(false);
        }
        SavePack[] savePack = SaveManager.LoadCoaster(coasterName);
        for(int i = 0; i < savePack.Length; i++)
        {
            this.AddRail();
            if(!savePack[i].IsFinalRail)
            {
                this.UpdateLastRail(
                    length: savePack[i].rp.Length,
                    railType: (int)savePack[i].Type
                );
                this.UpdateLastRailAdd(
                    elevation: savePack[i].rp.Elevation, 
                    rotation: savePack[i].rp.Rotation, 
                    inclination: savePack[i].rp.Inclination,
                    simulateRail: true
                );
            }
            else
            {
                this.AddFinalRail();
                return;
            }
        }
    }

    public (string[], Sprite[]) LoadCoastersNamesAndImages()
    {
        return SaveManager.LoadCoastersNamesAndImages();
    }

    public Sprite[] LoadCoastersImages()
    {
        return SaveManager.LoadCoastersImages();
    }

    public Sprite LoadCoasterImage(string coasterName)
    {
        return SaveManager.LoadCoasterImage(coasterName);
    }

    public string[] LoadCoastersNames()
    {
        return SaveManager.LoadCoastersNames();
    }

    // TODO: Make functions to change rail model props; car props

    // ---------------------------- Intern ---------------------------- //

    public GameObject InstantiateRail(Mesh mesh, Material material, Vector3 position)
    {
        Transform railTransform = Instantiate(_railPrefab, position, Quaternion.identity, _railsParent);
        railTransform.GetComponent<MeshFilter>().mesh = mesh;
        railTransform.GetComponent<Renderer>().material = material;
        return railTransform.gameObject;
    }

    public Car InstantiateCar(int id, CarsManager.CarType type)
    {
        Transform carTransform = Instantiate(CarsManager.inst.GetCarPrefab(id, type), Vector3.zero, Quaternion.identity, _carsParent);
        Car car = carTransform.gameObject.AddComponent(typeof(Car)) as Car;
        car.Initialize(0f, 0, CarsManager.inst.GetCarProps(id));
        return carTransform.gameObject.GetComponent<Car>();
    }

    public Rail GetLastRail()
    {
        return _constructor.CurrentRail;
    }

    public RailPhysics GetLastRailPhysics()
    {
        return _simulator.LastRailPhysics;
    }

    public Vector3 GetInitialPosition()
    {
        return _constructor.InitialPosition;
    }

    public Matrix4x4 GetInitialBasis()
    {
        return _constructor.InitialBasis;
    }

    public Vector3 GetFinalPosition()
    {
        return _constructor.FinalPosition;
    }

    public Matrix4x4 GetFinalBasis()
    {
        return _constructor.FinalBasis;
    }

    public Transform GetFirstCar()
    {
        return _simulator.FirstCar;
    }

    public float GetTotalLength()
    {
        return _constructor.TotalLength;
    }

    public bool IsComplete()
    {
        return _isComplete;
    }
}
