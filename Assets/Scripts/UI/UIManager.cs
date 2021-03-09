using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager _inst;
    public static UIManager inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<UIManager>();
            return _inst;
        }
    }

    #pragma warning disable 0649
    [SerializeField] private bool _debugMode;

    #pragma warning disable 0649
    [SerializeField] private Camera _camera;
    #pragma warning disable 0649
    [SerializeField] private RollerCoaster _rollerCoaster;
    #pragma warning disable 0649
    [SerializeField] private CameraHandler _cameraHandler;
    #pragma warning disable 0649
    [SerializeField] private Transform _pannel;
    #pragma warning disable 0649
    [SerializeField] private Transform _menuPannel;
    #pragma warning disable 0649
    [SerializeField] private InputField _nameInput;
    #pragma warning disable 0649
    [SerializeField] private Dropdown _railTypeDropdown;
    #pragma warning disable 0649
    [SerializeField] private Dropdown _heatmapDropdown;
    #pragma warning disable 0649
    [SerializeField] private UIRailProps _UIRailProps;
    #pragma warning disable 0649
    [SerializeField] private UIRailPhysics _UIRailPhysics;
    #pragma warning disable 0649
    [SerializeField] private Transform _screenshotCamera;
    #pragma warning disable 0649
    [SerializeField] private Transform _flash;
    #pragma warning disable 0649
    [SerializeField] private LoadPannel _loadPannel;
    #pragma warning disable 0649
    [SerializeField] private GameObject _settingsButton;
    
    [SerializeField] private bool _isPaused = false;
    [SerializeField] private bool _exitedMenu = false;

    private Animator _mpAnim;
    private Animator _menuAnim;

    void Awake()
    {
        _mpAnim = _pannel.GetComponent<Animator>();
        _menuAnim = _menuPannel.GetComponent<Animator>();
    }

    void Start()
    {
        if(!_debugMode)
        {
            ShowMenu();
            _isPaused = true;
            _exitedMenu = false;
            _cameraHandler.SetCanMove(false);
            _mpAnim.Play("HideState");
        }
        else
        {
            _settingsButton.SetActive(true);
            _rollerCoaster.Initialize();
            _rollerCoaster.AddRail(true);
            ConstructionArrows.inst.Initialize(_rollerCoaster);
            _isPaused = false;
            _exitedMenu = true;
            _cameraHandler.SetCanMove(true);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && _exitedMenu)
        {
            if(!_isPaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
    }

    // ---------------------------- Move Pannel ---------------------------- //

    #pragma warning disable 0649
    [SerializeField] private Vector2 _mainPannelSize = new Vector2(320f, 580f);
    private Vector2 _positionOffset = Vector3.zero;
    private float _yScale;

    public void OnPointerDownOnTop()
    {
        _yScale = Camera.main.aspect;
        Vector3 mousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
        _positionOffset = _pannel.GetComponent<RectTransform>().anchoredPosition - new Vector2((mousePosition[0] - 0.5f) * 1600, (mousePosition[1] - 0.5f) * 1600 / _yScale);
    }

    public void OnDragOnTop()
    {
        Vector3 mousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
        Vector2 tmpPosition = (new Vector2((mousePosition[0] - 0.5f) * 1600, (mousePosition[1] - 0.5f) * 1600 / _yScale)) + _positionOffset;
        float x = Mathf.Min(Mathf.Max(tmpPosition.x, -800 + _mainPannelSize[0] * 0.5f), 800 - _mainPannelSize[0] * 0.5f);
        float y = Mathf.Min(Mathf.Max(tmpPosition.y, -800 / _yScale + _mainPannelSize[1]), 800 / _yScale);
        _pannel.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }

    // ---------------------------- Main Pannel Animation Buttons ---------------------------- //
    
    [SerializeField] private bool _isAnimating = false;
    // _pannelState: 0 to Constructor and 1 to Terrain
    [SerializeField] private int _pannelState = 0;
    // _constructorPannelState: 0 to Manual Construction and 1 to Generator
    [SerializeField] private int _constructorPannelState = 0;
    [SerializeField] private bool _isRailPropsGlobal = true;
    [SerializeField] private bool _isBlueprintActive = false;
    [SerializeField] private bool _isSimulating = false;
    [SerializeField] private int _lasRailType = 0;
    

    public void ShowPannel(int constructorPannelState, bool loaded)
    {
        if(_isAnimating) return;
        _railTypeDropdown.value = _lasRailType;
        _heatmapDropdown.value = 0;
        _constructorPannelState = constructorPannelState;

        if (_constructorPannelState == 0)
        {
            _mpAnim.Play("MainState");
            ConstructionArrows.inst.Initialize(_rollerCoaster);
            ConstructionArrows.inst.ActiveArrows(!_rollerCoaster.IsComplete());
        }
        else
        {
            _mpAnim.Play("GenerationState");
        }
        _isSimulating = false;
        StartCoroutine(AnimationTime(0.5f));
    }

    public void HidePannel()
    {
        if(_isAnimating) return;
        _lasRailType = _railTypeDropdown.value;
        StartCoroutine(HidePannelCoroutine());
    }

    private IEnumerator HidePannelCoroutine()
    {
        _isAnimating = true;
        if (_pannelState == 1)
        {
            _mpAnim.Play("ChangeFromTerrain");
            yield return new WaitForSeconds(0.25f);
        }
        if (_constructorPannelState == 0)
            _mpAnim.Play("HideMainPannel");
        else
            _mpAnim.Play("HideGenerationPannel");

        ConstructionArrows.inst.ActiveArrows(false);

        yield return new WaitForSeconds(0.5f);
        _isAnimating = false;
    }

    public void TopContructButtonPressed()
    {
        if(_isAnimating || _pannelState == 0) return;
        _pannelState = 0;
        _mpAnim.Play("ChangeFromTerrain");
        if (_constructorPannelState == 0)
            ConstructionArrows.inst.ActiveArrows(!_rollerCoaster.IsComplete());

        StartCoroutine(AnimationTime(0.25f));

    }

    public void TopTerrainButtonPressed()
    {
        if(_isAnimating || _pannelState == 1) return;
        _pannelState = 1;
        _mpAnim.Play("ChangeToTerrain");
        if (_constructorPannelState == 0)
            ConstructionArrows.inst.ActiveArrows(false);
        StartCoroutine(AnimationTime(0.25f));

    }

    public void GlobalButtonPressed()
    {
        if(_isAnimating || _isRailPropsGlobal) return;
        _isRailPropsGlobal = true;
        _mpAnim.Play("ChangeFromLocalProperties");
        StartCoroutine(AnimationTime(0.25f));

    }

    public void LocalButtonPressed()
    {
        if(_isAnimating || !_isRailPropsGlobal) return;
        _isRailPropsGlobal = false;
        _mpAnim.Play("ChangeToLocalProperties");
        StartCoroutine(AnimationTime(0.25f));

    }

    public void BlueprintButtonPressed()
    {
        if(_isAnimating || _isBlueprintActive) return;
        _isBlueprintActive = true;
        ConstructionArrows.inst.ActiveArrows(false);
        _mpAnim.Play("ChangeToBlueprint");
        StartCoroutine(AnimationTime(0.75f));

    }

    public void CancelBlueprintButtonPressed()
    {
        if(_isAnimating || !_isBlueprintActive) return;
        _isBlueprintActive = false;
        ConstructionArrows.inst.ActiveArrows(!_rollerCoaster.IsComplete());
        _mpAnim.Play("ChangeFromBlueprint");
        StartCoroutine(AnimationTime(0.75f));

    }

    public void SimulateButtonPressed()
    {
        if(_isAnimating) return;

        _rollerCoaster.StartCarSimulation();
        _isSimulating = true;
        ConstructionArrows.inst.ActiveArrows(false);
        _UIRailPhysics.UpdateValues(_rollerCoaster, true);

        if (_constructorPannelState == 0)
            _mpAnim.Play("ChangeToSimulation");
        else
            _mpAnim.Play("GeneratorToSimulation");
        StartCoroutine(AnimationTime(0.5f));
    }

    public void StopSimulationButtonPressed()
    {
        if(_isAnimating) return;

        if (_cameraHandler.GetCameraMode() == CameraHandler.CameraMode.FirstPerson)
            _cameraHandler.ChangeCameraMode();
        _isSimulating = false;
        ConstructionArrows.inst.ActiveArrows(!_rollerCoaster.IsComplete());
        _UIRailPhysics.UpdateValues(_rollerCoaster, false);
        _rollerCoaster.StopCarSimulation();

        if (_constructorPannelState == 0)
            _mpAnim.Play("ChangeFromSimulation");
        else
            _mpAnim.Play("SimulationToGenerator");
        StartCoroutine(AnimationTime(0.5f));
    }

    // ---------------------------- Pause Pannel Animation Buttons ---------------------------- //

    // _MenuState: 0 to Select, 1 to Options, 2 to Credits, 3 to Save, 4 to Load
    [SerializeField] private int _MenuState = -1;

    public void ShowMenu()
    {
        _settingsButton.SetActive(false);
        _MenuState = -1;
        // TODO: Check if it's not interfering in nothing else
        _cameraHandler.SetCanMove(false);
        StopAllCoroutines();
        _menuPannel.gameObject.SetActive(true);
        _menuPannel.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
        _menuPannel.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
        _menuAnim.Play("ShowPauseMenu");
        StartCoroutine(AnimationTime(0.375f));

    }

    private void ShowPause()
    {
        _settingsButton.SetActive(false);
        _MenuState = -1;
        // TODO: Check if it's not interfering in nothing else
        _cameraHandler.SetCanMove(false);
        StopAllCoroutines();
        _menuPannel.gameObject.SetActive(true);
        _menuPannel.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(true);

        _menuPannel.GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(3).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(5).gameObject.SetActive(false);

        _menuAnim.Play("ShowPauseMenu");
        StartCoroutine(AnimationTime(0.375f));

    }

    private void GoToFirstMenuPannel()
    {
        if(_isAnimating) return;
        if(_MenuState != 4)
            _menuAnim.Play("FromSecondPannel");
        else
        {
            if(_exitedMenu)
                ShowPannel(_lastPannelState, false);
            _menuAnim.Play("FromLoadCoaster");
        }
        _MenuState = -1;
        StartCoroutine(AnimationTime(0.25f));
        
    }

    private void GoToSecondMenuPannel()
    {
        if(_isAnimating) return;
        _menuPannel.GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(3).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(5).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(_MenuState + 1).gameObject.SetActive(true);
        _menuAnim.Play("ToSecondPannel");
        StartCoroutine(AnimationTime(0.25f));
    }

    private void GoToLoadCoaster()
    {
        if (_isAnimating) return;
        _MenuState = 4;
        _menuPannel.GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(3).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(5).gameObject.SetActive(true);
        _menuAnim.Play("ToLoadCoaster");
        if (_exitedMenu)
        {
            _lastPannelState = _pannelState;
            HidePannel();
        }
        StartCoroutine(AnimationTime(0.25f));
    }

    private void HideMenu()
    {
        if(_isAnimating) return;
        _settingsButton.SetActive(true);
        _cameraHandler.SetCanMove(true);
        StartCoroutine(HideMenuCoroutine());
    }

    private IEnumerator HideMenuCoroutine()
    {
        _isAnimating = true;
        if (_MenuState != 4)
            _menuAnim.Play("HidePauseMenu");
        else
            _menuAnim.Play("CloseLoadPannel");
        yield return new WaitForSeconds(0.375f);
        _menuPannel.gameObject.SetActive(false);
        _isAnimating = false;
    }

    private IEnumerator AnimationTime(float time)
    {
        _isAnimating = true;
        yield return new WaitForSeconds(time);
        _isAnimating = false;
    }

    // ---------------------------- Main Pannel Normal Buttons ---------------------------- //

    public void AddRailButtonPressed()
    {
        if(_rollerCoaster.IsComplete()) return;
        _rollerCoaster.AddRail(true);
        ConstructionArrows.inst.ActiveArrows(true);
        ConstructionArrows.inst.UpdateArrows();
        // TODO: Update RailProps
    }

    public void AutoCompleteButtonPressed()
    {
        ConstructionArrows.inst.ActiveArrows(false);
        _rollerCoaster.AddFinalRail();
        // TODO: Update RailProps
    }

    public void RemoveRailButtonClicked()
    {
        (RailProps rp, ModelProps mp) = _rollerCoaster.RemoveLastRail();
        ConstructionArrows.inst.ActiveArrows(true);
        ConstructionArrows.inst.UpdateArrows();
        if (rp == null)
            // TODO: Warn player that he can't remove rail
            return;
        // TODO: Update RailProps
    }

    public void GenerateCoasterButtonPressed()
    {
        _rollerCoaster.GenerateCoaster();
    }

    public void ChangeCameraButtonPressed()
    {
        _cameraHandler.ChangeCameraMode();
    }

    public void UpdateRailType(int type)
    {
        // TODO: Don't call this when pannel is shown
        _rollerCoaster.UpdateLastRail(railType: type);
    }

    // ---------------------------- Pause Pannel Normal Buttons ---------------------------- //

    private bool _showArrowsWhenUnpause = false;
    private int _lastPannelState = -1;

    public void Pause()
    {
        ShowPause();
        _isPaused = true;
        if(ConstructionArrows.inst.IsActive)
        {
            _showArrowsWhenUnpause = true;
            ConstructionArrows.inst.ActiveArrows(false);
        }

    }

    public void Unpause()
    {
        if (_isAnimating) return;
        _isPaused = false;
        if (_showArrowsWhenUnpause)
        {
            _showArrowsWhenUnpause = false;
            if(_pannelState == 0)
                ConstructionArrows.inst.ActiveArrows(!_rollerCoaster.IsComplete());
        }
        HideMenu();
    }

    public void MenuOptionsButtonPressed()
    {
        if (_isAnimating) return;
        _MenuState = 1;
        GoToSecondMenuPannel();
    }

    public void MenuCreditsButtonPressed()
    {
        if (_isAnimating) return;
        _MenuState = 2;
        GoToSecondMenuPannel();
    }

    public void MenuConstructCoasterButtonPressed()
    {
        if (_isAnimating) return;
        _MenuState = 0;
        _pannelState = 0;
        GoToSecondMenuPannel();
    }

    public void MenuGenerateCoasterButtonPressed()
    {
        if (_isAnimating) return;
        _MenuState = 0;
        _pannelState = 1;
        GoToSecondMenuPannel();
    }

    public void MenuLoadCoasterButtonPressed()
    {
        if (_isAnimating) return;
        if(_exitedMenu)
        {
            StopSimulationButtonPressed();
            _isAnimating = false;
            ConstructionArrows.inst.ActiveArrows(false);
        }
        _loadPannel.Initialize(_rollerCoaster);
        GoToLoadCoaster();
    }

    public void LoadCoaster(string coasterName)
    {
        if(!_exitedMenu)
            _rollerCoaster.Initialize();
        _rollerCoaster.LoadCoaster(coasterName);
        _lasRailType = (int) _rollerCoaster.GetLastRail().mp.Type;
        ConstructionArrows.inst.Initialize(_rollerCoaster);
        _isPaused = false;
        _exitedMenu = true;
        _cameraHandler.SetCanMove(true);
        ShowPannel(0, true);
        _isAnimating = false;
        Unpause();
    }

    public void MenuSaveCoasterButtonPressed()
    {
        if (_isAnimating) return;
        _MenuState = 3;
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(0).gameObject.SetActive(true);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(1).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(2).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetChild(3).GetComponent<InputField>().text = _nameInput.text;
        _flash.gameObject.SetActive(true);
        GoToSecondMenuPannel();
    }

    public void MenuContinueSaveButtonPressed()
    {
        if (_isAnimating) return;
        // TODO: Change Name
        _screenshotCamera.gameObject.SetActive(true);
        _screenshotCamera.GetChild(0).GetComponent<Text>().text = _nameInput.text;
        _menuPannel.gameObject.SetActive(false);
        _pannel.gameObject.SetActive(false);
        _cameraHandler.SetCanMove(true);
    }

    public void Save()
    {
        StartCoroutine(SaveAnimation());
        // TODO: Warn player if it was saved
    }

    private IEnumerator SaveAnimation()
    {
        _screenshotCamera.gameObject.SetActive(false);
        yield return null;
        bool saved = _rollerCoaster.SaveCoaster(_nameInput.text);
        yield return null;
        _pannel.gameObject.SetActive(true);
        _menuPannel.gameObject.SetActive(true);
        _cameraHandler.SetCanMove(false);

        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(0).gameObject.SetActive(false);
        if (saved)
            _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(1).gameObject.SetActive(true);
        else
            _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(2).gameObject.SetActive(true);

        _flash.GetComponent<Animator>().Play("Flash");
    }

    public void MenuExitButtonPressed()
    {
        if (_isAnimating) return;
        // TODO dialoge "Are you sure"
        Application.Quit();
    }

    public void MenuReturnToMenuButtonPressed()
    {
        if (_isAnimating) return;
        // TODO dialoge "Are you sure"
        SceneManager.LoadScene(0);
        // TODO Bright Pannel
    }

    public void MenuContinueButtonPressed()
    {
        if (_isAnimating) return;
        // TODO
        // Check _MenuState
        StartCoroutine(MenuContinueCoroutine());
    }

    private IEnumerator MenuContinueCoroutine()
    {
        _rollerCoaster.Initialize();
        if(_pannelState == 0)
            _rollerCoaster.AddRail(true);
        else
            _rollerCoaster.GenerateCoaster();
        // TODO: Show the right Pannel
        ShowPannel(_pannelState, false);
        _isAnimating = true;
        _isPaused = false;
        _exitedMenu = true;
        _cameraHandler.SetCanMove(true);
        _menuAnim.Play("HidePauseMenu");
        yield return new WaitForSeconds(0.375f);
        _menuPannel.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        _menuPannel.GetChild(0).GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(410f, 0f);
        _menuPannel.gameObject.SetActive(false);
        _settingsButton.SetActive(true);
        _isAnimating = false;
    }

    // ---------------------------- UI Text Functions ---------------------------- //

    public void UpdateUIValues ()
    {
        _UIRailProps.UpdateValues(_rollerCoaster);
        _UIRailPhysics.UpdateValues(_rollerCoaster, _isSimulating);
    }
}
