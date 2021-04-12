using UnityEngine;

public class TerrainBrush : MonoBehaviour
{
    private static TerrainBrush _inst;
    public static TerrainBrush inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<TerrainBrush>();
            return _inst;
        }
    }

    [SerializeField] private int _state;
    [SerializeField] private float _radius;
    [SerializeField] private float _intencity;
    [SerializeField] private float _opacity;

    void Start()
    {
        _state = 1;
        _radius = 5f;
        _intencity = 20f;
        _opacity = 1f;
    }
    public void Active()
    {

    }

    public void Deactivate()
    {

    }

    public void UpdateValues()
    {

    }

    void Update()
    {
        if (_state > 0)
        {
            if (!UIManager.inst.IsPointerOverUIElement())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 500))
                {
                    Debug.DrawLine(ray.origin, hit.point);
                    Vector3 hitPosition = hit.point;
                    this.transform.position = hitPosition;
                    if (hit.transform.name.Equals("Ground") && Input.GetMouseButton(0))
                        Terrain.inst.UpdateAplifiers(hitPosition, _radius, _intencity * Time.deltaTime, _opacity);
                }
            }
        }
    }
}
