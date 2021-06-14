using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
    [SerializeField] private Transform _terrarianPannel;
    #pragma warning disable 0649
    [SerializeField] private InputField _nameInput;
    #pragma warning disable 0649
    [SerializeField] private Dropdown _railTypeDropdown;
    #pragma warning disable 0649
    [SerializeField] private Dropdown _heatmapDropdown;
    #pragma warning disable 0649
    [SerializeField] private Dropdown _modelIdDropdown;
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
    #pragma warning disable 0649
    [SerializeField] private UISettings _UISettings;
    #pragma warning disable 0649
    [SerializeField] private Transform _legendPannel;
    #pragma warning disable 0649
    [SerializeField] private UIWarning _UIWarning;
    #pragma warning disable 0649
    [SerializeField] private Transform _autocompleteButton;
    #pragma warning disable 0649
    [SerializeField] private Color[] _autocompleteButtonColors;
    #pragma warning disable 0649
    [SerializeField] private string[] _heatmapsTranslations = new string[6]
        {
            "none",
            "velocity",
            "GVertical",
            "GFrontal",
            "GLateral",
            "height",
        };
    
    [SerializeField] private bool _isPaused = false;
    [SerializeField] private bool _exitedMenu = false;
    [SerializeField] private bool _isLegendActive = false;

    private Animator _mpAnim;
    private Animator _menuAnim;
    private Animator _legendAnim;

    void Awake()
    {
        _mpAnim = _pannel.GetComponent<Animator>();
        _menuAnim = _menuPannel.GetComponent<Animator>();
        _legendAnim = _legendPannel.GetComponent<Animator>();
    }

    void Start()
    {
        _autocompleteButton.GetComponent<Image>().color = _autocompleteButtonColors[0];
        _UISettings.Initialize();
        if(!_debugMode)
        {
            ShowMenu();
            _isPaused = true;
            _exitedMenu = false;
            _cameraHandler.SetCanMove(false);
            _mpAnim.Play("HideState");
            _isLegendActive = false;
            _legendAnim.Play("HideLegendState");
        }
        else
        {
            _rollerCoaster.Initialize();
            _rollerCoaster.TestBlueprintsMinVelocity();

            // if (_constructorPannelState == 0)
            // {
            //     _rollerCoaster.AddRail(false);
            //     _rollerCoaster.GenerateSupports(_rollerCoaster.GetRailsCount() - 1);
            //     _rollerCoaster.AddRail(true);
            //     _lasRailType = 1;
            //     _railTypeDropdown.value = 1;
            //     _rollerCoaster.UpdateLastRail(railType: 1);
            //     ConstructionArrows.inst.Initialize(_rollerCoaster);
            // }
            // else
            // {        
            //     _rollerCoaster.GenerateCoaster();
            // }

            ShowPannel(_constructorPannelState, false);

            _isPaused = false;
            _exitedMenu = true;
            _cameraHandler.SetCanMove(true);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!_isPaused)
            {
                Pause();
            }
            else
            {
                if(_MenuState == -1)
                {
                    if(_exitedMenu)
                        Unpause();
                }
                else if(_MenuState != 3)
                {
                    GoToFirstMenuPannel();
                }
                else
                {
                    if(!_isTakingAPicture)
                        GoToFirstMenuPannel();
                }
            }
        }
    }

    // From: https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    // From: https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == 5)
                return true;
        }
        return false;
    }

    // From: https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
    //Gets all event system raycast results of current mouse or touch position.
    private static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public void Warn(string key)
    {
        _UIWarning.Warn(key);
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
    // _constructorPannelState: 0 to Relief and 1 to Objects
    [SerializeField] private int _terrarianPannelState = 0;
    

    public void ShowPannel(int constructorPannelState, bool loaded)
    {
        if(_isAnimating) return;
        if(constructorPannelState == 0)
            _railTypeDropdown.value = _lasRailType;
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
        if(loaded)
        {
            _heatmapDropdown.value = 0;
        }
        if (_isLegendActive)
        {
            _legendAnim.Play("ShowLegend");
        }
        _isSimulating = false;
        StartCoroutine(AnimationTime(0.5f));
    }

    public void HidePannel()
    {
        if(_isAnimating) return;
        _lasRailType = _railTypeDropdown.value;
        if(_isLegendActive)
        {
            _legendAnim.Play("HideLegend");
        }
        StartCoroutine(HidePannelCoroutine());
    }

    private IEnumerator HidePannelCoroutine()
    {
        _isAnimating = true;
        if (_pannelState == 1)
        {
            _mpAnim.Play("ObjectsToGenerator");
            yield return new WaitForSeconds(0.25f);
        }

        _pannelState = 0;
        DecorativeObjectPlacer.inst.Close();
        UITerrainBrush.inst.Deactivate();

        if (_constructorPannelState == 0)
            _mpAnim.Play("HideMainPannel");
        else
            _mpAnim.Play("HideGenerationPannel");

        if(_isBlueprintActive)
        {
            UIBlueprint.inst.Close();
            _isBlueprintActive = false;
        }

        ConstructionArrows.inst.ActiveArrows(false);

        yield return new WaitForSeconds(0.5f);
        _isAnimating = false;
    }

    public void TopContructButtonPressed()
    {
        if(_isAnimating || _pannelState == 0) return;
        _pannelState = 0;
        DecorativeObjectPlacer.inst.Close();
        UITerrainBrush.inst.Deactivate();
        Terrain.inst.ActiveColliders(false);

        if (_constructorPannelState == 0)
        {
            if (_terrarianPannelState == 0)
                _mpAnim.Play("ReliefToConstructor");
            else
                _mpAnim.Play("ObjectsToConstructor");
        }
        else
        {
            if (_terrarianPannelState == 0)
                _mpAnim.Play("ReliefToGenerator");
            else
                _mpAnim.Play("ObjectsToGenerator");
        }

        if (_constructorPannelState == 0)
            ConstructionArrows.inst.ActiveArrows(!_rollerCoaster.IsComplete());

        StartCoroutine(AnimationTime(0.25f));
    }

    public void TopTerrainButtonPressed()
    {
        if(_isAnimating || _pannelState == 1) return;
        _pannelState = 1;
        if(_terrarianPannelState == 0)
            UITerrainBrush.inst.Active();
        else
            DecorativeObjectPlacer.inst.Open();
        Terrain.inst.ActiveColliders(true);

        if (_constructorPannelState == 0)
        {
            if (_terrarianPannelState == 0)
                _mpAnim.Play("ConstructorToRelief");
            else
                _mpAnim.Play("ConstructorToObjects");
        }
        else
        {
            if (_terrarianPannelState == 0)
                _mpAnim.Play("GeneratorToRelief");
            else
                _mpAnim.Play("GeneratorToObjects");
        }
        
        if (_constructorPannelState == 0)
            ConstructionArrows.inst.ActiveArrows(false);
        StartCoroutine(AnimationTime(0.25f));
    }

    public void ChangeTerrarianState(int state)
    {
        if (_isAnimating || _pannelState == 0 || state == _terrarianPannelState) return;
        
        if (_terrarianPannelState == 0)
        {
            _terrarianPannelState = 1;
            UITerrainBrush.inst.Deactivate();
            DecorativeObjectPlacer.inst.Open();
            _terrarianPannel.GetComponent<Animator>().Play("ToObjects");
            _mpAnim.Play("ToObjectsHeight");
        }
        else
        {
            _terrarianPannelState = 0;
            DecorativeObjectPlacer.inst.Close();
            UITerrainBrush.inst.Active();
            _terrarianPannel.GetComponent<Animator>().Play("ToRelief");
            _mpAnim.Play("ToReliefHeight");
        }
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
        UIBlueprint.inst.Initialize(_rollerCoaster);
        _mpAnim.Play("ChangeToBlueprint");
        StartCoroutine(AnimationTime(0.75f));

    }

    public void CancelBlueprintButtonPressed()
    {
        if(_isAnimating || !_isBlueprintActive) return;
        _isBlueprintActive = false;
        ConstructionArrows.inst.ActiveArrows(!_rollerCoaster.IsComplete());
        UIBlueprint.inst.Close();
        ConstructionArrows.inst.UpdateArrows();
        _mpAnim.Play("ChangeFromBlueprint");
        StartCoroutine(AnimationTime(0.75f));

    }

    public void SimulateButtonPressed()
    {
        if(_isAnimating) return;
        if(_rollerCoaster.IsGenerating())
        {
            _UIWarning.Warn("warnIsGenerating");
            return;
        }

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
        if(_constructorPannelState == 0)
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
            _lastPannelState = _constructorPannelState;
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
        if(_rollerCoaster.IsComplete()) 
        {
            _UIWarning.Warn("warnComplete");
            return;
        }
        if(_rollerCoaster.CheckLastRailPlacement())
        {
            _rollerCoaster.SetLastRailPreview(false);
            _rollerCoaster.GenerateSupports(_rollerCoaster.GetRailsCount() - 1);
            _rollerCoaster.AddRail(true);
            ConstructionArrows.inst.ActiveArrows(true);
            ConstructionArrows.inst.UpdateArrows();
            if(_rollerCoaster.CanAddFinalRail())
                _autocompleteButton.GetComponent<Image>().color = _autocompleteButtonColors[1];
            else
                _autocompleteButton.GetComponent<Image>().color = _autocompleteButtonColors[0];
        }
        else
        {
            _UIWarning.Warn("warnIntersection");
        }
    }

    public void AutoCompleteButtonPressed()
    {
        if (_rollerCoaster.IsComplete())
        {
            _UIWarning.Warn("warnComplete");
            return;
        }
        if (!_rollerCoaster.CanAddFinalRail())
        {
            _UIWarning.Warn("warnAutocomplete");
            return;
        }
        ConstructionArrows.inst.ActiveArrows(false);
        _rollerCoaster.AddFinalRail();
        
        _rollerCoaster.GenerateSupports(_rollerCoaster.GetRailsCount() - 2);
        _rollerCoaster.GenerateSupports(_rollerCoaster.GetRailsCount() - 1);
        
        UpdateUIValues();
    }

    public void RemoveRailButtonClicked()
    {
        (RailProps rp, ModelProps mp) = _rollerCoaster.RemoveLastRail();
        _rollerCoaster.RemoveSupports(_rollerCoaster.GetRailsCount() - 1);
        _rollerCoaster.SetLastRailPreview(true);
        ConstructionArrows.inst.ActiveArrows(true);
        ConstructionArrows.inst.UpdateArrows();
        if (rp == null)
            _UIWarning.Warn("warnCantRemovePlataform");
        if (_rollerCoaster.CanAddFinalRail())
            _autocompleteButton.GetComponent<Image>().color = _autocompleteButtonColors[1];
        else
            _autocompleteButton.GetComponent<Image>().color = _autocompleteButtonColors[0];
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
        _rollerCoaster.UpdateLastRail(railType: type);
        UpdateUIValues();
    }

    public void SetHeatmap(int type)
    {
        _rollerCoaster.SetHeatmap(type);
        if(type == 0)
        {
            if(_isLegendActive)
            {
                _legendAnim.Play("HideLegend");
                _isLegendActive = false;
            }
        }
        else
        {
            if (!_isLegendActive)
            {
                _legendAnim.Play("ShowLegend");
                _isLegendActive = true;
            }
            _legendPannel.GetChild(0).GetChild(0).GetComponent<Text>().text = Translator.inst.GetTranslation(_heatmapsTranslations[type]);

            if(type == 1)
            {
                _legendPannel.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = "0<size=20>m/s</size>";
                _legendPannel.GetChild(1).GetChild(0).GetChild(2).GetComponent<Text>().text = "20<size=20>m/s</size>";
            }
            else if (type == 5)
            {
                _legendPannel.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = "0<size=20>m</size>";
                _legendPannel.GetChild(1).GetChild(0).GetChild(2).GetComponent<Text>().text = "50<size=20>m</size>";
            }
            else
            {
                _legendPannel.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = "-10<size=20>g</size>";
                _legendPannel.GetChild(1).GetChild(0).GetChild(2).GetComponent<Text>().text = "10<size=20>g</size>";
            }
        }
    }

    public void TranslateLegend()
    {
        _legendPannel.GetChild(0).GetChild(0).GetComponent<Text>().text = Translator.inst.GetTranslation(_heatmapsTranslations[_heatmapDropdown.value]);
    }
    

    // ---------------------------- Pause Pannel Normal Buttons ---------------------------- //

    private bool _showArrowsWhenUnpause = false;
    private int _lastPannelState = -1;
    private bool _isTakingAPicture = false;

    public void Pause()
    {
        ShowPause();
        _isPaused = true;
        _rollerCoaster.SetPauseCarSimulation(true);
        if(ConstructionArrows.inst.IsActive)
        {
            _showArrowsWhenUnpause = true;
            ConstructionArrows.inst.ActiveArrows(false);
        }

    }

    public void Unpause()
    {
        if (_isAnimating) return;
        _rollerCoaster.SetPauseCarSimulation(false);
        _isPaused = false;
        if (_showArrowsWhenUnpause)
        {
            _showArrowsWhenUnpause = false;
            if(_pannelState == 0 && _constructorPannelState == 0)
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
        _constructorPannelState = 0;
        GoToSecondMenuPannel();
    }

    public void MenuGenerateCoasterButtonPressed()
    {
        if (_isAnimating) return;
        _MenuState = 0;
        _pannelState = 0;
        _constructorPannelState = 1;
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
        ((string, Vector3, float)[] decorativeObjects, float[] terrain) = _rollerCoaster.LoadCoaster(coasterName);
        DecorativeObjectPlacer.inst.DestroyAllDecorativeObjects();
        DecorativeObjectPlacer.inst.Place(decorativeObjects);
        Terrain.inst.SetAplifiers(terrain);
        _lasRailType = (int) _rollerCoaster.GetLastRail().mp.Type;
        ConstructionArrows.inst.Initialize(_rollerCoaster);
        _isPaused = false;
        _exitedMenu = true;
        _cameraHandler.SetCanMove(true);
        ShowPannel(0, true);
        _nameInput.text = coasterName;
        _UIRailPhysics.UpdateValues(_rollerCoaster, false);
        _isAnimating = false;
        Unpause();
    }

    public void MenuSaveCoasterButtonPressed()
    {
        if (_isAnimating) return;
        _MenuState = 3;
        _isTakingAPicture = false;
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(0).gameObject.SetActive(true);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(1).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(2).gameObject.SetActive(false);
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetChild(3).GetComponent<InputField>().text = _nameInput.text;
        UpdateCoasterExistsWarning();
        _flash.gameObject.SetActive(true);
        GoToSecondMenuPannel();
    }

    public void UpdateCoasterExistsWarning()
    {
        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetChild(4).gameObject.SetActive(_rollerCoaster.CoasterExists(_nameInput.text));
    }

    public void MenuContinueSaveButtonPressed()
    {
        if (_isAnimating) return;
        _isTakingAPicture = true;
        _screenshotCamera.gameObject.SetActive(true);
        _screenshotCamera.GetChild(0).GetComponent<Text>().text = _nameInput.text;
        _menuPannel.gameObject.SetActive(false);
        _pannel.gameObject.SetActive(false);
        _cameraHandler.SetCanMove(true);
    }

    public void Save()
    {
        StartCoroutine(SaveAnimation());
    }

    private IEnumerator SaveAnimation()
    {
        _isTakingAPicture = false;
        _screenshotCamera.gameObject.SetActive(false);
        yield return null;
        bool saved = _rollerCoaster.SaveCoaster(_nameInput.text, DecorativeObjectPlacer.inst.GetAllDecorativeObjects(), Terrain.inst.GetAplifiers());
        yield return null;
        _flash.GetComponent<Animator>().Play("Flash");
        AudioManager.inst.PlayUIAudio(0);
        yield return null;
        if(saved)
        {
            float t = 0;
            while(t < 2f && !_rollerCoaster.CoasterExists(_nameInput.text))
            {
                t += Time.deltaTime;
                yield return null;
            }
            saved = _rollerCoaster.CoasterExists(_nameInput.text);
        }
        _pannel.gameObject.SetActive(true);
        _menuPannel.gameObject.SetActive(true);
        _cameraHandler.SetCanMove(false);

        _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(0).gameObject.SetActive(false);
        if (saved)
            _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(1).gameObject.SetActive(true);
        else
            _menuPannel.GetChild(0).GetChild(1).GetChild(4).GetChild(2).gameObject.SetActive(true);
    }

    public void MenuExitButtonPressed()
    {
        if (_isAnimating) return;
        Application.Quit();
    }

    public void MenuReturnToMenuButtonPressed()
    {
        if (_isAnimating) return;
        SceneManager.LoadScene(0);
    }

    public void MenuContinueButtonPressed()
    {
        if (_isAnimating) return;
        StartCoroutine(MenuContinueCoroutine());
    }

    private IEnumerator MenuContinueCoroutine()
    {
        _rollerCoaster.Initialize();
        int modelId = _modelIdDropdown.value;
        if (modelId == 0)
            modelId = 1;
        else
            modelId += 2;
        _rollerCoaster.ChangeRailModel(modelId);
        if(_constructorPannelState == 0)
        {
            _rollerCoaster.AddRail(false);
            _rollerCoaster.GenerateSupports(_rollerCoaster.GetRailsCount() - 1);
            _rollerCoaster.AddRail(true);
            _lasRailType = 1;
            _railTypeDropdown.value = 1;
            _rollerCoaster.UpdateLastRail(railType: 1);
        }
        else
        {
            _rollerCoaster.GenerateCoaster();
            Terrain.inst.SetAplifiers(null);
        }
        yield return null;
        ShowPannel(_constructorPannelState, false);
        _isAnimating = true;
        _isPaused = false;
        _exitedMenu = true;
        _nameInput.text = Translator.inst.GetTranslation("coasterName");
        _menuAnim.Play("HidePauseMenu");
        _cameraHandler.SetCanMove(true);
        yield return new WaitForSeconds(0.375f);
        _cameraHandler.SetCanMove(true);
        _nameInput.text = Translator.inst.GetTranslation("coasterName");
        _menuPannel.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        _menuPannel.GetChild(0).GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(410f, 0f);
        _menuPannel.gameObject.SetActive(false);
        _settingsButton.SetActive(true);
        _isAnimating = false;
    }

    // ---------------------------- UI Text Functions ---------------------------- //

    public void UpdateUIValues ()
    {
        if (_constructorPannelState == 0)
            _UIRailProps.UpdateValues(_rollerCoaster);
        _UIRailPhysics.UpdateValues(_rollerCoaster, _isSimulating);
    }
}
