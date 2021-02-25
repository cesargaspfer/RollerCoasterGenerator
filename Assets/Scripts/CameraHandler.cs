using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public enum CameraMode
    {
        Normal,
        FirstPerson,
    };


    // General Properties
    #pragma warning disable 0649
    [SerializeField] private RollerCoaster _rollerCoaster;
    [SerializeField] private bool _screenBorderAllowed = true;
    [SerializeField] private float _screenBorder = 0.025f;
    [SerializeField] private float _movementVelocity = 5f;
    [SerializeField] private Vector2 _zoomLimit = new Vector2(2f, 50f);
    [SerializeField] private float _zoomMaxAceleration = 0.3f;
    [SerializeField] private float _zoomMaxVelocity = 0.04f;
    [SerializeField] private float _maxZoomTimeOffset = 0.15f;    

    // Camera Properties
    [SerializeField] private CameraMode _currentCameraMode = CameraMode.Normal;
    [SerializeField] private Vector3 _currentTargetPosition = Vector3.zero;
    [SerializeField] private Vector3 _lastTransformPosition;
    [SerializeField] private Quaternion _lastTransformRotation;
    // TODO: Change
    [SerializeField] private Quaternion _normalQuaternion;
    [SerializeField] private float _currentZoom = 10f;
    [SerializeField] private bool _canZoom = true;
    
    private Camera _camera;
    private Vector3 _previousMousePosition = Vector3.zero;
    private float _zoomCurrentAceleration = 0f;
    private float _zoomCurrentVelocity = 0f;
    private Vector3 _screenResAdj;

    void Awake()
    {
        _camera = GetComponent<Camera>();

        _camera.transform.position = Vector3.zero;

        _camera.transform.Rotate(Vector3.right, 30f);
        _camera.transform.Rotate(Vector3.up, 15f, Space.World);
        _camera.transform.Translate(Vector3.forward * -_currentZoom);

        if(_screenBorderAllowed)
            Cursor.lockState = CursorLockMode.Confined;

        
        if (Screen.height < Screen.width)
        {
            _screenResAdj = new Vector3(((float) Screen.width) / ((float)Screen.height), 1f, 1f);
        }
        else
        {
            _screenResAdj = new Vector3(1f, ((float)Screen.height) / ((float)Screen.width), 1f);
        }

        _canZoom = true;
    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Confined)
            {
                Cursor.lockState = CursorLockMode.None;
                _screenBorderAllowed = false;
                _canZoom = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
                _screenBorderAllowed = true;
                _canZoom = true;
            }
        }
        if(_currentCameraMode == CameraMode.Normal)
        {
            if (!Input.GetMouseButton(2))
            {
                UpdateTargetPosition();
            }
            if (!Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                UpdateCameraZoom();
                UpdateCameraDirection();
            }

            // Update the camera position
            _camera.transform.position = _currentTargetPosition;
            _camera.transform.Translate(Vector3.forward * -_currentZoom);
        }
    }

    public void ChangeCameraMode()
    {
        if (_currentCameraMode == CameraMode.Normal)
        {
            Transform car = _rollerCoaster.GetFirstCar();
            if(car == null)
                return;
            _currentCameraMode = CameraMode.FirstPerson;
            _lastTransformPosition = gameObject.transform.position;
            _lastTransformRotation = gameObject.transform.rotation;
            gameObject.transform.SetParent(car);
            gameObject.transform.localPosition = new Vector3(0.1f, 1.3f, 0f);
            gameObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
        }
        else
        {
            _currentCameraMode = CameraMode.Normal;
            gameObject.transform.SetParent(null);
            gameObject.transform.position = _lastTransformPosition;
            gameObject.transform.rotation = _lastTransformRotation;
        }
    }

    private void UpdateTargetPosition()
    {
        if(Input.GetMouseButton(1))
        {
            if (Input.GetMouseButtonDown(1))
            {
                _previousMousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
            }
            if (Input.GetMouseButton(1))
            {
                Vector3 direction = _previousMousePosition - _camera.ScreenToViewportPoint(Input.mousePosition);
                direction = new Vector3(direction.x * _screenResAdj.x, 0f, direction.y * _screenResAdj.y);

                Vector3 foward = new Vector3(_camera.transform.forward.x, 0f, _camera.transform.forward.z);
                foward.Normalize();
                Vector3 right = new Vector3(_camera.transform.right.x, 0f, _camera.transform.right.z);
                right.Normalize();

                Vector3 moveDirection = foward * direction.z + right * direction.x;
                _currentTargetPosition += moveDirection * _movementVelocity * Mathf.Sqrt(_currentZoom) * 0.5f;

                _previousMousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
            }
        }
        else
        {
            Vector2 mousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
            if((_screenBorderAllowed && (mousePosition.y <= _screenBorder || mousePosition.y >= 1f - _screenBorder)) ||
            (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)))
            {
                Vector3 foward = new Vector3(_camera.transform.forward.x, 0f, _camera.transform.forward.z);
                foward.Normalize();
                if((_screenBorderAllowed && mousePosition.y <= _screenBorder) || 
                (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)))
                    foward = -foward;
                _currentTargetPosition += foward * _movementVelocity * Time.deltaTime * Mathf.Sqrt(_currentZoom);
            }
            if ((_screenBorderAllowed && (mousePosition.x <= _screenBorder || mousePosition.x >= 1f - _screenBorder)) ||
            (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)))
            {
                Vector3 right = new Vector3(_camera.transform.right.x, 0f, _camera.transform.right.z);
                right.Normalize();
                if ((_screenBorderAllowed && mousePosition.x <= _screenBorder) ||
                (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)))
                    right = -right;
                _currentTargetPosition += right * _movementVelocity * Time.deltaTime * Mathf.Sqrt(_currentZoom);
            }
        }
    }

    bool _zoomState = false;
    float _zoomTimeOffset = 0f;
    private void UpdateCameraZoom()
    {
        if(Input.mouseScrollDelta.y != 0f && _canZoom)
        {
            _zoomCurrentAceleration = - Mathf.Sign(Input.mouseScrollDelta.y) * _zoomMaxAceleration * Mathf.Pow(_currentZoom, 0.3f);
            _zoomCurrentVelocity += _zoomCurrentAceleration * Time.deltaTime * 2;
            if(Mathf.Abs(_zoomCurrentVelocity) > _zoomMaxVelocity * Mathf.Pow(_currentZoom, 0.9f))
                _zoomCurrentVelocity = Mathf.Sign(_zoomCurrentAceleration) * _zoomMaxVelocity;
            _currentZoom += _zoomCurrentVelocity;
            if (_currentZoom < _zoomLimit[0])
                _currentZoom = _zoomLimit[0];
            if (_currentZoom > _zoomLimit[1])
                _currentZoom = _zoomLimit[1];
            _zoomState = true;
            _zoomTimeOffset = 0f;
        }
        else
        {
            if(_zoomCurrentVelocity != 0)
            {
                if(_zoomState)
                {
                    if(_zoomTimeOffset < _maxZoomTimeOffset)
                    {
                        _zoomTimeOffset += Time.deltaTime;
                    }
                    else
                    {
                        _zoomCurrentAceleration = - _zoomCurrentAceleration;
                        _zoomState = false;
                    }
                }
                _zoomCurrentVelocity += _zoomCurrentAceleration * Time.deltaTime;
                if (Mathf.Abs(_zoomCurrentVelocity) > _zoomMaxVelocity * Mathf.Pow(_currentZoom, 1.5f))
                    _zoomCurrentVelocity = Mathf.Sign(_zoomCurrentAceleration) * _zoomMaxVelocity;
                if(!_zoomState && (Mathf.Abs(_zoomCurrentVelocity) < 0.05f || Mathf.Sign(_zoomCurrentAceleration) == Mathf.Sign(_zoomCurrentVelocity)))
                {
                    _zoomCurrentAceleration = 0f;
                    _zoomCurrentVelocity = 0f;
                }
                _currentZoom += _zoomCurrentVelocity;
                if (_currentZoom < _zoomLimit[0])
                {
                    _currentZoom = _zoomLimit[0];
                    if(_zoomCurrentVelocity < 0)
                    {
                        _zoomCurrentAceleration = 0f;
                        _zoomCurrentVelocity = 0f;
                    }
                }
                if (_currentZoom > _zoomLimit[1])
                {
                    _currentZoom = _zoomLimit[1];
                    if(_zoomCurrentVelocity > 0)
                    {
                        _zoomCurrentAceleration = 0f;
                        _zoomCurrentVelocity = 0f;
                    }
                }
                
            }
        }
    }

    private void UpdateCameraDirection()
    {
        if (Input.GetMouseButtonDown(2))
        {
            _previousMousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(2))
        {
            Vector3 direction = _previousMousePosition - _camera.ScreenToViewportPoint(Input.mousePosition);

            _camera.transform.position = _currentTargetPosition;
            
            if(_camera.transform.eulerAngles.x >= 80f && direction.y < 0)
            {
                _camera.transform.eulerAngles = new Vector3(80f, _camera.transform.eulerAngles.y, 0f);
            }
            else if (_camera.transform.eulerAngles.x <= 5f && direction.y > 0)
            {
                _camera.transform.eulerAngles = new Vector3(5f, _camera.transform.eulerAngles.y, 0f);
            }
            else {
                if(Mathf.Abs(direction.y) > 0.05f)
                {
                    direction = new Vector3(0f, Mathf.Sign(direction.y) * 0.05f, 0f);
                }
                _camera.transform.Rotate(Vector3.right, direction.y * 180f);
                if (_camera.transform.eulerAngles.x > 80f && _camera.transform.eulerAngles.x <= 270f)
                {
                    _camera.transform.eulerAngles = new Vector3(80f, _camera.transform.eulerAngles.y, 0f);
                }
                if (_camera.transform.eulerAngles.x < 5f || _camera.transform.eulerAngles.x > 270f)
                {
                    _camera.transform.eulerAngles = new Vector3(5f, _camera.transform.eulerAngles.y, 0f);
                }
            }
            _camera.transform.Rotate(Vector3.up, -direction.x * 180f, Space.World);
            
            _previousMousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
        }
    }

    public void SetCanZoom(bool value)
    {
        _canZoom = value;
    }

    public CameraMode GetCameraMode()
    {
        return _currentCameraMode;
    }
}
