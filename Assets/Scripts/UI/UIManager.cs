using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
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
    [SerializeField] private Dropdown _heatmapDropdown;

    private Animator _mpAnim;
    private Animator _menuAnim;

    void Awake()
    {
        _mpAnim = _pannel.GetComponent<Animator>();
        _menuAnim = _menuPannel.GetComponent<Animator>();
    }

    void Start()
    {
        _rollerCoaster.Initialize();
        ShowPannel(0);
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

    public void ShowPannel(int constructorPannelState)
    {
        if(_isAnimating) return;
        _railTypeDropdown.value = 0;
        _heatmapDropdown.value = 0;
        _constructorPannelState = constructorPannelState;
        if (_constructorPannelState == 0)
        {
            _mpAnim.Play("MainState");
            _rollerCoaster.AddRail(true);
            ConstructionArrows.inst.Initialize(_rollerCoaster);
        }
        else
        {
            _mpAnim.Play("GenerationState");
        }
        StartCoroutine(AnimationTime(1f));
    }

    public void HidePannel()
    {
        if(_isAnimating) return;
        StartCoroutine(HidePannelCoroutine());
    }

    private IEnumerator HidePannelCoroutine()
    {
        _isAnimating = true;
        if (_pannelState == 1)
        {
            _mpAnim.Play("ChangeFromTerrain");
            yield return new WaitForSeconds(0.5f);
        }
        if (_constructorPannelState == 0)
            _mpAnim.Play("HideMainPannel");
        else
            _mpAnim.Play("HideGenerationPannel");

        ConstructionArrows.inst.ActiveArrows(false);

        yield return new WaitForSeconds(1f);
        _isAnimating = false;
    }

    public void TopContructButtonPressed()
    {
        if(_isAnimating || _pannelState == 0) return;
        _pannelState = 0;
        _mpAnim.Play("ChangeFromTerrain");
        if (_constructorPannelState == 0)
            ConstructionArrows.inst.ActiveArrows(true);

        StartCoroutine(AnimationTime(0.5f));

    }

    public void TopTerrainButtonPressed()
    {
        if(_isAnimating || _pannelState == 1) return;
        _pannelState = 1;
        _mpAnim.Play("ChangeToTerrain");
        if (_constructorPannelState == 0)
            ConstructionArrows.inst.ActiveArrows(false);
        StartCoroutine(AnimationTime(0.5f));

    }

    public void GlobalButtonPressed()
    {
        if(_isAnimating || _isRailPropsGlobal) return;
        _isRailPropsGlobal = true;
        _mpAnim.Play("ChangeFromLocalProperties");
        StartCoroutine(AnimationTime(0.5f));

    }

    public void LocalButtonPressed()
    {
        if(_isAnimating || !_isRailPropsGlobal) return;
        _isRailPropsGlobal = false;
        _mpAnim.Play("ChangeToLocalProperties");
        StartCoroutine(AnimationTime(0.5f));

    }

    public void BlueprintButtonPressed()
    {
        if(_isAnimating || _isBlueprintActive) return;
        _isBlueprintActive = true;
        _mpAnim.Play("ChangeToBlueprint");
        StartCoroutine(AnimationTime(1.5f));

    }

    public void CancelBlueprintButtonPressed()
    {
        if(_isAnimating || !_isBlueprintActive) return;
        _isBlueprintActive = false;
        _mpAnim.Play("ChangeFromBlueprint");
        StartCoroutine(AnimationTime(1.5f));

    }

    public void SimulateButtonPressed()
    {
        if(_isAnimating) return;

        _rollerCoaster.StartCarSimulation();

        if (_constructorPannelState == 0)
            _mpAnim.Play("ChangeToSimulation");
        else
            _mpAnim.Play("GeneratorToSimulation");
        StartCoroutine(AnimationTime(1f));
    }

    public void StopSimulationButtonPressed()
    {
        if(_isAnimating) return;

        if (_cameraHandler.GetCameraMode() == CameraHandler.CameraMode.FirstPerson)
            _cameraHandler.ChangeCameraMode();
        _rollerCoaster.StopCarSimulation();

        if (_constructorPannelState == 0)
            _mpAnim.Play("ChangeFromSimulation");
        else
            _mpAnim.Play("SimulationToGenerator");
        StartCoroutine(AnimationTime(1f));
    }

    // ---------------------------- Pause Pannel Animation Buttons ---------------------------- //

    // _MenuState: 0 to Select, 1 to Options, 2 to Credits 
    [SerializeField] private int _MenuState = 0;

    public void ShowMenu()
    {
        // TODO: Check if it's not interfering in nothing else
        StopAllCoroutines();
        _menuPannel.gameObject.SetActive(true);
        _menuPannel.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
        _menuPannel.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
        _menuAnim.Play("ShowPauseMenu");
        StartCoroutine(AnimationTime(0.75f));

    }

    private void ShowPause()
    {
        // TODO: Check if it's not interfering in nothing else
        StopAllCoroutines();
        _menuPannel.gameObject.SetActive(true);
        _menuPannel.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(true);
        _menuAnim.Play("ShowPauseMenu");
        StartCoroutine(AnimationTime(0.75f));

    }

    private void GoToFirstMenuPannel()
    {
        if(_isAnimating) return;
        _menuAnim.Play("FromSecondPannel");
        StartCoroutine(AnimationTime(0.5f));
        
    }

    private void GoToSecondMenuPannel()
    {
        if(_isAnimating) return;
        _menuPannel.GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(3).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(_MenuState + 1).gameObject.SetActive(true);
        _menuAnim.Play("ToSecondPannel");
        StartCoroutine(AnimationTime(0.5f));
    }

    private void HideMenu()
    {
        if(_isAnimating) return;
        StartCoroutine(HideMenuCoroutine());
    }

    private IEnumerator HideMenuCoroutine()
    {
        _isAnimating = true;
        _menuAnim.Play("HidePauseMenu");
        yield return new WaitForSeconds(0.75f);
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
        _rollerCoaster.AddRail();
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

    public void Pause()
    {
        ShowPause();
    }

    public void Unpause()
    {
        if (_isAnimating) return;
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
        GoToSecondMenuPannel();
    }

    public void MenuGenerateCoasterButtonPressed()
    {
        if (_isAnimating) return;
        _MenuState = 0;
        GoToSecondMenuPannel();
    }

    public void MenuLoadCoasterButtonPressed()
    {
        if (_isAnimating) return;
        // _MenuState = 3;
        // GoToSecondMenuPannel();
        // TODO
    }

    public void MenuSaveCoasterButtonPressed()
    {
        if (_isAnimating) return;
        _rollerCoaster.SaveCoaster(_nameInput.text);
        // TODO: Warn player if it was saved
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
        _isAnimating = true;
        _menuAnim.Play("HidePauseMenu");
        yield return new WaitForSeconds(0.75f);
        _menuPannel.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        _menuPannel.GetChild(0).GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(410f, 0f);
        _menuPannel.gameObject.SetActive(false);
        _isAnimating = false;
    }

    
    // ---------------------------- Change Rail Properties Functions ---------------------------- //
    // TODO
    
    public void UpdateRailElevation(string elevation)
    {
        float convertedString = -1;
        try
        {
            convertedString = float.Parse(elevation);
        }
        catch
        {
            return;
        }
        _rollerCoaster.UpdateLastRail(elevation: convertedString * Mathf.PI / 180f);
    }

    public void UpdateRailRotation(string rotation)
    {
        float convertedString = -1;
        try
        {
            convertedString = float.Parse(rotation);
        }
        catch
        {
            return;
        }
        _rollerCoaster.UpdateLastRail(rotation: convertedString * Mathf.PI / 180f);
    }

    public void UpdateRailInclination(string inclination)
    {
        float convertedString = -1;
        try
        {
            convertedString = float.Parse(inclination);
        }
        catch
        {
            return;
        }
        _rollerCoaster.UpdateLastRail(inclination: -convertedString * Mathf.PI / 180f);
    }

    public void UpdateRailLength(string length)
    {
        int convertedString = -1;
        try
        {
            convertedString = int.Parse(length);
        }
        catch
        {
            return;
        }
        _rollerCoaster.UpdateLastRail(length: convertedString);
    }

    private string Rad2DegRound(float angle)
    {
        float rad = angle * 180f / Mathf.PI;
        rad = Mathf.Round(rad);
        return "" + rad;
    }
}
