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

    [SerializeField] private int _state = 0;
    [SerializeField] private float _radius;
    [SerializeField] private float _intencity;
    [SerializeField] private float _opacity;

    void Start()
    {
        _state = 0;
    }

    public void Active()
    {
        _state = 1;
    }

    public void Deactivate()
    {
        _state = 0;
    }

    public void UpdateValues(float radius, float intencity, float opacity)
    {
        _radius = radius;
        _intencity = intencity;
        _opacity = opacity;
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
                    this.transform.GetChild(0).gameObject.SetActive(true);
                    Debug.DrawLine(ray.origin, hit.point);
                    Vector3 hitPosition = hit.point;
                    this.transform.position = hitPosition;
                    if (hit.transform.name.Equals("Ground"))
                    {
                        this.transform.GetChild(0).gameObject.SetActive(true);
                        if(Input.GetMouseButton(0))
                            Terrain.inst.UpdateAplifiers(hitPosition, _radius, _intencity * Time.deltaTime, _opacity);
                    }
                    else
                        this.transform.GetChild(0).gameObject.SetActive(false);
                }
                else
                    this.transform.GetChild(0).gameObject.SetActive(false);
            }
            else
                this.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
