using UnityEngine;

[System.Serializable]
public class RailProps
{
    [SerializeField] public float _elevation;
    [SerializeField] public float _rotation;
    [SerializeField] public float _inclination;
    [SerializeField] public float _length;

    public RailProps(float elevation, float rotation, float inclination, float length)
    {
        _elevation = elevation;
        _rotation = rotation;
        _inclination = inclination;
        _length = length;
    }
    public RailProps(float length)
    {
        _elevation = 0;
        _rotation = 0;
        _inclination = 0;
        _length = length;
    }

    public RailProps(Vector4 properties)
    {
        _elevation =    properties[0];
        _rotation =     properties[1];
        _inclination =  properties[2];
        _length = properties[3];
    }

    public RailProps(float[] properties)
    {
        Debug.Assert(properties.Length == 4, "The properties of RailProps isn't equals 4");
        _elevation = properties[0];
        _rotation = properties[1];
        _inclination = properties[2];
        _length = properties[3];
    }

    public RailProps Clone()
    {
        return new RailProps(_elevation, _rotation, _inclination, _length);
    }

    public static RailProps operator -(RailProps rp1, RailProps rp2)
    {
        RailProps tmp = rp1.Clone();
        tmp.Elevation -= rp2.Elevation;
        if (tmp.Elevation < -Mathf.PI)
            tmp.Elevation += 2 * Mathf.PI;
        else if (tmp.Elevation > Mathf.PI)
            tmp.Elevation -= 2 * Mathf.PI;
        tmp.Rotation -= rp2.Rotation;
        if (tmp.Rotation < -Mathf.PI)
            tmp.Rotation += 2 * Mathf.PI;
        else if (tmp.Rotation > Mathf.PI)
            tmp.Rotation -= 2 * Mathf.PI;
        tmp.Inclination -= rp2.Inclination;
        if (tmp.Inclination < -Mathf.PI)
            tmp.Inclination += 2 * Mathf.PI;
        else if (tmp.Inclination > Mathf.PI)
            tmp.Inclination -= 2 * Mathf.PI;
        return tmp;
    }
    
    public static RailProps operator +(RailProps rp1, RailProps rp2)
    {
        RailProps tmp = rp1.Clone();
        tmp.Elevation += rp2.Elevation;
        tmp.Rotation += rp2.Rotation;
        tmp.Inclination += rp2.Inclination;
        return tmp;
    }

    public float Elevation
    {
        get { return _elevation; }
        set { _elevation = value; }
    }

    public float Rotation
    {
        get { return _rotation; }
        set { _rotation = value; }
    }

    public float Inclination
    {
        get { return _inclination; }
        set { _inclination = value; }
    }

    public float Length
    {
        get { return _length; }
        set { _length = value; }
    }

    public Vector3 Radians
    {
        get { return new Vector3(_elevation, _rotation, _inclination); }
    }
}
