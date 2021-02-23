using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static ImaginarySphere;

[System.Serializable]
public class Constructor
{
    private RollerCoaster _rollerCoaster;

    [SerializeField] private RailProps _currentGlobalrp;
    [SerializeField] private RailProps _lastGlobalrp;
    [SerializeField] private ModelProps _mp;
    [SerializeField] private float _length;
    [SerializeField] private Vector3 _position;
    [SerializeField] private Matrix4x4 _basis;
    [SerializeField] private Vector3 _finalPosition;
    [SerializeField] private Matrix4x4 _finalBasis;

    [SerializeField] private List<Rail> _rails;
    private Rail _currentRail;

    private Vector3 _initialPosition;
    private Matrix4x4 _initialBasis;
    [SerializeField] private RailProps _initialGlobalrp;

    public Constructor(RollerCoaster rollerCoaster, RailProps rp, ModelProps mp, SpaceProps sp)
    {
        _rollerCoaster = rollerCoaster;
        _lastGlobalrp = new RailProps(0f, 0f, 0f, rp.Length);
        _currentGlobalrp = rp.Clone();
        _initialGlobalrp = rp.Clone();
        _mp = mp;
        _length = rp.Length;
        _rails = new List<Rail>();
        _currentRail = null;
        _position = sp.Position;
        _basis = sp.Basis;
        _initialPosition = sp.Position;
        _initialBasis = sp.Basis;
        _finalPosition = sp.Position;
        _finalBasis = sp.Basis;

        // TODO: others properties
    }

    public Rail AddRail()
    {
        _position = _finalPosition;
        _basis = _finalBasis;
        
        _lastGlobalrp = _currentGlobalrp.Clone();

        RailProps localrp = _currentGlobalrp - _lastGlobalrp;
        SpaceProps sp = new SpaceProps(_position, _basis);
        (Bezier curve, Matrix4x4 finalBasis, Vector3 finalPosition) = CalculateImaginarySphere(sp, localrp);

        _finalPosition = finalPosition;
        _finalBasis = finalBasis;

        sp.Curve = curve;


        _currentRail = new Rail(this, localrp, _mp, sp);

        _rails.Add(_currentRail);
        
        return _currentRail;
    }

    public Rail UpdateLastRail(RailProps rp = null, ModelProps mp = null, float radius = -1f)
    {
        if (_rails.Count == 0)
            return null;

        if (rp != null)
            _currentGlobalrp = rp;
        if (mp != null)
            _mp = mp;


        RailProps localrp = _currentGlobalrp - _lastGlobalrp;
        SpaceProps sp = new SpaceProps(_position, _basis);
        (Bezier curve, Matrix4x4 finalBasis, Vector3 finalPosition) = CalculateImaginarySphere(sp, localrp, radius);

        _finalPosition = finalPosition;
        _finalBasis = finalBasis;

        sp.Curve = curve;
        if(radius != -1)
        {
            localrp.Length = curve.Length;
        }

        _currentRail.UpdateRail(rp:localrp, mp: mp, sp: sp);

        return _currentRail;
    }

    public Rail UpdateLastRailAdd(float elevation = -999f, float rotation = -999f, float inclination = -999f, int length = -999, int railType = -999)
    {
        if (_rails.Count == 0)
            return null;

        ModelProps mp = null;

        if (elevation != -999f)
            _currentGlobalrp.Elevation += elevation;
        if (rotation != -999f)
            _currentGlobalrp.Rotation += rotation;
        if (inclination != -999f)
            _currentGlobalrp.Inclination += inclination;
        if (length != -999)
            _currentGlobalrp.Length += length;
        if (railType != -999)
        {
            _mp.Type = (RailModelProperties.RailType)railType;
            mp = _mp;
        }

        this.UpdateLastRail(rp: _currentGlobalrp, mp: mp);

        return _currentRail;
    }

    public Rail UpdateLastRail(float elevation = -999f, float rotation = -999f, float inclination = -999f, int length = -999, int railType = -999)
    {
        if (_rails.Count == 0)
            return null;

        ModelProps mp = null;

        if (elevation != -999f)
            _currentGlobalrp.Elevation = elevation;
        if (rotation != -999f)
            _currentGlobalrp.Rotation = rotation;
        if (inclination != -999f)
            _currentGlobalrp.Inclination = inclination;
        if (length != -999)
            _currentGlobalrp.Length = length;
        if (railType != -999)
        {
            _mp.Type = (RailModelProperties.RailType) railType;
            mp = _mp;
        }

        this.UpdateLastRail(rp:_currentGlobalrp, mp:mp);

        return _currentRail;
    }

    public (RailProps, ModelProps) RemoveLastRail()
    {
        if(_rails.Count <= 0)
            return (null, null);

        Rail removedRail = _rails[_rails.Count - 1];
        _rails.RemoveAt(_rails.Count - 1);

        if(_rails.Count > 0)
        {
            _currentRail = _rails[_rails.Count - 1];

            _position = _currentRail.sp.Position;
            _basis = _currentRail.sp.Basis;

            _finalPosition = removedRail.sp.Position;
            _finalBasis = removedRail.sp.Basis;

            _currentGlobalrp = _lastGlobalrp;
            _lastGlobalrp -= _currentRail.rp;

            _mp = _currentRail.mp;
            
            this.UpdateLastRail(rp:_currentGlobalrp);
        }
        else
        {
            _currentRail = null;

            _position = _initialPosition;
            _basis = _initialBasis;

            _finalPosition = _initialPosition;
            _finalBasis = _initialBasis;

            _currentGlobalrp = _initialGlobalrp;
            _lastGlobalrp = new RailProps(0f, 0f, 0f, 5f);
        }

        removedRail.Destroy();

        return (_currentGlobalrp, _mp);
    }

    public (Rail, Rail) AddFinalRail()
    {
        // TODO: Check if can finish track

        Vector3 pi = _position;
        Vector3 ni = _basis.GetColumn(0);
        Vector3 pf = _initialPosition;
        Vector3 nf = - _initialBasis.GetColumn(0);

        Vector3 p = pf - pi;
        Vector3 n = nf - ni;

        float a = 4f - Vector3.Dot(n, n);
        float b = - 2f * Vector3.Dot(p, n);
        float c = - Vector3.Dot(p, p);

        // TODO: Improve this quadratic equation solver
        float radius = -1f;
        if(Mathf.Abs(a) > 0.00001f)
            radius = (-b + Mathf.Sqrt((b * b) - 4f * a * c)) / (2f * a);
        else
            radius =  - c / b;
        

        Vector3 c1 = pi + radius * ni;
        Vector3 c2 = pf + radius * nf;
        Vector3 r = (c2 - c1) * 0.5f;
        Vector3 rn = r.normalized;

        Matrix4x4 basis = _basis;
        (float firstElevation, float firstRotation) = GetRotationAngles(basis, basis.GetColumn(0), rn);
        RailProps firstrp = new RailProps(firstElevation, firstRotation, 0f, _currentGlobalrp.Length);

        basis = ThreeRotationMatrix(basis, firstrp.Radians) * basis;

        (float secondElevation, float secondRotation) = GetRotationAngles(basis, basis.GetColumn(0), -nf);

        RailProps secondrp = new RailProps(secondElevation, secondRotation, 0f, _currentGlobalrp.Length);
        basis = ThreeRotationMatrix(basis, secondrp.Radians) * basis;

        float inclination = 0.5f * Angle(basis.GetColumn(1), _initialBasis.GetColumn(1));
        if (inclination >= 3.1415)
            inclination = 0;
        firstrp.Inclination = inclination;
        secondrp.Inclination = inclination;

        Rail rail1 = this.UpdateLastRail(rp: _currentGlobalrp + firstrp, radius: radius);
        this.AddRail();
        Rail rail2 = this.UpdateLastRail(rp: _currentGlobalrp + secondrp, radius: radius);

        // Sphere debug
        // GameObject mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // mySphere.transform.localScale = Vector3.one * 2f * radius;
        // mySphere.transform.position = pi + radius * ni;

        // mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // mySphere.transform.localScale = Vector3.one * 2f * radius;
        // mySphere.transform.position = pf + radius * nf;

        return (rail1, rail2);
    }

    public GameObject InstantiateRail(Mesh mesh, Material material)
    {
        return _rollerCoaster.InstantiateRail(mesh, material).gameObject;
    }

    public RailProps CurrentGlobalrp
    {
        get { return _currentGlobalrp; }
    }

    public List<Rail> Rails
    {
        get { return _rails; }
    }
}
