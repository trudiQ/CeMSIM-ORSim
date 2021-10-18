using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PPEOptionPoint : MonoBehaviour
{
    public UnityEvent OnHover;
    public UnityEvent OnUnhover;
    public UnityEvent OnSelect;

    public Color unhoverColor = Color.gray;
    public Color hoverColor = Color.blue;

    public Image colorContainer;

    private bool hovered = false;

    private void Start()
    {
        colorContainer.color = unhoverColor;
    }

    public void Hover()
    {
        if (!hovered)
        {
            OnHover.Invoke();
            colorContainer.color = hoverColor;
            hovered = true;
        }
    }

    public void Unhover()
    {
        if (hovered)
        {
            OnUnhover.Invoke();
            colorContainer.color = unhoverColor;
            hovered = false;
        } 
    }

    public void Select()
    {
        OnSelect.Invoke();
    }
}
