using System.Text;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using static RailModelProperties;
using static ImaginarySphere;
using static SaveManager;

public class RollerCoaster : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private bool _debugAddFinalRail;
    #pragma warning disable 0649
    [SerializeField] public Transform _railPrefab;
    #pragma warning disable 0649
    [SerializeField] public Transform _railsParent;
    #pragma warning disable 0649
    [SerializeField] public Transform _supportsParent;
    #pragma warning disable 0649
    [SerializeField] public Transform _carsParent;
    #pragma warning disable 0649
    [SerializeField] public Transform _heatmapsParent;
    #pragma warning disable 0649
    [SerializeField] private Transform _carManagerPrefab;
    #pragma warning disable 0649
    [SerializeField] public Transform _finalRailDebugger;
    [SerializeField] private Constructor _constructor;
    [SerializeField] private Generator _generator;
    [SerializeField] private Simulator _simulator;
    [SerializeField] private BlueprintManager _blueprintManager;
    [SerializeField] private bool _isComplete;
    [SerializeField] private bool _carSimulationPause;
    [SerializeField] private bool _isHeatmapActive;
    [SerializeField] private int _heatmapValue = -1;

    private IEnumerator _carSimulation = null;

    // TODO: Add arguments
    public void Initialize()
    {
        RailProps rp = new RailProps(0f, 0f, 0f, 5); 
        ModelProps mp = new ModelProps(1, RailType.Platform, 10);
        SpaceProps sp = new SpaceProps(Vector3.up, Matrix4x4.identity);
        _constructor = new Constructor(this, rp, mp, sp);
        _carSimulation = null;
        _simulator = new Simulator(this, 1f / 60f, 0, 1);
        _blueprintManager = new BlueprintManager();
        _isComplete = false;
        _heatmapValue = -1;
        SetHeatmap(0);

        if (CarsManager.inst == null)
        {
            Instantiate(_carManagerPrefab, Vector3.zero, Quaternion.identity);
        }
        
        _generator = new Generator(this);
        if(_debugAddFinalRail)
        {
            var finalRailDebuggerTransform = Instantiate(_finalRailDebugger, -Vector3.right, Quaternion.identity);
            finalRailDebuggerTransform.GetComponent<FinalRailDebugger>().constructor = _constructor;
            AddRail(false);
        }
    }

    public void AddRail(bool isPreview)
    {
        if(!_simulator.IsSimulating && !_isComplete)
        {
            Rail rail = _constructor.AddRail(isPreview);
            _simulator.AddRail(rail);
        }
    }

    public void AddFinalRail(int railType = -1)
    {
        if (_isComplete) return;
        (Rail rail1, Rail rail2) = _constructor.AddFinalRail(railType);
        _simulator.UpdateLastRail(rail1);
        _simulator.AddRail(rail2);
        _isComplete = true;
    }

    public (RailProps, ModelProps) RemoveLastRail(bool secure = true)
    {
        if(secure && _constructor.Rails.Count <= 2)
            return (null, null);
        _simulator.RemoveLastRail();
        _isComplete = false;
        return _constructor.RemoveLastRail();
    }

    public bool CanAddFinalRail()
    {
        return _constructor.CanAddFinalRail();
    }

    public void UpdateLastRail(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
    {
        if (!_simulator.IsSimulating && !_isComplete)
        {
            Rail rail = _constructor.UpdateLastRail(elevation, rotation, inclination, length, railType);
            _simulator.UpdateLastRail(rail);
        }
        else
        {
            // TODO: Warn player? *Restart simulation?* *Restart simulation if cars are in final rail?*
        }
    }

    public void UpdateLastRailAdd(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
    {
        if (!_simulator.IsSimulating && !_isComplete)
        {
            Rail rail = _constructor.UpdateLastRailAdd(elevation, rotation, inclination, length, railType);
            _simulator.UpdateLastRail(rail);
        }
        else
        {
            // TODO: Warn player? *Restart simulation?* *Restart simulation if cars are in final rail?*
        }
    }

    public void AddBlueprint(List<(RailProps, RailType)> rails, bool isPreview)
    {
        List<Rail> newRails = _constructor.AddBlueprint(rails, isPreview);
        for(int i = 0; i < newRails.Count; i++)
            _simulator.AddRail(newRails[i]);
    }

    public void GenerateSupports(int id)
    {
        _constructor.GenerateSupports(id);
    }

    public void RemoveSupports(int id)
    {
        _constructor.RemoveSupports(id);
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
        if(_heatmapValue != 0)
        {
            int tmpheatmapValue = _heatmapValue;
            _heatmapValue = -1;
            SetHeatmap(tmpheatmapValue);
        }
    }

    public void SetRailPreview(int id, bool isPreview)
    {
        _constructor.SetRailPreview(id, isPreview);
    }

    public void SetLastRailPreview(bool isPreview)
    {
        _constructor.SetRailPreview(_constructor.Rails.Count - 1, isPreview);
    }

    public bool SaveCoaster(string coasterName, (string, Vector3, float)[] decorativeObjects, float[] terrain)
    {
        return SaveManager.SaveCoaster(coasterName, _constructor.Rails.ToArray(), decorativeObjects, terrain);
    }

    public ((string, Vector3, float)[], float[]) LoadCoaster(string coasterName)
    {
        if (_simulator.IsSimulating)
            StopCarSimulation();

        _isComplete = false;
        SetHeatmap(0);
        float railsCount = _constructor.Rails.Count;
        for (int i = 0; i < railsCount; i++)
        {
            this.RemoveLastRail(false);
        }
        (SavePack[] savePack, int modelId, (string, Vector3, float)[] decorativeObjects, float[] terrain) = SaveManager.LoadCoaster(coasterName);

        // Helps when projecting blueprints
        // string coasterSting = "";
        // for (int i = 0; i < savePack.Length; i++)
        // {
        //     string sE = savePack[i].rp.Elevation.ToString().Replace(',', '.');
        //     string sR = (-savePack[i].rp.Rotation).ToString().Replace(',', '.');
        //     string sI = (-savePack[i].rp.Inclination).ToString().Replace(',', '.');
        //     int sL = Mathf.RoundToInt(savePack[i].rp.Length);
        //     coasterSting += "rails.Add((new RailProps(" + sE + "f, " + sR + "f * orientation, " + sI + "f * orientation, " + sL + " * lengthScale), RailType.Normal));\n";
        // }

        ChangeRailModel(modelId);

        for(int i = 0; i < savePack.Length; i++)
        {
            if(!savePack[i].IsFinalRail)
            {
                if (i < savePack.Length - 1)
                    this.AddRail(false);
                else
                    this.AddRail(true);
                this.UpdateLastRail(
                    length: savePack[i].rp.Length,
                    railType: (int)savePack[i].Type
                );
                this.UpdateLastRailAdd(
                    elevation: savePack[i].rp.Elevation, 
                    rotation: savePack[i].rp.Rotation, 
                    inclination: savePack[i].rp.Inclination
                );
                if (i < savePack.Length - 1)
                    GenerateSupports(i);
            }
            else
            {
                this.AddRail(false);
                this.AddFinalRail((int) savePack[i].Type);
                GenerateSupports(i);
                GenerateSupports(i+1);
                return (decorativeObjects, terrain);
            }
        }
        return (decorativeObjects, terrain);
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

    public bool CoasterExists(string coasterName)
    {
        return SaveManager.CoasterExists(coasterName);
    }

    public void SetHeatmap(int type)
    {
        if(_heatmapValue == type) return;
        _heatmapValue = type;
        _isHeatmapActive = type != 0;
        if(_isHeatmapActive)
        {
            _railsParent.gameObject.SetActive(false);
            _heatmapsParent.gameObject.SetActive(true);
        }
        else
        {
            _railsParent.gameObject.SetActive(true);
            _heatmapsParent.gameObject.SetActive(false);
        }
        _constructor.SetHeatmap(type);
    }
    
    public bool CheckRailPlacement(int id)
    {
        return _constructor.CheckRailPlacement(id);
    }

    public bool CheckLastRailPlacement()
    {
        return _constructor.CheckLastRailPlacement();
    }

    public void ChangeRailModel(int modelId)
    {
        _constructor.ChangeRailModel(modelId);
    }

    // TODO: Make functions to change rail model props; car props

    // ---------------------------- Intern ---------------------------- //

    public GameObject InstantiateRail(Mesh mesh, Material material, Vector3 position, bool isSupport = false, bool isHeatmap = false)
    {
        Transform railTransform = Instantiate(_railPrefab, position, Quaternion.identity, _railsParent);
        if (isSupport)
            railTransform.SetParent(_supportsParent);
        else if(isHeatmap)
            railTransform.SetParent(_heatmapsParent);
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

    public void StartChildCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void StopChildCoroutine(IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }

    public static Vector3 MeasureRail(RailProps rp)
    {
        return MeasureRail(rp, new SpaceProps(Vector3.zero, Matrix4x4.identity));
    }

    public static Vector3 MeasureRail(RailProps rp, SpaceProps sp)
    {
        (_, Matrix4x4 finalBasis, Vector3 finalPosition) = CalculateImaginarySphere(sp, rp);
        return finalPosition - sp.Position;
    }

    public BlueprintManager GetBlueprintManager()
    {
        return _blueprintManager;
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

    public int GetRailsCount()
    {
        return _constructor.Rails.Count;
    }

    public float GetTotalLength()
    {
        return _constructor.TotalLength;
    }

    public bool IsComplete()
    {
        return _isComplete;
    }

    public bool IsGenerating()
    {
        return _generator.IsGenerating;
    }

    private bool _testBlueprintsMinVelocityCoroutineCanContinue = false;
    private bool _testAllBlueprintParamsCanContinue = false;
    private float _testBpVel = 5f;

    public void TestBlueprintsMinVelocity()
    {
        StartCoroutine(TestBlueprintsMinVelocityCoroutine());
    }

    public IEnumerator TestBlueprintsMinVelocityCoroutine()
    {
        List<string> elements = _blueprintManager.GetElementNames();

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Equals("Lever") || elements[i].Equals("Fall") || elements[i].Equals("Curve") || elements[i].Equals("Straight") || elements[i].Equals("Plataform"))
                continue;
            if(!elements[i].Equals("Hill"))
                continue;
            _testBlueprintsMinVelocityCoroutineCanContinue = false;
            StartCoroutine(TestAllBlueprintParams(_blueprintManager.GetElement(elements[i])));
            yield return new WaitUntil(() => _testBlueprintsMinVelocityCoroutineCanContinue);
        }

        Debug.Log("Done Testing Blueprint Params!");
    }


    public IEnumerator TestAllBlueprintParams(Blueprint bp)
    {
        string subtype = bp.GetSubtypeNames()[0];
        if(bp.Name.Equals("Hill"))
            subtype = "StraightLength";

        Dictionary<string, string> bpParamsProps = bp.GetParams()[subtype];
        List<(string, float, float, float)> paramsRest = new List<(string, float, float, float)>();

        Dictionary<string, float> bpParams = new Dictionary<string, float>();
        bpParams["orientation"] = 1;
        List<string> paramsToCheck = new List<string>();

        foreach (string paramKey in bpParamsProps.Keys)
        {
            if(paramKey.Equals("orientation"))
                continue;
            string[] paramProps = bpParamsProps[paramKey].Split(';');
            float intercalationValue = float.Parse(paramProps[0], CultureInfo.InvariantCulture.NumberFormat);
            float minValue = float.Parse(paramProps[1], CultureInfo.InvariantCulture.NumberFormat);
            float maxValue = float.Parse(paramProps[2], CultureInfo.InvariantCulture.NumberFormat);


            paramsRest.Add((paramKey, intercalationValue, minValue, maxValue));
            bpParams[paramKey] = minValue;
            paramsToCheck.Add(paramKey);
        }

        StringBuilder csv = new StringBuilder();
        string line = "";
        foreach(string key in paramsToCheck)
        {
            line += key + ";";
        }
        line += "velocity";
        csv.AppendLine(line);

        _testAllBlueprintParamsCanContinue = false;
        StartCoroutine(TestBlueprintParams(bp, subtype, bpParams, 5f));
        yield return new WaitUntil(() => _testAllBlueprintParamsCanContinue);

        float minVel = _testBpVel;
        line = "";
        foreach (string key in paramsToCheck)
        {
            line += Mathf.Round(bpParams[key] * 10) / 10f + ";";
        }
        line += Mathf.RoundToInt(_testBpVel * 10) / 10f;
        csv.AppendLine(line);

        // TODO: Loop
        while(true)
        {
            // Change Params
            int index = paramsToCheck.Count - 1;
            while(true)
            {
                (string name, float intercalationValue, float minValue, float maxValue) = paramsRest[index];
                bpParams[name] += intercalationValue;
                if(bpParams[name] > maxValue)
                {
                    bpParams[name] = minValue;
                    minVel = 5f;
                    index--;
                    if (index < 0)
                        break;
                }
                else
                {
                    break;
                }
            }
            if(index < 0)
                break;

            _testAllBlueprintParamsCanContinue = false;
            StartCoroutine(TestBlueprintParams(bp, subtype, bpParams, minVel));
            yield return new WaitUntil(() => _testAllBlueprintParamsCanContinue);

            minVel = _testBpVel;
            line = "";
            foreach (string key in paramsToCheck)
            {
                line += Mathf.Round(bpParams[key] * 10) / 10f + ";";
            }
            line += Mathf.RoundToInt(_testBpVel * 10) / 10f;
            csv.AppendLine(line);
        }


        File.WriteAllText("Results/" + bp.Name + ".csv", csv.ToString());
        _testBlueprintsMinVelocityCoroutineCanContinue = true;
    }

    public IEnumerator TestBlueprintParams(Blueprint bp, string subtype, Dictionary<string, float> bpParams, float startVel)
    {
        if (_simulator.IsSimulating)
            StopCarSimulation();
        float railsCount = _constructor.Rails.Count;
        for (int i = 0; i < railsCount; i++)
        {
            this.RemoveLastRail(false);
        }

        List<(RailProps, RailType)> bpRails = bp.GetBlueprint(subtype, bpParams);
        AddBlueprint(bpRails, false);
        AddRail(false);
        UpdateLastRail(railType: 0);
        _testBpVel = startVel;

        while(true)
        {
            _simulator.InitialRailPhysics.Final = new RailPhysics.Props(_testBpVel, _simulator.InitialRailPhysics.Final.GForce);
            StartCarSimulation();

            yield return new WaitUntil(() => {
                return _simulator.FirstCarVelocity <= 0.5f || _simulator.FirstCarLap >= 1;
            });

            if(_simulator.FirstCarLap >= 1)
                break;
            _testBpVel += 0.5f;

            StopCarSimulation();
        }
        StopCarSimulation();

        _testAllBlueprintParamsCanContinue = true;
    }
}
