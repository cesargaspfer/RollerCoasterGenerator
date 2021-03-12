using UnityEngine;

public class PannelDrag : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Camera _camera;
    #pragma warning disable 0649
    [SerializeField] private Vector2 _mainPannelSize = new Vector2(320f, 580f);
    #pragma warning disable 0649
    [SerializeField] private Vector2 _initialPosition = new Vector2(-620f, 380f);
    private Vector2 _positionOffset = Vector3.zero;
    private float _yScale;

    void Start()
    {
        float pannelX = PlayerPrefs.GetFloat(this.gameObject.name + "X", _initialPosition.x);
        float pannelY = PlayerPrefs.GetFloat(this.gameObject.name + "Y", _initialPosition.y);
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(pannelX, pannelY);
    }


    public void OnPointerDown()
    {
        _yScale = Camera.main.aspect;
        Vector3 mousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
        _positionOffset = this.GetComponent<RectTransform>().anchoredPosition - new Vector2((mousePosition[0] - 0.5f) * 1600, (mousePosition[1] - 0.5f) * 1600 / _yScale);
    }

    public void OnDrag()
    {
        Vector3 mousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
        Vector2 tmpPosition = (new Vector2((mousePosition[0] - 0.5f) * 1600, (mousePosition[1] - 0.5f) * 1600 / _yScale)) + _positionOffset;
        float x = Mathf.Min(Mathf.Max(tmpPosition.x, -800 + _mainPannelSize[0] * 0.5f), 800 - _mainPannelSize[0] * 0.5f);
        float y = Mathf.Min(Mathf.Max(tmpPosition.y, -800 / _yScale + _mainPannelSize[1]), 800 / _yScale);
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }

    public void OnPointerUp()
    {
        PlayerPrefs.SetFloat(this.gameObject.name + "X", this.GetComponent<RectTransform>().anchoredPosition.x);
        PlayerPrefs.SetFloat(this.gameObject.name + "Y", this.GetComponent<RectTransform>().anchoredPosition.y);
    }
}
