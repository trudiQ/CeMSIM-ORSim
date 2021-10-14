using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialVisibilityToggle : MonoBehaviour
{
    public new Renderer renderer;
    public int[] materialSlotsToToggle;

    public void ShowMaterial(int index)
    {
        if (index >= 0 && index < materialSlotsToToggle.Length)
        {
            SetOpacity(materialSlotsToToggle[index], 1);
        }
        else
            Debug.LogWarning("MaterialVisibilityToggle material index out of range: Requested " + index + ", Max " + (materialSlotsToToggle.Length - 1));
    }

    public void HideMaterial(int index)
    {
        if (index >= 0 && index < materialSlotsToToggle.Length)
        {
            SetOpacity(materialSlotsToToggle[index], 0);
        }
        else
            Debug.LogWarning("MaterialVisibilityToggle material index out of range: Requested " + index + ", Max " + (materialSlotsToToggle.Length - 1));
    }

    public void ShowAll()
    {
        foreach(int i in materialSlotsToToggle)
        {
            SetOpacity(i, 1);
        }
    }

    private void SetOpacity(int index, float opacity)
    {
        if (index >= 0 && index < renderer.materials.Length)
        {
            Material mat = renderer.materials[index];

            mat.SetColor("_Color", new Color(mat.color.r, mat.color.g, mat.color.b, opacity));
        }
        else
            Debug.LogWarning("MaterialVisibilityToggle renderer index out of range: Requested " + index + ", Max " + (renderer.materials.Length - 1));
    }
}
