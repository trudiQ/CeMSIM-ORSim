using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPEClothSwitcher : MonoBehaviour
{
    public GameObject[] objectsWithClothMesh;
    public MaterialVisibilityToggle handMeshMaterialToggle;
    public InteractableClothController clothController;
    public string clothName;

    private ClothPair pair;

    void Start()
    {
        pair = clothController.clothingPairs.Find((x) => x.clothName == clothName);
    }

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
        if (pair.equipCount == 0)
        {
            Debug.Log("Hide all was called.");
            foreach (GameObject clothMesh in objectsWithClothMesh)
                clothMesh.SetActive(false);

            handMeshMaterialToggle.ShowAll();
        }
    }
}
