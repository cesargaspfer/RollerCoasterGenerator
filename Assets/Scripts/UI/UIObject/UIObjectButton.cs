using UnityEngine;
using UnityEngine.UI;

public class UIObjectButton : MonoBehaviour
{
    [SerializeField] private string _name;

    public void Initialize(string name, RenderTexture rt)
    {
        _name = name;
        this.transform.GetChild(0).GetComponent<RawImage>().texture = rt;
    }

    public void ChangeObjectName(string name)
    {
        _name = name;
    }

    public void OnClick()
    {
        UIObjectManager.inst.UIObjectButtonClicked(_name);
    }

    public RawImage UIImage
    {
        get { return this.transform.GetChild(0).GetComponent<RawImage>(); }
    }
}
