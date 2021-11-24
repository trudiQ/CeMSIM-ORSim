using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPESpawner : MonoBehaviour
{
    public GameObject ppePrefab;
    public float respawnThresholdDistance = 0.5f;

    private GameObject currentlyTrackedObject;

    private void Start()
    {
        Instantiate(ppePrefab, transform.position, transform.rotation);
    }

    private void Update()
    {
        if (Vector3.Distance(currentlyTrackedObject.transform.position, transform.position) > respawnThresholdDistance)
            Instantiate(ppePrefab, transform.position, transform.rotation);
    }
}
