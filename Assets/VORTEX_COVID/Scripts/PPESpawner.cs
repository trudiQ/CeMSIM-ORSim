using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPESpawner : MonoBehaviour
{
    public GameObject ppePrefab;
    public float respawnThresholdDistance = 0.5f;

    private GameObject currentlyTrackedObject;
    private InteractableClothController clothController;

    private void Start()
    {
        clothController = FindObjectOfType<InteractableClothController>();

        SpawnPPE();
    }

    private void Update()
    {
        if (Vector3.Distance(currentlyTrackedObject.transform.position, transform.position) > respawnThresholdDistance)
            SpawnPPE();
    }

    private void SpawnPPE()
    {
        currentlyTrackedObject = Instantiate(ppePrefab, transform.position, transform.rotation);
        clothController.AddSceneCloth(currentlyTrackedObject.GetComponent<InteractableCloth>());
    }
}
