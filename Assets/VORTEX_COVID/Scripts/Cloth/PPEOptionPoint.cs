using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PPEOptionPoint : MonoBehaviour
{
    public UnityEvent OnHover;
    public UnityEvent OnUnhover;
    public UnityEvent OnSelect;

    private bool hovered = false;

    public void Hover()
    {
        if (!hovered)
        {
            OnHover.Invoke();
            hovered = true;
        }
    }

    public void Unhover()
    {
        if (hovered)
        {
            OnUnhover.Invoke();
            hovered = false;
        } 
    }

    public void Select()
    {
        if (hovered)
        {
            OnSelect.Invoke();
        }
    }
}
