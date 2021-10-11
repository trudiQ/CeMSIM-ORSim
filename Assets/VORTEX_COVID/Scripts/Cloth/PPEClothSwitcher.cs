using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPEClothSwitcher : MonoBehaviour
{
    public GameObject[] objectsWithMesh;
    public GameObject objectWithHandMesh;

    public void Show(int index)
    {
        if (index >= 0 && index < objectsWithMesh.Length)
        {
            objectsWithMesh[index].SetActive(true);
            objectWithHandMesh.SetActive(false);
        }
        else
            Debug.LogWarning("ClothSwitcher index out of range.");
    }

    public void Hide(int index)
    {
        if (index >= 0 && index < objectsWithMesh.Length)
            objectsWithMesh[index].SetActive(false);
        else
            Debug.LogWarning("ClothSwitcher index out of range.");
    }

    public void HideAll()
    {
        foreach (GameObject mesh in objectsWithMesh)
            mesh.SetActive(false);

        objectWithHandMesh.SetActive(true);
    }
}
