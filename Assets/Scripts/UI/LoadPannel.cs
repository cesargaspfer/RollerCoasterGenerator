using UnityEngine;
using UnityEngine.UI;

public class LoadPannel : MonoBehaviour
{

    #pragma warning disable 0649
    [SerializeField] private Transform _loadCoasterButton;
    #pragma warning disable 0649
    [SerializeField] private Transform _buttonsContent;
    #pragma warning disable 0649
    [SerializeField] private Text _coasterName;
    #pragma warning disable 0649
    [SerializeField] private Image _coasterImage;
    #pragma warning disable 0649
    [SerializeField] private GameObject _loadButton;

    private string[] _coastersNames;
    private Sprite[] _coastersScreenshots;
    private int _selectedId;

    public void Initialize(RollerCoaster rollerCoaster)
    {
        (_coastersNames, _coastersScreenshots) = rollerCoaster.LoadCoastersImages();

        foreach(Transform gm in _buttonsContent)
        {
            GameObject.Destroy(gm.gameObject);
        }

        _buttonsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, _coastersNames.Length * 45f);
        for (int i = 0; i < _coastersNames.Length; i++)
        {
            var instButton = Instantiate(_loadCoasterButton, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            instButton.transform.SetParent(_buttonsContent.transform);
            instButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            instButton.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
            instButton.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, -i * 45f - 25f, 0.0f);

            int id = i;
            instButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { LoadCoasterButtonClicked((int) id); });
            instButton.GetChild(0).GetComponentInChildren<Text>().text = _coastersNames[i];
        }

        if (_coastersNames.Length == 0)
        {
            _loadButton.SetActive(false);
            _coasterImage.sprite = null;
            // TODO: Translate
            _coasterName.text = "Nenhuma montanha-russa encontrada.";
        }
        else
        {
            _loadButton.SetActive(true);
            LoadCoasterButtonClicked(0);
        }
    }

    public void LoadCoasterButtonClicked(int id)
    {
        _coasterName.text = _coastersNames[id];
        _coasterImage.sprite = _coastersScreenshots[id];
        _selectedId = id;
    }

    public void LoadButtonClicked()
    {
        UIManager.inst.LoadCoaster(_coastersNames[_selectedId]);
    }
}
