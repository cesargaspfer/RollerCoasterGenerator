using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorativeObjectPlacer : MonoBehaviour
{
    private static DecorativeObjectPlacer _inst;
    public static DecorativeObjectPlacer inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<DecorativeObjectPlacer>();
            return _inst;
        }
    }

    #pragma warning disable 0649
    [SerializeField] private Transform _decorativeObjectPrefab;
    #pragma warning disable 0649
    [SerializeField] private Transform _decorativeObjectPlacerPrefab;
    #pragma warning disable 0649
    [SerializeField] private Transform _decorativeObjectsContent;
    [SerializeField] private int _state = -1;
    [SerializeField] private string _currentObjectName = "";
    [SerializeField] private float _currentHeight = 0f;
    [SerializeField] private float _currentRotation = 0f;
    [SerializeField] private Transform _placerTransform;
    [SerializeField] private bool _clicked = false;
    
    public void Open()
    {
        _state = 0;
        _currentHeight = 0f;
        _currentRotation = 0f;
    }

    public void Close()
    {
        _state = -1;
        StopPlacement();
    }

    public void StartPlacement(string objectName)
    {
        _currentObjectName = objectName;
        _state = 1;

        if(_placerTransform == null)
        {
            _placerTransform = Instantiate(_decorativeObjectPlacerPrefab, Vector3.zero, Quaternion.identity);
            _placerTransform.transform.localPosition = new Vector3(0, 0, 0);
            _placerTransform.transform.localScale = Vector3.one * 0.5f;
        }

        foreach (Transform tr in _placerTransform)
        {
            GameObject.Destroy(tr.gameObject);
        }

        if (!objectName.Equals("null"))
        {
            GameObject instance = Instantiate(Resources.Load("Objects/" + objectName, typeof(GameObject))) as GameObject;
            instance.transform.SetParent(_placerTransform);
            instance.transform.localPosition = new Vector3(-5, 0, -5);
            instance.transform.localEulerAngles = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.name = objectName;
        }

        _clicked = false;
    }

    public void StopPlacement()
    {
        _currentObjectName = "";
        _state = 0;
        if(_placerTransform != null)
            GameObject.Destroy(_placerTransform.gameObject);
        _placerTransform = null;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            _clicked = true;

        if (_state > 0)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _currentHeight -= 3.275f;
                _placerTransform.transform.position = new Vector3
                (
                    _placerTransform.transform.position.x,
                    _currentHeight,
                    _placerTransform.transform.position.z
                );
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                _currentHeight += 3.275f;
                _placerTransform.transform.position = new Vector3
                (
                    _placerTransform.transform.position.x,
                    _currentHeight,
                    _placerTransform.transform.position.z
                );
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                _currentRotation = (_currentRotation + 90f) % 360f;
                _placerTransform.transform.localEulerAngles = new Vector3(0f, _currentRotation, 0f);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIObjectManager.inst.DeselectButtonClicked();
            }
        }
    }

    void FixedUpdate()
    {
        if (_state > 0)
        {
            if(!UIManager.inst.IsPointerOverUIElement())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 500))
                {
                    Debug.DrawLine(ray.origin, hit.point);
                    Vector3 hitPosition = hit.point;
                    if(!hit.transform.name.Equals("Ground"))
                        hitPosition -= ray.direction * 0.05f;
                    _placerTransform.transform.position = new Vector3
                    (
                        Mathf.RoundToInt(hitPosition.x / 5f) * 5f,
                        _currentHeight,
                        Mathf.RoundToInt(hitPosition.z / 5f) * 5f
                    );
                }

                if (_clicked)
                {
                    if(_state == 2)
                        _currentRotation = _placerTransform.transform.localEulerAngles.y;

                    Place(_currentObjectName, _placerTransform.transform.position, _currentRotation);

                    if(_state == 2)
                        UIObjectManager.inst.DeselectButtonClicked();
                }
            }
        }
        else if (_state == 0)
        {
            if (_clicked && !UIManager.inst.IsPointerOverUIElement())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 500) && hit.transform.name != "Ground")
                {
                    UIObjectManager.inst.UIObjectButtonClicked(hit.transform.GetChild(0).name);

                    _currentHeight = hit.transform.localPosition.y;
                    _currentRotation = hit.transform.localEulerAngles.y;
                    _placerTransform.localPosition = new Vector3(_placerTransform.localPosition.x, _currentHeight, _placerTransform.localPosition.z);
                    _placerTransform.localEulerAngles = new Vector3(0, _currentRotation, 0);
                    
                    GameObject.Destroy(hit.transform.gameObject);
                    _state = 2;
                }
            }
        }

        if(_clicked)
            _clicked = false;
    }

    public void DestroyAllDecorativeObjects()
    {
        foreach(Transform tr in _decorativeObjectsContent)
        {
            GameObject.Destroy(tr.gameObject);
        }
    }

    public void Place((string, Vector3, float)[] decorativeObjects)
    {
        if(decorativeObjects == null) 
            return;

        foreach((string objectName, Vector3 position, float rotation) in decorativeObjects)
        {
            Place(objectName, position, rotation);
        }
    }

    public void Place(string objectName, Vector3 position, float rotation)
    {
        if(objectName.Equals("") || objectName.Equals("null"))
            return;

        Transform decorativeObject = Instantiate(_decorativeObjectPrefab, Vector3.zero, Quaternion.identity);
        decorativeObject.transform.SetParent(_decorativeObjectsContent);
        decorativeObject.transform.localPosition = position;
        decorativeObject.transform.localEulerAngles = new Vector3(0f, rotation, 0f);
        decorativeObject.transform.localScale = Vector3.one * 0.5f;

        GameObject instance = Instantiate(Resources.Load("Objects/" + objectName, typeof(GameObject))) as GameObject;
        instance.transform.SetParent(decorativeObject);
        instance.transform.localPosition = new Vector3(-5, 0, -5);
        instance.transform.localEulerAngles = Vector3.zero;
        instance.transform.localScale = Vector3.one;
        instance.transform.name = objectName;
    }

    public (string, Vector3, float)[] GetAllDecorativeObjects()
    {
        (string, Vector3, float)[] decorativeObjects = new (string, Vector3, float)[_decorativeObjectsContent.childCount];

        for(int i = 0; i < decorativeObjects.Length; i++)
        {
            Transform currentObject = _decorativeObjectsContent.GetChild(i);
            decorativeObjects[i] = (currentObject.GetChild(0).name, currentObject.transform.localPosition, currentObject.transform.localEulerAngles.y);
        }

        return decorativeObjects;
    }
}
