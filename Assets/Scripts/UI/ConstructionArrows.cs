using UnityEngine;
using static Algebra;
using static RailModelManager;
using static ImaginarySphere;

public class ConstructionArrows : MonoBehaviour
{
    private static ConstructionArrows _inst;
    public static ConstructionArrows inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<ConstructionArrows>();
            return _inst;
        }
    }

    #pragma warning disable 0649
    [SerializeField] private Camera _camera;
    private RollerCoaster _rc;
    private bool _isActive = false;
    private float _lastLength = -1;


    private bool _mouseButton0Down = false;
    private RailProps _lastGlobalrp;
    private RailProps _currentGlobalrp;
    private Vector3 _previousMousePosition = Vector3.zero;
    private Vector2 _dirX = Vector3.zero;
    private Vector2 _dirY = Vector3.zero;
    private Vector2 _dirZ = Vector3.zero;
    private Vector2 _dirInclination = Vector3.zero;
    private float _lastValue;
    
    public void Initialize(RollerCoaster rollerCoaster)
    {
        _rc = rollerCoaster;
        _lastLength = -1;
        ActiveArrows(true);
        UpdateArrows();
        _mouseButton0Down = false;
    }

    public void UpdateArrows()
    {
        Rail rail = _rc.GetLastRail();
        this.transform.position = _rc.GetFinalPosition();

        RailProps currentrp = _rc.GetCurrentGlobalrp();
        RailProps tmprp = new RailProps(currentrp.Elevation, currentrp.Rotation, 0f, 1f);
        Matrix4x4 rotationMatrix = ThreeRotationMatrix(Matrix4x4.identity, tmprp.Radians);
        this.transform.GetChild(0).transform.rotation = ExtractRotationFromMatrix(ref rotationMatrix);

        currentrp = _rc.GetCurrentGlobalrp();
        if(Mathf.Cos(currentrp.Elevation) >= -0.05f)
            tmprp = new RailProps(0f, currentrp.Rotation, 0f, 1f);
        else
            tmprp = new RailProps(0f, currentrp.Rotation + Mathf.PI, 0f, 1f);
        rotationMatrix = ThreeRotationMatrix(Matrix4x4.identity, tmprp.Radians);
        this.transform.GetChild(1).transform.rotation = ExtractRotationFromMatrix(ref rotationMatrix);

        this.transform.GetChild(2).transform.rotation = rail.GetQuaternionAt(1f);
        this.transform.GetChild(3).transform.rotation = rail.GetQuaternionAt(1f);

        // Vector3 tmpEulerAngles = this.transform.GetChild(2).transform.eulerAngles;
        // tmpEulerAngles = new Vector3(-this.transform.eulerAngles.x + 90f, tmpEulerAngles.y, -this.transform.eulerAngles.z);
        // this.transform.GetChild(2).transform.eulerAngles = tmpEulerAngles;
        // tmpEulerAngles = this.transform.GetChild(3).transform.eulerAngles;
        // tmpEulerAngles = new Vector3(-this.transform.eulerAngles.x + 90f, tmpEulerAngles.y, -this.transform.eulerAngles.z);
        // this.transform.GetChild(3).transform.eulerAngles = tmpEulerAngles;

        if(_lastLength != rail.rp.Length)
        {
            _lastLength = rail.rp.Length;

            // TODO: Improve arrows length
            // RailProps rp = new RailProps(Mathf.PI * 0.25f, 0f, 0f, 0.9f * _lastLength);
            RailProps rp = new RailProps(Mathf.PI * 0.25f, 0f, 0f, 5f + 0.05f * _lastLength);
            ModelProps mp = new ModelProps(2, 0, 7);
            SpaceProps sp = new SpaceProps(Vector3.zero, Matrix4x4.identity);
            (Bezier curve, Matrix4x4 finalBasis, Vector3 finalPosition) = CalculateImaginarySphere(sp, rp);
            sp.Curve = curve;


            RailModel railModel = GetRailModel(2);
            Material[] materials = null;
            Mesh[] mesh = null;

            (mesh, materials) = railModel.GenerateMeshes(rp, mp, sp, mesh);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    this.transform.GetChild(i).GetChild(j).GetComponent<MeshFilter>().mesh = mesh[0];
                    this.transform.GetChild(i).GetChild(j).GetComponent<MeshCollider>().sharedMesh = mesh[0];
                    this.transform.GetChild(i).GetChild(j).GetComponent<MeshCollider>().convex = true;
                }
            }

            // TODO: Improve arrows length
            // this.transform.GetChild(4).transform.localPosition = new Vector3(0f, _lastLength, 0f);
            // this.transform.GetChild(5).transform.localPosition = new Vector3(0f, _lastLength, 0f);
            this.transform.GetChild(2).GetChild(0).transform.localPosition = new Vector3(0f, 5f + 0.05f * _lastLength, 0f);
            this.transform.GetChild(2).GetChild(1).transform.localPosition = new Vector3(0f, 5f + 0.05f * _lastLength, 0f);

            rp = new RailProps(0f, 0f, 0f, 5f + 0.05f * _lastLength);
            (curve, finalBasis, finalPosition) = CalculateImaginarySphere(sp, rp);
            sp.Curve = curve;
            materials = null;
            mesh = null;
            (mesh, materials) = railModel.GenerateMeshes(rp, mp, sp, mesh);
            this.transform.GetChild(3).GetChild(0).GetComponent<MeshFilter>().mesh = mesh[0];
            this.transform.GetChild(3).GetChild(0).GetComponent<MeshCollider>().sharedMesh = mesh[0];
            this.transform.GetChild(3).GetChild(0).GetComponent<MeshCollider>().convex = true;
            this.transform.GetChild(3).GetChild(1).GetComponent<MeshFilter>().mesh = mesh[1];
        }

        UIManager.inst.UpdateUIValues();

    }

    public void ActiveArrows(bool active)
    {
        if(_isActive == active && active)
        {
            if (_rc.GetLastGlobalrp().Inclination <= -0.025f || _rc.GetLastGlobalrp().Inclination >= 0.025f)
            {
                this.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                this.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                this.transform.GetChild(0).GetChild(0).gameObject.SetActive(active);
                this.transform.GetChild(0).GetChild(1).gameObject.SetActive(active);
            }
        }
        else
        {
            _isActive = active;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 2; j++)
                this.transform.GetChild(i).GetChild(j).gameObject.SetActive(active);

            if (active && (_rc.GetLastGlobalrp().Inclination <= -0.025f || _rc.GetLastGlobalrp().Inclination >= 0.025f))
            {
                this.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                this.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
        }
    }



    public void OnPointerDownOnArrow(int arrowId)
    {
        if (!Input.GetMouseButton(0)) return;
        _mouseButton0Down = true;

        ActiveOtherArrows(false, arrowId);

        UpdateOrientations();

        if(arrowId == 0)
            _lastValue = _currentGlobalrp.Elevation;
        else if (arrowId == 1)
            _lastValue = _currentGlobalrp.Rotation;
        else if (arrowId == 2)
            _lastValue = _currentGlobalrp.Inclination;
        else if (arrowId == 3)
            _lastValue = _currentGlobalrp.Length;
    }

    public void UpdateOrientations()
    {
        Vector3 pos = _rc.GetFinalPosition();
        Matrix4x4 basis = _rc.GetFinalBasis();

        RailProps currentrp = _rc.GetCurrentGlobalrp();
        RailProps tmprp = new RailProps(currentrp.Elevation, currentrp.Rotation, 0f, 1f);
        Matrix4x4 rotationMatrix = ThreeRotationMatrix(Matrix4x4.identity, tmprp.Radians);
        
        Vector3 worldY;
        if (Mathf.Cos(_rc.GetLastGlobalrp().Inclination) >= -0.05f)
            worldY = pos + (Vector3)rotationMatrix.GetColumn(1);
        else
            worldY = pos - (Vector3)rotationMatrix.GetColumn(1);

        currentrp = _rc.GetCurrentGlobalrp();
        tmprp = new RailProps(0f, currentrp.Rotation, 0f, 1f);
        rotationMatrix = ThreeRotationMatrix(Matrix4x4.identity, tmprp.Radians);
        // this.transform.GetChild(1).transform.rotation = ExtractRotationFromMatrix(ref rotationMatrix);

        Vector3 worldZ;
        if (Mathf.Cos(currentrp.Elevation) >= -0.05f)
            worldZ = pos + (Vector3)rotationMatrix.GetColumn(2);
        else
            worldZ = pos - (Vector3)rotationMatrix.GetColumn(2);
        

        Vector3 worldX = pos + (Vector3)basis.GetColumn(0);

        Vector2 viewportPos = _camera.ScreenToViewportPoint(_camera.WorldToScreenPoint(pos));
        Vector2 viewportX = _camera.ScreenToViewportPoint(_camera.WorldToScreenPoint(worldX));
        Vector2 viewportY = _camera.ScreenToViewportPoint(_camera.WorldToScreenPoint(worldY));
        Vector2 viewportZ = _camera.ScreenToViewportPoint(_camera.WorldToScreenPoint(worldZ));

        _dirX = (viewportPos - viewportX);
        _dirY = (viewportPos - viewportY).normalized;
        _dirZ = (viewportPos - viewportZ).normalized;

        tmprp = new RailProps(currentrp.Elevation, currentrp.Rotation, currentrp.Inclination, 1f);
        rotationMatrix = ThreeRotationMatrix(Matrix4x4.identity, tmprp.Radians);
        Vector3 worldInclination = pos + (Vector3)rotationMatrix.GetColumn(2);
        Vector2 viewportInclination = _camera.ScreenToViewportPoint(_camera.WorldToScreenPoint(worldInclination));

        _dirInclination = (viewportPos - viewportInclination).normalized;

        _previousMousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
        _lastGlobalrp = _rc.GetLastGlobalrp().Clone();
        _currentGlobalrp = _rc.GetCurrentGlobalrp().Clone() - _lastGlobalrp;

    }

    public void OnDragOnArrow(int arrowId)
    {
        if (!Input.GetMouseButton(0)) return;
        
        Vector3 mousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
        Vector2 mouseoffset = _previousMousePosition - mousePosition;

        
        bool changed = false;
        if (arrowId == 0)
        {
            if (_dirY.magnitude == 0f) return;
            float scalarProjY = Vector2.Dot(mouseoffset, _dirY);
            Vector2 projectedY = scalarProjY * _dirY;
            float magY = Mathf.Sign(scalarProjY) * projectedY.magnitude;

            float angle =  magY * 2f;
            float currentAngle = _currentGlobalrp.Elevation / Mathf.PI;
            angle = Mathf.Max(-0.5f, Mathf.Min(0.5f, currentAngle + angle));
            angle = (Mathf.Round(angle * 12f) / 12f) * Mathf.PI;

            if(angle != _lastValue)
            {
                _rc.UpdateLastRail(elevation: _lastGlobalrp.Elevation + angle, simulateRail:true);
                _lastValue = angle;
                changed = true;
            }
        }
        else if (arrowId == 1)
        {
            if (_dirZ.magnitude == 0f) return;
            float scalarProjZ = Vector2.Dot(mouseoffset, _dirZ);        
            Vector2 projectedZ = scalarProjZ * _dirZ;
            float magZ = Mathf.Sign(scalarProjZ) * projectedZ.magnitude;

            float angle = -magZ * 2f;
            float currentAngle = _currentGlobalrp.Rotation / Mathf.PI;
            angle = Mathf.Max(-0.5f, Mathf.Min(0.5f, currentAngle + angle));
            angle = (Mathf.Round(angle * 12f) / 12f) * Mathf.PI;

            if (angle != _lastValue)
            {
                _rc.UpdateLastRail(rotation: _lastGlobalrp.Rotation + angle, simulateRail:true);
                _lastValue = angle;
                changed = true;
            }
        }
        else if (arrowId == 2)
        {
            if (_dirInclination.magnitude == 0f) return;
            float scalarProjZ = Vector2.Dot(mouseoffset, _dirInclination);        
            Vector2 projectedZ = scalarProjZ * _dirInclination;
            float magZ = Mathf.Sign(scalarProjZ) * projectedZ.magnitude;

            float angle = magZ * 2f;
            float currentAngle = _currentGlobalrp.Inclination / Mathf.PI;
            angle = Mathf.Max(-0.5f, Mathf.Min(0.5f, currentAngle + angle));
            angle = (Mathf.Round(angle * 12f) / 12f) * Mathf.PI;

            if (angle != _lastValue)
            {
                _rc.UpdateLastRail(inclination: _lastGlobalrp.Inclination + angle, simulateRail:true);
                _lastValue = angle;
                changed = true;
            }
        }
        else if (arrowId == 3)
        {
            if (_dirX.magnitude == 0f) return;
            float scalarProjX = Vector2.Dot(mouseoffset, _dirX);
            Vector2 projectedX = scalarProjX * _dirX / (_dirX.magnitude * _dirX.magnitude);
            float magX = Mathf.Sign(scalarProjX) * projectedX.magnitude;

            float length = magX / _dirX.magnitude;
            float currentLength = _currentGlobalrp.Length;

            length = Mathf.Max(5f, currentLength + length);
            length = Mathf.Round(length);

            if (length != _lastValue)
            {
                _rc.UpdateLastRail(length: length, simulateRail:true);
                _lastValue = length;
                changed = true;
            }
        }

        if (changed)
        {
            UpdateArrows();
        }
    }

    public void OnPointerUpOnArrow(int arrowId)
    {
        if (!_mouseButton0Down || Input.GetMouseButton(0)) return;
        _mouseButton0Down = false;

        ActiveOtherArrows(true, arrowId);
    }

    public void ActiveOtherArrows(bool active, int except)
    {
        ActiveArrows(active);
        this.transform.GetChild(except).GetChild(0).gameObject.SetActive(true);
        this.transform.GetChild(except).GetChild(1).gameObject.SetActive(true);
    }

    public bool IsActive
    {
        get { return _isActive; }
    }

}
