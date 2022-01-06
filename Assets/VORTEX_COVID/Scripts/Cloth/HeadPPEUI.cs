using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadPPEUI : MonoBehaviour
{
    public Image bouffantPanelImage;
    public Image maskPanelImage;
    public Image shieldPanelImage;
    public Color defaultColor = Color.gray;
    public Color highlightedColor = Color.blue;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void BouffantHovered()
    {
        Hover(bouffantPanelImage);
    }

    public void BouffantUnhovered()
    {
        Unhover(bouffantPanelImage);
    }

    public void FaceMaskHovered()
    {
        Hover(maskPanelImage);
    }

    public void FaceMaskUnhovered()
    {
        Unhover(maskPanelImage);
    }

    public void FaceShieldHovered()
    {
        Hover(shieldPanelImage);
    }

    public void FaceShieldUnhovered()
    {
        Unhover(shieldPanelImage);
    }

    private void Hover(Image image)
    {
        gameObject.SetActive(true);
        image.color = highlightedColor;
    }
    
    private void Unhover(Image image)
    {
        gameObject.SetActive(false);
        image.color = defaultColor;
    }
}
