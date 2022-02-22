using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaterialSwitcher : MonoBehaviour
{
    public int materialIndex = 0;
    public Material materialToSwitchTo;

    public void SwitchMaterials()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material[] foundMaterials = renderer.materials;
        foundMaterials[materialIndex] = materialToSwitchTo;
        renderer.materials = foundMaterials;
    }
}
