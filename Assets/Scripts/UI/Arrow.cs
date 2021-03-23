using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private int _arrowId;
    #pragma warning disable 0649
    [ColorUsage(true, true)]
    [SerializeField] private Color _color;
    #pragma warning disable 0649
    [SerializeField] private Transform _otherArrow;
    [SerializeField] private bool _buttonIn;
    [SerializeField] private bool _buttonDown;

    void Start()
    {
        ChangeColor(1f);
    }

    public void OnPointerEnter()
    {
        _buttonIn = true;
        ChangeColor(1.1f);
    }

    public void OnPointerExit()
    {
        _buttonIn = false;
        if(!(_buttonIn || _buttonDown))
        {
            ChangeColor(1f);
        }
    }

    public void OnPointerDown()
    {
        _buttonDown = true;
        ChangeColor(1.1f);
        ConstructionArrows.inst.OnPointerDownOnArrow(_arrowId);
    }

    public void OnPointerUp()
    {
        _buttonDown = false;
        if(!(_buttonIn || _buttonDown))
        {
            ChangeColor(1f);
        }
        ConstructionArrows.inst.OnPointerUpOnArrow(_arrowId);
    }

    public void OnDrag()
    {
        ConstructionArrows.inst.OnDragOnArrow(_arrowId);
    }

    public void ChangeColor(float scale)
    {
        Color emissionColor = _color * scale;
        this.GetComponent<Renderer>().material.SetColor("_Color", emissionColor);
        _otherArrow.GetComponent<Renderer>().material.SetColor("_Color", emissionColor);
    }
}
