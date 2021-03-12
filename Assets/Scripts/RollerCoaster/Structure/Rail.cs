using UnityEngine;
using static Algebra;
using static RailModelManager;

[System.Serializable]
public class Rail
{
    public enum CarStatus
    {
        Backward,
        In, 
        Foward,
    }

    [System.NonSerialized]
    private Constructor _constructor;

    [SerializeField] private RailProps _rp;
    [SerializeField] private ModelProps _mp;
    [SerializeField] private SpaceProps _sp;
    [SerializeField] private bool _isFinalRail;
    [SerializeField] private float _radius;
    
    private Mesh[] _mesh = null;
    private RailModel _railModel = null;
    private GameObject[] _gameObject;
    private float _lastLength;

    private float _inclinationToMatrixLookAt;

    public Rail(Constructor constructor, RailProps rp, ModelProps mp, SpaceProps sp, int isFinalRail)
    {
        _constructor = constructor;
        _lastLength = -1f;
        this.UpdateRail(rp, mp, sp, isFinalRail);
    }

    public void Destroy()
    {
        for(int i = 0; i < _gameObject.Length; i++)
            GameObject.Destroy(_gameObject[i]);
        _mesh = null;
        _railModel = null;
        _rp = null;
        _mp = null;
        _sp = null;
    }

    public void UpdateRail(RailProps rp = null, ModelProps mp = null, SpaceProps sp = null, int isFinalRail = -1)
    {
        if (rp != null)
            _rp = rp.Clone();
        if (mp != null)
            _mp = mp.Clone();
        if (sp != null)
            _sp = sp.Clone();
        if(isFinalRail != -1)
            _isFinalRail = isFinalRail == 1;

        _railModel = GetRailModel(_mp.ModelId);

        if (_gameObject == null || mp != null || _lastLength != _rp.Length)
        {
            _mesh = null;
        }

        Material[] materials = null;

        (_mesh, materials) = _railModel.GenerateMeshes(_rp, _mp, _sp, _mesh);

        if(_gameObject == null || mp != null || _lastLength != _rp.Length)
        {
            if(_gameObject != null)
            {
                // Applying _mesh to an object in the scene
                for (int i = 0; i < _gameObject.Length; i++)
                    GameObject.Destroy(_gameObject[i]);
            }
            _lastLength = _rp.Length;
            _gameObject = new GameObject[_mesh.Length];
            for(int i = 0; i < _gameObject.Length; i++)
                _gameObject[i] = _constructor.InstantiateRail(_mesh[i], materials[i], _sp.Position);
        }

        _inclinationToMatrixLookAt = GetInclinationToMatrixLookAt(_sp.Basis, ThreeRotationMatrix(_sp.Basis, _rp.Radians) * _sp.Basis);

        Vector3 x0 = GetBasisAt(0f).GetColumn(0);
        Vector3 x1 = GetBasisAt(1f).GetColumn(0);

        _radius = _rp.Length / Angle(x0, x1);
        Debug.Log(_radius);
    }

    public CarStatus IsInRail(float position)
    {
        if (position >= _rp.Length)
            return CarStatus.Foward;
        if (position < 0)
            return CarStatus.Backward;
        return CarStatus.In;
    }

    public (Vector3, CarStatus) GetPositionInRail(float position)
    {
        if(position >= _rp.Length)
            return (Vector3.zero, CarStatus.Foward);
        if(position < 0)
            return (Vector3.zero, CarStatus.Backward);
        float t = position / (float) _rp.Length;
        return (_sp.Curve.Sample(t), CarStatus.In);
    }
    
    public Matrix4x4 GetBasisAt(float t)
    {
        // float t = position / (float)_rp.Length;
        Matrix4x4 rotationMatrix = ThreeRotationMatrix(sp.Basis, t * _rp.Radians);
        // Matrix4x4 rotationMatrix = MatrixLookAt(sp.Basis, sp.Curve.GetTangentAt(t), t * _inclinationToMatrixLookAt);
        Matrix4x4 finalMatrix = rotationMatrix * _sp.Basis;
        return finalMatrix;
    }
    
    public Matrix4x4 GetRotationBasisAt(float t)
    {
        Matrix4x4 basis = GetBasisAt(t);
        Matrix4x4 finalMatrix = OPBTMatrix(Matrix4x4.identity, basis);
        return finalMatrix;
    }

    public Quaternion GetQuaternionAt(float t)
    {
        Matrix4x4 rotationMatrix = GetRotationBasisAt(t);
        return ExtractRotationFromMatrix(ref rotationMatrix);
    }

    public Vector3 GetTangentAt(float position)
    {
        float t = position / (float) _rp.Length;
        return _sp.Curve.GetTangentAt(t);
    }

    public RailProps rp
    {
        get { return _rp; }
    }

    public ModelProps mp
    {
        get { return _mp; }
    }

    public SpaceProps sp
    {
        get { return _sp; }
    }

    public bool IsFinalRail
    {
        get { return _isFinalRail; }
    }

    public float Radius
    {
        get { return _radius; }
    }
}
