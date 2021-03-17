using System.Collections;
using UnityEngine;
using static Algebra;
using static RailModelManager;
using static SupportsManager;

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
    [SerializeField] private int _previewMode;
    [SerializeField] private float _radius;
    
    private Mesh[] _mesh = null;
    private RailModel _railModel = null;
    private GameObject[] _railsGameObjects;
    private GameObject[] _supportsGameObjects;
    private GameObject[] _heatmapGO;
    private RailPhysics.Props[] _physicsAlongRail;
    private float _lastLength;
    private float _lastCoasterLenght;
    

    private float _inclinationToMatrixLookAt;

    public Rail(Constructor constructor, RailProps rp, ModelProps mp, SpaceProps sp, int isFinalRail, float lastCoasterLenght)
    {
        _constructor = constructor;
        _lastLength = -1f;
        _heatmapGO = new GameObject[2];
        _previewMode = 0;
        _lastCoasterLenght = lastCoasterLenght;
        this.UpdateRail(rp, mp, sp, isFinalRail);
    }

    public void Destroy()
    {
        for(int i = 0; i < _railsGameObjects.Length; i++)
            GameObject.Destroy(_railsGameObjects[i]);
        RemoveSupports();
        GameObject.Destroy(_heatmapGO[0]);
        GameObject.Destroy(_heatmapGO[1]);
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

        if (_railsGameObjects == null || mp != null || _lastLength != _rp.Length)
        {
            _mesh = null;
        }

        Material[] materials = null;

        (_mesh, materials) = _railModel.GenerateMeshes(_rp, _mp, _sp, _mesh);

        if(_railsGameObjects == null || mp != null || _lastLength != _rp.Length)
        {
            if(_railsGameObjects != null)
            {
                // Applying _mesh to an object in the scene
                for (int i = 0; i < _railsGameObjects.Length; i++)
                    GameObject.Destroy(_railsGameObjects[i]);
            }
            _lastLength = _rp.Length;
            _railsGameObjects = new GameObject[_mesh.Length];
            for(int i = 0; i < _railsGameObjects.Length; i++)
                _railsGameObjects[i] = _constructor.InstantiateRail(_mesh[i], materials[i], _sp.Position);
        }

        _inclinationToMatrixLookAt = GetInclinationToMatrixLookAt(_sp.Basis, ThreeRotationMatrix(_sp.Basis, _rp.Radians) * _sp.Basis);

        Vector3 x0 = GetBasisAt(0f).GetColumn(0);
        Vector3 x1 = GetBasisAt(1f).GetColumn(0);

        _radius = _rp.Length / Angle(x0, x1);
        
        (Mesh[] heatmapMesh, Material[] heatmapMaterials) = GetRailModel(0).GenerateMeshes(_rp, _mp, _sp, null);
        if(_heatmapGO != null)
        {
            GameObject.Destroy(_heatmapGO[0]);
            GameObject.Destroy(_heatmapGO[1]);
        }
        _heatmapGO[0] = _constructor.InstantiateRail(heatmapMesh[0], heatmapMaterials[0], _sp.Position, isHeatmap: true);
        _heatmapGO[1] = _constructor.InstantiateRail(heatmapMesh[1], heatmapMaterials[1], _sp.Position, isHeatmap: true);
    }

    public void GenerateSupports()
    {
        float currentLength = 5f - (_lastCoasterLenght % 5);

        RemoveSupports();

        _supportsGameObjects = new GameObject[(int)(_rp.Length + (_lastCoasterLenght % 5))];
        int index = 0;
        float t = _sp.Curve.GetNextT(0, currentLength);
        while(currentLength <= _rp.Length)
        {
            (Mesh[] supportMesh, Material[] supportMaterials) = GetSupports(0).GenerateMeshes(this, t);
            _supportsGameObjects[index] = _constructor.InstantiateRail(supportMesh[0], supportMaterials[0], Vector3.zero, isSupport: true);
            index++;
            t = _sp.Curve.GetNextT(t, 5f);
            currentLength += 5f;
        }
    }

    public void RemoveSupports()
    {
        if (_supportsGameObjects != null)
            for (int i = 0; i < _supportsGameObjects.Length; i++)
                if (_supportsGameObjects[i] != null)
                    GameObject.Destroy(_supportsGameObjects[i]);
    }

    // previewMode: 0 - normal; 1 to preview green; 1 to preview red;
    public void SetPreview(int previewMode)
    {
        _previewMode = previewMode;
        if(previewMode == 0)
        {
            Material[] materials = _railModel.GetMaterials(_mp.Type);
            for (int i = 0; i < _railsGameObjects.Length; i++)
                _railsGameObjects[i].GetComponent<Renderer>().material = materials[i];
        }
        else
        {
            Material previewMaterial = null;
            if(previewMode == 1)
                previewMaterial = Resources.Load("Materials/RollerCoaster/PreviewGreen", typeof(Material)) as Material;
            else
                previewMaterial = Resources.Load("Materials/RollerCoaster/PreviewRed", typeof(Material)) as Material;
            for (int i = 0; i < _railsGameObjects.Length; i++)
                _railsGameObjects[i].GetComponent<Renderer>().material = previewMaterial;
        }
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
        if(position > _rp.Length)
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

    public void SetPhysicsAlongRail(RailPhysics.Props[] physicsAlongRail)
    {
        _physicsAlongRail = physicsAlongRail;
    }

    private int _currentHeatmap = 0;
    private IEnumerator _currentHeatmapCoroutine;

    public void SetHeatmap(int type)
    {
        _currentHeatmap = type;
        if(_currentHeatmapCoroutine != null)
            _constructor.StopChildCoroutine(_currentHeatmapCoroutine);
        _currentHeatmapCoroutine = SetHeatmapCoroutine();
        _constructor.StartChildCoroutine(_currentHeatmapCoroutine);
    }

    private IEnumerator SetHeatmapCoroutine()
    {
        Mesh mesh;
        Vector3[] vertices;
        Color[] colors;
        int resolution = BasicRMProperties.modelResolution;

        if (_physicsAlongRail == null)
        {
            mesh = _heatmapGO[0].GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices;
            colors = new Color[vertices.Length];

            Color tmpColor = new Color(1f, 0f, 0f, 0.35f);
            for (int i = 0; i <= (int)rp.Length; i++)
            {
                Color color = tmpColor;
                for (int j = 0; j < resolution; j++)
                {
                    colors[i * resolution + j] = color;
                }
            }
            mesh.colors = colors;

            mesh = _heatmapGO[1].GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices;
            colors = new Color[vertices.Length];
            for (int i = 0; i < 2; i++)
            {
                Color color = tmpColor;
                for (int j = 0; j < vertices.Length / 2; j++)
                {
                    colors[i * resolution + j] = color;
                }
            }
            mesh.colors = colors;            
        }

        while (_physicsAlongRail == null || _physicsAlongRail.Length <= (int)rp.Length)
        {
            yield return null;
        }

        mesh = _heatmapGO[0].GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        colors = new Color[vertices.Length];

        for (int i = 0; i <= (int)rp.Length; i++)
        {
            Color color = GetHeatmapColor(_currentHeatmap, i);
            for (int j = 0; j < resolution; j++)
            {
                colors[i * resolution + j] = color;
            }
        }
        mesh.colors = colors;

        mesh = _heatmapGO[1].GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        colors = new Color[vertices.Length];
        for (int i = 0; i < 2; i++)
        {
            Color color = GetHeatmapColor(_currentHeatmap, i * (int)rp.Length);
            for (int j = 0; j < vertices.Length / 2; j++)
            {
                colors[i * resolution + j] = color;
            }
        }
        mesh.colors = colors;
    }

    private Color GetHeatmapColor(int type, int t)
    {
        switch (type)
        {
            case 0:
                return Color.white;
            case 1:
                return Color.Lerp(new Color(0.05f, 0.05f, 0.05f), new Color(0.95f, 0.95f, 0.95f), _physicsAlongRail[t].Velocity / 20f);
            case 2:
                return LerpGForce(_physicsAlongRail[t].GForce.y);
            case 3:
                return LerpGForce(_physicsAlongRail[t].GForce.x);
            case 4:
                return LerpGForce(_physicsAlongRail[t].GForce.z);
            case 5:
                (Vector3 position, CarStatus _) = GetPositionInRail(t);
                return Color.Lerp(new Color(0.05f, 0.05f, 0.05f), new Color(0.95f, 0.95f, 0.95f), position.y / 50f);
            default:
                return Color.white;
        }
    }

    private Color LerpGForce(float value)
    {
        return Color.Lerp(new Color(0.05f, 0.05f, 0.05f), new Color(0.95f, 0.95f, 0.95f), value / 20f + 0.5f);
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

    public int PreviewMode
    {
        get { return _previewMode; }
    }

    public float Radius
    {
        get { return _radius; }
    }
}
