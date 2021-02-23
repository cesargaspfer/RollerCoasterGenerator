using UnityEngine;

[System.Serializable]
public class SpaceProps
{
    // Initial position
    [SerializeField] private Vector3 _position;
    // Initial basis
    [SerializeField] private Matrix4x4 _basis;
    [SerializeField] private Bezier _curve;

    public SpaceProps(Vector3 position, Matrix4x4 basis)
    {
        _position = position;
        _basis = basis;
        _curve = null;
    }

    public SpaceProps(Vector3 position, Matrix4x4 basis, Bezier curve)
    {
        _position = position;
        _basis = basis;
        _curve = curve;
    }

    public SpaceProps Clone()
    {
        return new SpaceProps(_position, _basis, _curve.Clone());
    }
    
    public Vector3 Position
    {
        get { return _position; }
        set { _position = value; }
    }

    public Matrix4x4 Basis
    {
        get { return _basis; }
        set { _basis = value; }
    }

    public Bezier Curve
    {
        get { return _curve; }
        set { _curve = value; }
    }
}
