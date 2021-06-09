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
    private bool _finalizedCoaster = false;

    private int _currentModelPartId;

    private List<(string, string)> _model;

    public Generator(RollerCoaster rollerCoaster)
    {
        _rollerCoaster = rollerCoaster;
        _blueprintManager = _rollerCoaster.GetBlueprintManager();
        _isGenerating = false;
        // Generate();
        // _rc.GenerateCoaster();

        _model = new List<(string, string)>() {
            {("Plataform", "")},
            {("Straight", "x;30;1000")},
            {("Curve", "-1")},
            {("Straight", "z;30;50")},
            {("Curve", "-1")},
            {("Straight", "x;-50;-100")},
            {("Curve", "-1")},
            {("Straight", "z;20;0")},
            {("Curve", "-1")},
            {("Straight", "x;-40;-30")},
            {("End", "")},
        };
        // _model = new List<(string, string)>() {
        //     {("Plataform", "")},
        //     {("Straight", "x;50;1000")},
        //     {("Turn", "-1")},
        //     {("Straight", "x;-50;-100")},
        //     {("Turn", "-1")},
        //     {("Straight", "x;-40;-30")},
        //     {("End", "")},
        // };
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

        _finalizedCoaster = false;

        _currentModelPartId = 0;
        while(!_finalizedCoaster)
        {
            AddBluerpint();
            yield return new WaitUntil(() => _generatorCanContinue);
        }

        _isGenerating = false;
    }

    private void AddRail(RailProps rp, RailType railType)
    {
        _rollerCoaster.AddRail(false);
        _rollerCoaster.UpdateLastRailAdd(elevation: rp.Elevation, rotation: rp.Rotation, inclination: rp.Inclination);
        _rollerCoaster.UpdateLastRail(length: rp.Length, railType: (int) railType);
        _rollerCoaster.GenerateSupports(_rollerCoaster.GetRailsCount() - 1);
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

        (string elementType, string posRestrictions) = _model[_currentModelPartId];
        Debug.Log("index: " + _currentModelPartId + " h: " + _status.sp.Position.y + " v: " + _status.rp.Final.Velocity);


        if(elementType.Equals("End"))
        {
            _rollerCoaster.AddRail(false);
            _rollerCoaster.UpdateLastRail(railType: 1);
            _rollerCoaster.AddFinalRail();
            _rollerCoaster.GenerateSupports(_rollerCoaster.GetRailsCount() - 2);
            _rollerCoaster.GenerateSupports(_rollerCoaster.GetRailsCount() - 1);
            _finalizedCoaster = true;
            _currentModelPartId++;
        }
        else 
        {
            List<(RailProps, RailType)> rails = null;

            if(elementType.Equals("Plataform"))
            {
                Dictionary<string, float> bpParams = new Dictionary<string, float>() {
                    {"length", 5},
                };
                rails = _blueprintManager.GetElement("Plataform").GetBlueprint("Plataform", bpParams);
            }
            else
            {
                // Filter possible blueprint elements
                (List<string> possibleElements, List<float> cumulativeProbs) = FilterPossibleBlueprints(elementType);

                for(int i = 0; i < possibleElements.Count; i++)
                {
                    Debug.Log(possibleElements[i] + " " + cumulativeProbs[i]);
                }
                // Drawn a possible element
                (string bpElement, Blueprint blueprint) = DrawnBlueprintElement(possibleElements, cumulativeProbs);

                // Drown a subtype
                (string bpSubtype, string parRestrictionsString) = DrawnBlueprintSubtype(elementType, bpElement);


                // Randomize parameters
                // Dictionary<string, float> bpParams = RandomizeBlueprintParams(blueprint, bpSubtype);
                Dictionary<string, float> bpParams = blueprint.GenerateParams(bpSubtype, _rollerCoaster, _status.sp, _status.rp);

                if(!parRestrictionsString.Equals(""))
                {
                    string[] parRestrictions = parRestrictionsString.Split(';');
                    foreach(string s in parRestrictions)
                    {
                        string[] splited = s.Split('=');
                        bpParams[splited[0]] = float.Parse(splited[1]);
                    }
                }
                if(elementType.Equals("Curve") || elementType.Equals("Turn"))
                {
                    bpParams["orientation"] = int.Parse(posRestrictions);
                    Debug.Log(elementType + " orientation = " + posRestrictions);
                }

                // Debug.Log(blueprint.Name + " " + bpSubtype);
                // foreach(string key in bpParams.Keys)
                // {
                //     Debug.Log(key + " " + bpParams[key]);
                // }

                rails = blueprint.GetBlueprint(bpSubtype, bpParams);
            }

            float previousPos = 0f;
            if (elementType.Equals("Straight"))
            {
                previousPos = posRestrictions.Split(';')[0].Equals("x") ? _status.sp.Position.x : _status.sp.Position.z;
            }

            for (int i = 0; i < rails.Count; i++)
            {
                (RailProps rp, RailType railType) = rails[i];
                AddRail(rp, railType);
                yield return new WaitUntil(() => _blueprintCanContinue);
            }

            if(!elementType.Equals("Straight"))
            {
                _currentModelPartId++;
            }
            // TODO: Fix
            else {
                string[] splitedRestrictions = posRestrictions.Split(';');
                float curPos = splitedRestrictions[0].Equals("x") ? _status.sp.Position.x : _status.sp.Position.z;
                int minPos = int.Parse(splitedRestrictions[1]);
                int maxPos = int.Parse(splitedRestrictions[2]);
                Debug.Log("POS " + previousPos + " " + curPos + " " + minPos + " " + maxPos);
                if(maxPos - minPos > 0)
                {
                    if (curPos > minPos)
                    {
                        if(curPos > maxPos)
                        {
                            for (int i = 0; i < rails.Count; i++)
                            {
                                _rollerCoaster.RemoveLastRail(false);
                            }
                            AddRail(new RailProps(0f, 0f, 0f, Mathf.Abs(maxPos - previousPos)), RailType.Normal);
                            yield return new WaitUntil(() => _blueprintCanContinue);
                        }
                        
                        _currentModelPartId++;
                    }
                }
                else
                {
                    if (curPos < minPos)
                    {
                        if (curPos < maxPos)
                        {
                            for (int i = 0; i < rails.Count; i++)
                            {
                                _rollerCoaster.RemoveLastRail();
                            }
                            AddRail(new RailProps(0f, 0f, 0f, Mathf.Abs(maxPos - previousPos)), RailType.Normal);
                            yield return new WaitUntil(() => _blueprintCanContinue);
                        }
                        _currentModelPartId++;
                    }
                }
            }
        }
        _generatorCanContinue = true;
    }

    private (List<string>, List<float>) FilterPossibleBlueprints(string elementType)
    {
        List<string> possibleElements = new List<string>();
        List<float> elementCumulativeProbabilities = new List<float>();

        List<(string, string, string)> elements = _blueprintManager.GetElementsByType(elementType);

        List<string> elementNames = new List<string>();

        for(int i = 0; i < elements.Count; i++)
        {
            (string name, _, _) = elements[i];
            if(elementNames.Count > 0 && name.Equals(elementNames[elementNames.Count -1]))
                continue;
            elementNames.Add(name);
        }

        foreach (string keyElement in elementNames)
        {
            float probability = _blueprintManager.GetElement(keyElement).GetProbability(_status.sp, _status.rp);
            if (probability > 0f)
            {
                possibleElements.Add(keyElement);
                if (possibleElements.Count == 1)
                    elementCumulativeProbabilities.Add(probability);
                else
                    elementCumulativeProbabilities.Add(probability + elementCumulativeProbabilities[possibleElements.Count - 2]);
            }
        }
        return (possibleElements, elementCumulativeProbabilities);
    }

    private (string, Blueprint) DrawnBlueprintElement(List<string> possibleElements, List<float> cumulativeProbs)
    {
        float drawn = Random.Range(0f, cumulativeProbs[cumulativeProbs.Count - 1]);
        int drawnElementId = 0;
        for (int i = 0; i < possibleElements.Count; i++)
        {
            if (drawn < cumulativeProbs[i])
                break;
            else
                drawnElementId++;
        }
        if (drawnElementId >= possibleElements.Count)
            drawnElementId = possibleElements.Count - 1;

        string bpElement = possibleElements[drawnElementId];
        Blueprint blueprint = _blueprintManager.GetElement(bpElement);

        return (bpElement, blueprint);
    }

    private (string, string) DrawnBlueprintSubtype(string elementType, string drawnElement)
    {
        List<(string, string, string)> elements = _blueprintManager.GetElementsByType(elementType);

        List<(string, string)> subtypes = new List<(string, string)>();
        for(int i = 0; i < elements.Count; i++)
        {
            (string type, string subtype, string restrictions) = elements[i];
            if(type.Equals(drawnElement))
                subtypes.Add((subtype, restrictions));
        }
        int drawn = Random.Range(0, subtypes.Count);
        Debug.Log(elementType + " " + drawnElement + " " + subtypes.Count + " " + drawn);
        Debug.Log(subtypes[drawn]);
        return subtypes[drawn];
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
