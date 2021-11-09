﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPEClothSwitcher : MonoBehaviour
{
    public GameObject[] objectsWithClothMesh;
    public MaterialVisibilityToggle handMeshMaterialToggle;
    public MultiGloveToggle multiGloveToggle;

    public void Show(int index)
    {
        if (index >= 0 && index < objectsWithClothMesh.Length)
        {
            objectsWithClothMesh[index].SetActive(true);
            handMeshMaterialToggle.HideMaterial(0);
        }
        else
            Debug.LogWarning("ClothSwitcher index out of range.");
    }

    public void Hide(int index)
    {
        if (index >= 0 && index < objectsWithClothMesh.Length)
        {
            objectsWithClothMesh[index].SetActive(false);
            handMeshMaterialToggle.ShowMaterial(0);
        }
        else
            Debug.LogWarning("ClothSwitcher index out of range.");
    }

    public void HideAll()
    {
        multiGloveToggle.GloveUnequipped();
        if (multiGloveToggle.currentGloveEquippedCount == 0)
        {
            Debug.Log("Hide all was called.");
            foreach (GameObject clothMesh in objectsWithClothMesh)
                clothMesh.SetActive(false);

            handMeshMaterialToggle.ShowAll();
        }
    }
}
