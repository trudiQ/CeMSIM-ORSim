using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPESpawner : MonoBehaviour
{
    public GameObject ppePrefab;
    [Tooltip("(Optional) Add a PPE GameObject to this if it is already in the scene at the start.")]
    public GameObject sceneObject;
    public float respawnThresholdDistance = 0.5f;

    private GameObject currentlyTrackedObject;
    private InteractableClothController clothController;

    private void Start()
    {
        clothController = FindObjectOfType<InteractableClothController>();

        if (!sceneObject)
            SpawnPPE();
        else
            currentlyTrackedObject = sceneObject;
    }

    private void Update()
    {
        if (Vector3.Distance(currentlyTrackedObject.transform.position, transform.position) > respawnThresholdDistance)
            SpawnPPE();
    }

    private void SpawnPPE()
    {
        if (ppePrefab)
        {
            currentlyTrackedObject = Instantiate(ppePrefab, transform.position, transform.rotation);
            clothController.AddSceneCloth(currentlyTrackedObject.GetComponent<InteractableCloth>());
        }
        else
            Debug.LogWarning("No prefab specified for " + gameObject.name);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, respawnThresholdDistance);
    }
}
