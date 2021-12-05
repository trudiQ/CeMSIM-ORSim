using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(GrabPointHover))]
public class PPEUnequipOptions : MonoBehaviour
{
    public enum GloveUnequipState { Wrist, Palm, Fingertips }

    public PPEOptionPoint[] unequipPoints;
    public GloveUnequipState unequipState { get; private set; }

    public UnityEvent<GloveUnequipState> onUnequipOptionSelected;

    private GloveUnequipState hoveredPoint;

    public void PPEHovered()
    {
        foreach (PPEOptionPoint point in unequipPoints)
            point.gameObject.SetActive(true);
    }

    public void PPEUnhovered()
    {
        foreach (PPEOptionPoint point in unequipPoints)
            point.gameObject.SetActive(false);
    }

    public void PointHovered(int index)
    {
        if (index > 0 && index < unequipPoints.Length)
        {
            unequipPoints[index].Hover();
            hoveredPoint = (GloveUnequipState)index;

            Debug.Log(unequipPoints[index].name + " hovered");
        }
    }

    public void PointUnhovered(int index)
    {
        if (index > 0 && index < unequipPoints.Length)
            unequipPoints[index].Unhover();
        Debug.Log(unequipPoints[index].name + " unhovered");
    }

    public void OnInteracted()
    {
        onUnequipOptionSelected.Invoke(hoveredPoint);
    }
}
