using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static ImaginarySphere;
using static RailModelProperties;

[System.Serializable]
public class Generator
{
    [System.Serializable]
    private struct Status
    {
        public RailPhysics rp;
        public Vector3 finalPos;
        public Matrix4x4 finalBasis;
        public float totalLength;
        public float height
        {
            get { return finalPos.y; }
        }
        public SpaceProps sp;

        public Status(RailPhysics rpValue, Vector3 finalPosValue, Matrix4x4 finalBasisValue, float totalLengthValue)
        {
            rp = rpValue;
            finalPos = finalPosValue;
            finalBasis = finalBasisValue;
            totalLength = totalLengthValue;
            sp = new SpaceProps(finalPos, finalBasis);
        }
    }

    public Vector3 initialPosition;
    public Matrix4x4 initialBasis;

    private RollerCoaster _rollerCoaster;
    private BlueprintManager _blueprintManager;
    [SerializeField] private Status _status;
    private bool _isGenerating = false;
    private IEnumerator currentGeneratingCoroutine = null;
    private IEnumerator _blueprintCoroutine = null;
    private IEnumerator _statusCoroutine = null;
    private bool _generatorCanContinue = true;
    private bool _blueprintCanContinue = true;
    private bool _finalizeCoaster = false;

    public Generator(RollerCoaster rollerCoaster)
    {
        _rollerCoaster = rollerCoaster;
        _blueprintManager = _rollerCoaster.GetBlueprintManager();
        _isGenerating = false;
        // Generate();
        // _rc.GenerateCoaster();
    }

    public void Generate()
    {
        _isGenerating = true;
        if(_statusCoroutine != null)
            _rollerCoaster.StopChildCoroutine(_statusCoroutine);
        if (_blueprintCoroutine != null)
            _rollerCoaster.StopChildCoroutine(_blueprintCoroutine);
        if(currentGeneratingCoroutine != null)
            _rollerCoaster.StopChildCoroutine(currentGeneratingCoroutine);
        currentGeneratingCoroutine = GenerateCoroutine();
        _statusCoroutine = null;
        _blueprintCoroutine = null;

        _rollerCoaster.StartChildCoroutine(currentGeneratingCoroutine);        
    }

    private IEnumerator GenerateCoroutine()
    {
        // Start by getting the initial status
        UpdateStatus();
        yield return new WaitUntil(() => _blueprintCanContinue);

        _blueprintCanContinue = true;
        _generatorCanContinue = true;

        _finalizeCoaster = false;

        while(!_finalizeCoaster)
        {
            AddBluerpint();
            yield return new WaitUntil(() => _generatorCanContinue);
        }

        FinalizeCoaster();

        _isGenerating = false;
    }

    private void AddRail(RailProps rp, RailType railType)
    {
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: rp.Elevation, rotation: rp.Rotation, inclination: rp.Inclination);
        _rollerCoaster.UpdateLastRail(length: rp.Length, railType: (int) railType);
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        _blueprintCanContinue = false;
        _statusCoroutine = UpdateStatusCoroutine();
        _rollerCoaster.StartChildCoroutine(_statusCoroutine);
    }

    private IEnumerator UpdateStatusCoroutine()
    {
        RailPhysics railPhysics = _rollerCoaster.GetLastRailPhysics();
        while(railPhysics == null || railPhysics.Final == null)
        {
            yield return null;
            railPhysics = _rollerCoaster.GetLastRailPhysics();
        }
        _status = new Status(railPhysics, _rollerCoaster.GetFinalPosition(), _rollerCoaster.GetFinalBasis(), _rollerCoaster.GetTotalLength());
        _blueprintCanContinue = true;
    }

    private void AddBluerpint()
    {
        _generatorCanContinue = false;
        _blueprintCoroutine = AddBluerpintCoroutine();
        _rollerCoaster.StartChildCoroutine(_blueprintCoroutine);
    }

    private IEnumerator AddBluerpintCoroutine()
    {

        // Filter possible blueprint types
        (List<string> possibleTypes, List<float> cumulativeProbs) = FilterPossibleBlueprints();
        
        // Drawn a possible type
        (int typeId, Blueprint blueprint) = DrawnBlueprintType(possibleTypes, cumulativeProbs);

        // Drown a subtype
        (int subtypeId, string bpSubtype) = DrawnBlueprintSubtype(blueprint);


        // Randomize parameters
        // Dictionary<string, float> bpParams = RandomizeBlueprintParams(blueprint, bpSubtype);
        Dictionary<string, float> bpParams = blueprint.GenerateParams(bpSubtype, _rollerCoaster, _status.sp, _status.rp);

        // Debug.Log(blueprint.Name + " " + bpSubtype);
        // foreach(string key in bpParams.Keys)
        // {
        //     Debug.Log(key + " " + bpParams[key]);
        // }

        // Add blueprint rails
        List<(RailProps, RailType)> rails = blueprint.GetBlueprint(bpSubtype, bpParams);

        for(int i = 0; i < rails.Count; i++)
        {
            (RailProps rp, RailType railType) = rails[i];
            AddRail(rp, railType);
            yield return new WaitUntil(() => _blueprintCanContinue);
        }

        if(_status.totalLength >= 1000f)
            _finalizeCoaster = true;

        _generatorCanContinue = true;
    }

    private (List<string>, List<float>) FilterPossibleBlueprints()
    {
        List<string> possibleTypes = new List<string>();
        List<float> typeCumulativeProbabilities = new List<float>();

        List<string> types = _blueprintManager.GetTypeNames();
        foreach (string keyType in types)
        {
            float probability = _blueprintManager.GetType(keyType).GetProbability(_status.sp, _status.rp);
            if (probability > 0f)
            {
                possibleTypes.Add(keyType);
                if (possibleTypes.Count == 1)
                    typeCumulativeProbabilities.Add(probability);
                else
                    typeCumulativeProbabilities.Add(probability + typeCumulativeProbabilities[possibleTypes.Count - 2]);
            }
        }
        return (possibleTypes, typeCumulativeProbabilities);
    }

    private (int, Blueprint) DrawnBlueprintType(List<string> possibleTypes, List<float> cumulativeProbs)
    {
        float drawn = Random.Range(0f, cumulativeProbs[cumulativeProbs.Count - 1]);
        int drawnTypeId = 0;
        for (int i = 0; i < possibleTypes.Count; i++)
        {
            if (drawn < cumulativeProbs[i])
                break;
            else
                drawnTypeId++;
        }
        if (drawnTypeId >= possibleTypes.Count)
            drawnTypeId = possibleTypes.Count - 1;

        string bpType = possibleTypes[drawnTypeId];
        Blueprint blueprint = _blueprintManager.GetType(bpType);

        return (drawnTypeId, blueprint);
    }

    private (int, string) DrawnBlueprintSubtype(Blueprint blueprint)
    {
        List<string> subtypes = blueprint.GetGenerationSubtypeNames();
        int drawnSubtypes = Random.Range(0, subtypes.Count);
        string bpSubtype = subtypes[drawnSubtypes];
        
        return (drawnSubtypes, bpSubtype);
    }

    private Dictionary<string, float> RandomizeBlueprintParams(Blueprint blueprint, string bpSubtype)
    {
        Dictionary<string, string> bpParamsProps = blueprint.GetParams()[bpSubtype];
        Dictionary<string, float> bpParams = new Dictionary<string, float>();

        foreach (string paramKey in bpParamsProps.Keys)
        {
            string[] paramProps = bpParamsProps[paramKey].Split(';');
            float intercalationValue = float.Parse(paramProps[0], CultureInfo.InvariantCulture.NumberFormat);
            float minValue = float.Parse(paramProps[1], CultureInfo.InvariantCulture.NumberFormat);
            float maxValue = float.Parse(paramProps[2], CultureInfo.InvariantCulture.NumberFormat);
            int range = (int)((maxValue - minValue) / intercalationValue);

            int drawnRange = Random.Range(0, range);
            float drawnValue = minValue + intercalationValue * drawnRange;

            bpParams.Add(paramKey, drawnValue);
        }

        return bpParams;
    }

    private void FinalizeCoaster()
    {
        // TODO
        _finalizeCoaster = false;
    }

    private void GenerateCurveMax90(float rotation)
    {
        rotation = Mathf.Sign(rotation) * Mathf.Min(Mathf.Abs(rotation), Mathf.PI * 0.5f);

        int pieces = Random.Range(2, 4);
        
        float length = ( (_status.rp.Final.Velocity * 2f) / ( (float) pieces) ) * ( Mathf.Abs(rotation) * 2f / Mathf.PI );
        length = Mathf.Max(length, 1f);

        rotation /= (float) pieces;

        // AddRail(rotation: rotation, inclination:-rotation, length: length, railType: (int)RailType.Normal);
        // for (int i = 1; i < pieces - 1; i++)
        //     AddRail(rotation: rotation);
        // AddRail(rotation: rotation, inclination: rotation);
    }

    public bool IsGenerating
    {
        get { return _isGenerating; }
    }
}
