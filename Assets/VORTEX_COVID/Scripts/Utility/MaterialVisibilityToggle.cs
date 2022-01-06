using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialVisibilityToggle : MonoBehaviour
{
    public new Renderer renderer;
    public int[] materialSlotsToToggle;
    public Material defaultMaterial;
    public Material transparentMaterial;

    public void ShowMaterial(int index)
    {
        if (index >= 0 && index < materialSlotsToToggle.Length)
        {
            SetOpacity(materialSlotsToToggle[index], false);
        }
        else
            Debug.LogWarning("MaterialVisibilityToggle material index out of range (" + name + "): Requested " + index + ", Max " + (materialSlotsToToggle.Length - 1));
    }

    public void HideMaterial(int index)
    {
        if (index >= 0 && index < materialSlotsToToggle.Length)
        {
            SetOpacity(materialSlotsToToggle[index], true);
        }
        else
            Debug.LogWarning("MaterialVisibilityToggle material index out of range ("+ name +") : Requested " + index + ", Max " + (materialSlotsToToggle.Length - 1));
    }

    public void ShowAll()
    {
        foreach(int i in materialSlotsToToggle)
        {
            SetOpacity(i, false);
        }
    }

    private void SetOpacity(int index, bool hide)
    {
        if (index >= 0 && index < renderer.materials.Length)
        {
            //renderer.materials[index] = hide == true ? transparentMaterial : originalMaterial;

            Material[] newMaterials = new Material[renderer.materials.Length];

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                if (i == index)
                    newMaterials[i] = hide == true ? transparentMaterial : defaultMaterial;
                else
                    newMaterials[i] = renderer.materials[i];
            }

            renderer.materials = newMaterials;
        }
        else
            Debug.LogWarning("MaterialVisibilityToggle renderer index out of range: Requested " + index + ", Max " + (renderer.materials.Length - 1));
    }
}
