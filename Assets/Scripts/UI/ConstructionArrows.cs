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

    private RollerCoaster _rc;
    private bool _isActive = false;
    private float _lastLength = -1;
    public void Initialize(RollerCoaster rollerCoaster)
    {
        _rc = rollerCoaster;
        _lastLength = -1;
        ActiveArrows(true);
        UpdateArrows();
    }

    public void UpdateArrows()
    {
        Rail rail = _rc.GetLastRail();
        this.transform.position = _rc.GetFinalPosition();
        this.transform.rotation = rail.GetQuaternionAt(1f);

        if(_lastLength != rail.rp.Length)
        {
            _lastLength = rail.rp.Length;
            RailProps rp = new RailProps(Mathf.PI * 0.25f, 0f, 0f, 0.9f * _lastLength);
            ModelProps mp = new ModelProps(2, 0, 7);
            SpaceProps sp = new SpaceProps(Vector3.zero, Matrix4x4.identity);
            (Bezier curve, Matrix4x4 finalBasis, Vector3 finalPosition) = CalculateImaginarySphere(sp, rp);
            sp.Curve = curve;


            RailModel railModel = GetRailModel(2);
            Material[] materials = null;
            Mesh[] mesh = null;

            (mesh, materials) = railModel.GenerateMeshes(rp, mp, sp, mesh);

            for (int i = 0; i < 6; i++)
                this.transform.GetChild(i).GetComponent<MeshFilter>().mesh = mesh[0];

            this.transform.GetChild(4).transform.localPosition = new Vector3(0f, _lastLength, 0f);
            this.transform.GetChild(5).transform.localPosition = new Vector3(0f, _lastLength, 0f);

            rp = new RailProps(0f, 0f, 0f, 3f);
            (curve, finalBasis, finalPosition) = CalculateImaginarySphere(sp, rp);
            sp.Curve = curve;
            materials = null;
            mesh = null;
            (mesh, materials) = railModel.GenerateMeshes(rp, mp, sp, mesh);
            this.transform.GetChild(6).GetComponent<MeshFilter>().mesh = mesh[0];
            this.transform.GetChild(7).GetComponent<MeshFilter>().mesh = mesh[1];
        }

    }

    public void ActiveArrows(bool active)
    {
        if(_isActive == active) return;
        _isActive = active;
        for(int i = 0; i < 8; i++)
            this.transform.GetChild(i).gameObject.SetActive(active);
    }

    public bool IsActive
    {
        get { return _isActive; }
    }

}
