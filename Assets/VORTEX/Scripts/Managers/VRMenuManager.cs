using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMenuManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject canvasPrefab;
    public GameObject[] menuPrefabs;

    [Header("References")]
    public Camera vRCamera;

    [HideInInspector]
    public Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        canvas = Instantiate(canvasPrefab, vRCamera.transform).GetComponent<Canvas>() as Canvas;
        
        for(int i=0; i<menuPrefabs.Length;i++)
        {
            Instantiate(menuPrefabs[i], canvas.transform);
        }
    }
}
