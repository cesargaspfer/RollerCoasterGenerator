using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectManager : MonoBehaviour
{
    private static UIObjectManager _inst;
    public static UIObjectManager inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<UIObjectManager>();
            return _inst;
        }
    }

    #pragma warning disable 0649
    [SerializeField] private Transform _UIObjectPrefab;
    #pragma warning disable 0649
    [SerializeField] private Transform _UIObjectButtonPrefab;
    #pragma warning disable 0649
    [SerializeField] private Transform _UIObjectsContent;
    #pragma warning disable 0649
    [SerializeField] private Transform _UIObjectButtonsContent;
    #pragma warning disable 0649
    [SerializeField] private GameObject[] _pageButtons;
    #pragma warning disable 0649
    [SerializeField] private GameObject _deselectButton;
    [SerializeField] private UIObjectButton[] _UIObjectButtons;
    [SerializeField] private Transform[] _UIObjects;
    [SerializeField] private List<string> _objectsNames;
    [SerializeField] private int _currentPage;
    [SerializeField] private string _currentObjectName;

    void Awake()
    {
        _objectsNames = new List<string>();

        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/Resources/Objects/");
        for(int i = 0; i < fileEntries.Length; i++)
        {
            if(!fileEntries[i].EndsWith(".meta"))
            {
                string fileName = Path.GetFileName(fileEntries[i]);
                fileName = fileName.Substring(0, fileName.Length - 4);
                _objectsNames.Add(fileName);
            }
        }

        _UIObjectButtons = new UIObjectButton [9];
        _UIObjects = new Transform [9];
        for(int i = 0; i < 9; i++)
        {
            (_UIObjectButtons[i], _UIObjects[i]) = InstantiateUIObjectButtom(i);
        }

        _currentObjectName = "";

        _deselectButton.SetActive(false);

        ChangeToPage(0);
    }

    private (UIObjectButton, Transform) InstantiateUIObjectButtom(int index)
    {

        Transform uiObjectButton = Instantiate(_UIObjectButtonPrefab, Vector3.zero, Quaternion.identity);
        uiObjectButton.SetParent(_UIObjectButtonsContent);
        uiObjectButton.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
        uiObjectButton.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3((index % 3) * 90 - 90, -50 - 90 * (index / 3), 0);
        uiObjectButton.transform.localScale = Vector3.one;

        (Transform uiObject, RenderTexture rt) = InstantiateUIObject(index);

        UIObjectButton uiObjectButtonScr = uiObjectButton.GetComponent<UIObjectButton>();
        uiObjectButtonScr.Initialize("null", rt);

        return (uiObjectButtonScr, uiObject);
    }

    private (Transform, RenderTexture) InstantiateUIObject(int index)
    {
        Transform uiObject = Instantiate(_UIObjectPrefab, Vector3.zero, Quaternion.identity);
        uiObject.SetParent(_UIObjectsContent);
        uiObject.transform.localPosition = new Vector3(index * 40, 0, 0);
        uiObject.transform.localScale = Vector3.one;

        RenderTexture renderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        renderTexture.Create();
        uiObject.GetChild(0).GetComponent<Camera>().targetTexture = renderTexture;
        // TODO: check if there is no to release the texture 
        renderTexture.Release();

        return (uiObject, renderTexture);
    }

    private void ChangeObjectInUIObject(string objectName, int index)
    {
        Transform uiObject = _UIObjects[index];

        foreach(Transform tr in uiObject.GetChild(1))
        {
            GameObject.Destroy(tr.gameObject);
        }

        if (objectName != "null")
        {
            GameObject instance = Instantiate(Resources.Load("Objects/" + objectName, typeof(GameObject))) as GameObject;
            instance.transform.SetParent(uiObject.GetChild(1));
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localEulerAngles = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.layer = 8;
            foreach(Transform child in instance.transform)
                child.gameObject.layer = 8;
        }
    }

    public void ChangeToPreviousPage()
    {
        ChangeToPage(_currentPage - 1);
    }

    public void ChangeToNextPage()
    {
        ChangeToPage(_currentPage + 1);
    }

    public void ChangeToPage(int page)
    {
        _currentPage = page;
        int initialIndex = page * 9;

        if(page == 0)
            _pageButtons[0].SetActive(false);
        else
            _pageButtons[0].SetActive(true);

        int buttonIndex = 0;
        for(int i = initialIndex; i < Mathf.Min(initialIndex + 9, _objectsNames.Count); i++)
        {
            _UIObjectButtons[buttonIndex].gameObject.SetActive(true);
            _UIObjectButtons[buttonIndex].ChangeObjectName(_objectsNames[i]);
            ChangeObjectInUIObject(_objectsNames[i], buttonIndex);
            buttonIndex++;
        }

        if (initialIndex + 9 >= _objectsNames.Count)
            _pageButtons[1].SetActive(false);
        else
            _pageButtons[1].SetActive(true);

        for (int i = buttonIndex; i < 9; i++)
        {
            _UIObjectButtons[i].gameObject.SetActive(false);
        }
    }

    public void DeselectButtonClicked()
    {
        UIObjectButtonClicked(_currentObjectName);
    }

    public void UIObjectButtonClicked(string objectName)
    {
        if(_currentObjectName.Equals(objectName))
        {
            _currentObjectName = "";
            DecorativeObjectPlacer.inst.StopPlacement();
            _deselectButton.SetActive(false);
        }
        else
        {
            _currentObjectName = objectName;
            DecorativeObjectPlacer.inst.StartPlacement(_currentObjectName);
            _deselectButton.SetActive(true);
        }
    }
}
