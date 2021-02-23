using UnityEngine;
using static RailModelProperties;

[System.Serializable]
public class ModelProps
{
    [SerializeField] private int _modelId;
    [SerializeField] private RailType _type;
    [SerializeField] private int _resolution;
    
    public ModelProps(int modelId, RailType type, int resolution)
    {
        _modelId = modelId;
        _type = type;
        _resolution = resolution;
    }

    public ModelProps(Vector3 properties)
    {
        _modelId = (int) properties[0];
        _type = (RailType) properties[1];
        _resolution = (int) properties[2];
    }

    public ModelProps(int[] properties)
    {
        Debug.Assert(properties.Length == 3, "The properties of ModelProps isn't equals 3");
        _modelId = properties[0];
        _type = (RailType) properties[1];
        _resolution = properties[2];
    }

    public ModelProps Clone()
    {
        return new ModelProps(_modelId, _type, _resolution);
    }

    public int ModelId
    {
        get { return _modelId; }
        set { _modelId = value; }
    }

    public RailType Type
    {
        get { return _type; }
        set { _type = value; }
    }

    public int Resolution
    {
        get { return _resolution; }
        set { _resolution = value; }
    }
}
