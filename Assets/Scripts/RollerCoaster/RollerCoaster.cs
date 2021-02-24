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
    [SerializeField] private SaveManager _saveManager;


    private IEnumerator _carSimulation = null;
    
    void Start()
    {
        RailProps rp = new RailProps(0f, 0f, 0f, 5); 
        ModelProps mp = new ModelProps(1, RailType.Platform, 10);
        SpaceProps sp = new SpaceProps(Vector3.up, Matrix4x4.identity);
        _constructor = new Constructor(this, rp, mp, sp);
        _carSimulation = null;
        _simulator = new Simulator(this, 0.01f, 0, 3);
        _saveManager = new SaveManager();

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

    public void AddRail()
    {
        if(!_simulator.IsSimulating)
        {
            Rail rail = _constructor.AddRail();
            _simulator.AddRail(rail);
        }
        else
        {
            // TODO: Warn player? *Restart simulation?* *Restart simulation if cars are in final rail?*
        }
    }

    public void AddFinalRail()
    {
        (Rail rail1, Rail rail2) = _constructor.AddFinalRail();
        _simulator.UpdateLastRail();
        _simulator.AddRail(rail2);
    }

    public (RailProps, ModelProps) RemoveLastRail(bool secure = true)
    {
        if(secure && _constructor.Rails.Count <= 1)
            return (null, null);
        _simulator.RemoveLastRail();
        return _constructor.RemoveLastRail();
    }

    public void UpdateLastRail(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
    {
        if (!_simulator.IsSimulating)
        {
            _constructor.UpdateLastRail(elevation, rotation, inclination, length, railType);
            _simulator.UpdateLastRail();
        }
        else
        {
            // TODO: Warn player? *Restart simulation?* *Restart simulation if cars are in final rail?*
        }
    }

    public void UpdateLastRailAdd(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
    {
        if (!_simulator.IsSimulating)
        {
            _constructor.UpdateLastRailAdd(elevation, rotation, inclination, length, railType);
            _simulator.UpdateLastRail();
        }
        else
        {
            // TODO: Warn player? *Restart simulation?* *Restart simulation if cars are in final rail?*
        }
    }

    public void StartCarSimulation()
    {
        if (!_simulator.CanSimulateCar())
            return;

        _carSimulation = CarSimulation();
        StartCoroutine(_carSimulation);
    }

    private IEnumerator CarSimulation()
    {
        _simulator.StartCarSimulation();
        yield return null;

        while(_simulator.IsSimulating)
        {
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

    public RailProps GetCurrentGlobalrp()
    {
        return _constructor.CurrentGlobalrp;
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

    public void SaveCoaster(string fileName)
    {
        _saveManager.Save(fileName, _constructor.Rails.ToArray());
    }

    public void LoadCoaster(string fileName)
    {
        if (_simulator.IsSimulating)
            return;
        float railsCount = _constructor.Rails.Count;
        for (int i = 0; i < railsCount; i++)
        {
            this.RemoveLastRail(false);
        }
        SavePack[] savePack = _saveManager.Load(fileName);
        for(int i = 0; i < savePack.Length; i++)
        {
            this.AddRail();
            this.UpdateLastRail(
                length: savePack[i].rp.Length,
                railType: (int)savePack[i].Type
            );
            this.UpdateLastRailAdd(
                elevation: savePack[i].rp.Elevation, 
                rotation: savePack[i].rp.Rotation, 
                inclination: savePack[i].rp.Inclination
            );
        }
    }

    // TODO: Make functions to change rail model props; car props

    // ---------------------------- Intern ---------------------------- //

    public GameObject InstantiateRail(Mesh mesh, Material material)
    {
        Transform railTransform = Instantiate(_railPrefab, Vector3.zero, Quaternion.identity, _railsParent);
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
}
