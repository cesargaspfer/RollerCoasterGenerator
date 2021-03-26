using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Algebra;
using static ImaginarySphere;
using static RailModelProperties;

[System.Serializable]
public class Constructor
{
    private RollerCoaster _rollerCoaster;

    [SerializeField] private RailProps _currentGlobalrp;
    [SerializeField] private RailProps _lastGlobalrp;
    [SerializeField] private ModelProps _mp;
    [SerializeField] private float _totalLength;    
    [SerializeField] private Vector3 _position;
    [SerializeField] private Matrix4x4 _basis;
    [SerializeField] private Vector3 _finalPosition;
    [SerializeField] private Matrix4x4 _finalBasis;

    [SerializeField] private List<Rail> _rails;
    [SerializeField] private List<Rail> _railsIntersection;
    private Rail _currentRail;

    private Vector3 _initialPosition;
    private Matrix4x4 _initialBasis;
    [SerializeField] private RailProps _initialGlobalrp;
    [SerializeField] private int _heatmapValue;

    public Constructor(RollerCoaster rollerCoaster, RailProps rp, ModelProps mp, SpaceProps sp)
    {
        _rollerCoaster = rollerCoaster;
        SaveManager.SetPaths();
        _lastGlobalrp = new RailProps(0f, 0f, 0f, rp.Length);
        _currentGlobalrp = rp.Clone();
        _initialGlobalrp = rp.Clone();
        _mp = mp;
        _totalLength = 0f;
        _rails = new List<Rail>();
        _railsIntersection = new List<Rail>();
        _currentRail = null;
        _position = sp.Position;
        _basis = sp.Basis;
        _initialPosition = sp.Position;
        _initialBasis = sp.Basis;
        _finalPosition = sp.Position;
        _finalBasis = sp.Basis;
        _heatmapValue = 0;
    }

    public Rail AddRail(bool isPreview)
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

        if(_rails.Count > 0)
        {
            _railsIntersection.Add(_rails[_rails.Count - 1]);
        }


        if (_rails.Count >= 1)
        {
            _totalLength += _rails[_rails.Count - 1].rp.Length;
        }
        _currentRail = new Rail(this, localrp, _mp, sp, 0, _totalLength);

        _rails.Add(_currentRail);

        if(isPreview)
        {
            SetRailPreview(_rails.Count - 1, isPreview);
        }

        _currentRail.SetHeatmap(_heatmapValue);
        // Debug.Log("Intersected: " + CheckIntersection(_currentRail));

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

        int isFinalRail = 0;
        sp.Curve = curve;
        if(radius != -1)
        {
            localrp.Length = curve.Length;
            rp.Length = curve.Length;
            _currentGlobalrp.Length = curve.Length;
            isFinalRail = 1;
        }

        _currentRail.UpdateRail(rp:localrp, mp: mp, sp: sp, isFinalRail:isFinalRail);

        if(_currentRail.PreviewMode > 0)
            SetRailPreview(_rails.Count -1, true);

        _currentRail.SetHeatmap(_heatmapValue);

        // Debug.Log("Intersected: " + CheckIntersection(_currentRail));

        return _currentRail;
    }

    public Rail UpdateLastRailAdd(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
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

    public Rail UpdateLastRail(float elevation = -999f, float rotation = -999f, float inclination = -999f, float length = -999, int railType = -999)
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
        if (_railsIntersection.Count > 0)
            _railsIntersection.RemoveAt(_rails.Count - 1);

        if (_rails.Count > 0)
            _totalLength -= _rails[_rails.Count - 1].rp.Length;

        if(_rails.Count > 0)
        {
            _currentRail = _rails[_rails.Count - 1];

            _position = _currentRail.sp.Position;
            _basis = _currentRail.sp.Basis;

            _finalPosition = removedRail.sp.Position;
            _finalBasis = removedRail.sp.Basis;

            _currentGlobalrp = _lastGlobalrp;
            _currentGlobalrp.Length = _currentRail.rp.Length;
            _lastGlobalrp -= _currentRail.rp;
            _lastGlobalrp.Length = _currentRail.rp.Length;

            _mp = _currentRail.mp;
            
            this.UpdateLastRail(rp:_currentGlobalrp, mp:_mp);
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

    public List<Rail> AddBlueprint(List<(RailProps, RailType)> rails, bool isPreview)
    {
        List<Rail> newRails = new List<Rail>();
        for(int i = 0; i < rails.Count; i++)
        {
            (RailProps rp, RailType rt) = rails[i];

            newRails.Add(AddRail(isPreview));
            UpdateLastRailAdd(elevation: rp.Elevation, rotation: rp.Rotation, inclination: rp.Inclination, railType: (int) rt);
            UpdateLastRail(length: rp.Length);
        }
        return newRails;
    }

    public void GenerateSupports(int id)
    {
        _rails[id].GenerateSupports();
    }

    public void RemoveSupports(int id)
    {
        _rails[id].RemoveSupports();
    }

    public void SetRailPreview(int id, bool isPreview)
    {
        int canPlace = 0;
        if (isPreview)
        {
            canPlace = CanPlace(_rails[id]) ? 1 : 2;
        }
        _rails[id].SetPreview(canPlace);
    }

    public bool CanPlace(Rail rail)
    {
        return !CheckIntersection(rail);
    }

    public bool CheckRailPlacement(int id)
    {
        if (_rails[id].PreviewMode > 0)
        {
            return _rails[id].PreviewMode == 1;
        }
        else
        {
            return CanPlace(_rails[id]);
        }
    }

    public bool CheckLastRailPlacement()
    {
        if(_currentRail.PreviewMode > 0)
        {
            return _currentRail.PreviewMode == 1;
        }
        else
        {
            return CanPlace(_currentRail);
        }
    }

    public bool CanAddFinalRail()
    {
        Vector3 position = _rails[_rails.Count - 2].sp.Position;
        if(GetSignedDistanceFromPlane(_initialBasis.GetColumn(0), _initialPosition, position) >= 0)
            return false;
        if(Angle(_rails[_rails.Count - 2].GetBasisAt(1f).GetColumn(0), _initialPosition - position) > Mathf.PI * 0.25)
            return false;
        return true;
    }

    public void TestAddFinalRail(Vector3 position, Matrix4x4 basis)
    {
        RemoveLastRail();
        RemoveLastRail();
        RemoveLastRail();
        AddRail(false);
        _position = position;
        _basis = basis;
        _initialPosition = Vector3.up;
        _initialBasis = Matrix4x4.identity;
        // _initialBasis[0, 0] = 0f;
        // _initialBasis[0, 1] = 1f;
        // _initialBasis[1, 0] = -1f;
        // _initialBasis[1, 1] = 0f;
        AddFinalRail();
    }

    public (Rail, Rail) AddFinalRail(int railType = -1)
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
        RailProps firstrp = new RailProps(firstElevation, firstRotation, 0f, -1);

        basis = ThreeRotationMatrix(basis, firstrp.Radians) * basis;

        (float secondElevation, float secondRotation) = GetRotationAngles(basis, basis.GetColumn(0), -nf);

        RailProps secondrp = new RailProps(secondElevation, secondRotation, 0f, -1);
        basis = ThreeRotationMatrix(basis, secondrp.Radians) * basis;

        float inclination = Angle(basis.GetColumn(1), _initialBasis.GetColumn(1));
        // if (inclination >= 3.1415)
        //     inclination = 0;
        Matrix4x4 rotationMatrix = RotationMatrix(inclination, _initialBasis.GetColumn(0));
        if(((Vector3)_initialBasis.GetColumn(1) - rotationMatrix.MultiplyPoint3x4(basis.GetColumn(1))).magnitude > 0.001f)
            inclination = -inclination;
        inclination *= 0.5f;
        firstrp.Inclination = inclination;


        basis = ThreeRotationMatrix(_basis, firstrp.Radians) * _basis;
        (secondElevation, secondRotation) = GetRotationAngles(basis, basis.GetColumn(0), -nf);
        secondrp = new RailProps(secondElevation, secondRotation, 0f, -1);
        basis = ThreeRotationMatrix(basis, secondrp.Radians) * basis;
        inclination = Angle(basis.GetColumn(1), _initialBasis.GetColumn(1));
        // if (inclination >= 3.1415)
        //     inclination = 0;
        rotationMatrix = RotationMatrix(inclination, _initialBasis.GetColumn(0));
        if (((Vector3)_initialBasis.GetColumn(1) - rotationMatrix.MultiplyPoint3x4(basis.GetColumn(1))).magnitude > 0.001f)
            inclination = -inclination;
        secondrp.Inclination = inclination;

        if(railType != -1)
        {
            _mp = _mp.Clone();
            _mp.Type = (RailModelProperties.RailType) railType;
        }

        this.SetRailPreview(_rails.Count - 1, false);
        Rail rail1 = this.UpdateLastRail(rp: firstrp + _lastGlobalrp, mp: _mp, radius: radius);
        this.AddRail(false);
        Rail rail2 = this.UpdateLastRail(rp: secondrp + _lastGlobalrp, mp: _mp, radius: radius);

        // Sphere debug
        // GameObject mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // mySphere.transform.localScale = Vector3.one * 2f * radius;
        // mySphere.transform.position = pi + radius * ni;

        // mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // mySphere.transform.localScale = Vector3.one * 2f * radius;
        // mySphere.transform.position = pf + radius * nf;

        return (rail1, rail2);
    }

    public bool CheckIntersection(Rail rail)
    {
        // TODO: Use k-d Tree
        for(int i = 1; i < _railsIntersection.Count - 1; i++)
        {
            Rail r2 = _railsIntersection[i];
            float ta1 = 0f;
            float tb1 = 1f;
            float ta2 = 0f;
            float tb2 = 1f;

            Bezier c1 = rail.sp.Curve;
            Bezier c2 = r2.sp.Curve;

            float minD = 999f;

            while(true)
            {
                Vector3 a1 = c1.Sample(ta1);
                Vector3 b1 = c1.Sample(tb1);
                Vector3 a2 = c2.Sample(ta2);
                Vector3 b2 = c2.Sample(tb2);

                float d1 = (a1 - a2).magnitude;
                float d2 = (a1 - b2).magnitude;
                float d3 = (b1 - a2).magnitude;
                float d4 = (b1 - b2).magnitude;

                minD = Mathf.Min(new float[4] { d1, d2, d3, d4 });

                float tm1 = (ta1 + tb1) * 0.5f;
                float tm2 = (ta2 + tb2) * 0.5f;

                if (d1 == minD)
                {
                    tb1 = tm1;
                    tb2 = tm2;
                }
                else if (d2 == minD)
                {
                    tb1 = tm1;
                    ta2 = tm2;
                }
                else if (d3 == minD)
                {
                    ta1 = tm1;
                    tb2 = tm2;
                }
                else if (d4 == minD)
                {
                    ta1 = tm1;
                    ta2 = tm2;
                }

                if(((a1 - b1).magnitude < 0.1f && (a2 - b2).magnitude < 0.1f) || minD < 2f)
                    break;
            }
            if(minD < 2f)
                return true;
        }
        return false;
    }

    public void SetHeatmap(int type)
    {
        _heatmapValue = type;
        for (int i = 0; i < _rails.Count; i++)
        {
            _rails[i].SetHeatmap(type);
        }
    }

    public void StartChildCoroutine(IEnumerator coroutine)
    {
        _rollerCoaster.StartChildCoroutine(coroutine);
    }

    public void StopChildCoroutine(IEnumerator coroutine)
    {
        _rollerCoaster.StopChildCoroutine(coroutine);
    }

    public GameObject InstantiateRail(Mesh mesh, Material material, Vector3 position, bool isHeatmap = false, bool isSupport = false)
    {
        return _rollerCoaster.InstantiateRail(mesh, material, position, isHeatmap: isHeatmap, isSupport: isSupport).gameObject;
    }

    public RailProps CurrentGlobalrp
    {
        get { return _currentGlobalrp; }
    }

    public RailProps LastGlobalrp
    {
        get { return _lastGlobalrp; }
    }

    public List<Rail> Rails
    {
        get { return _rails; }
    }

    public Rail CurrentRail
    {
        get { return _currentRail; }
    }

    public Vector3 InitialPosition
    {
        get { return _initialPosition; }
    }

    public Matrix4x4 InitialBasis
    {
        get { return _initialBasis; }
    }

    public Vector3 FinalPosition
    {
        get { return _finalPosition; }
    }

    public Matrix4x4 FinalBasis
    {
        get { return _finalBasis; }
    }

    public float TotalLength
    {
        get { return _totalLength; }
    }    
}
