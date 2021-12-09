using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnRandomizer : MonoBehaviour
{
    public GameObject[] prefabs;
    public GameObject[] spawnLocations;

    private void Start()
    {
        List<GameObject> availableLocations = new List<GameObject>();
        availableLocations.AddRange(spawnLocations);

        if (spawnLocations.Length < prefabs.Length)
        {
            Debug.LogError("ObjectSpawnRandomizer has unequal object and spawn location arrays.");
            return;
        }

        foreach (GameObject prefab in prefabs)
        {
            int index = Random.Range(0, availableLocations.Count);

            Instantiate(prefab, availableLocations[index].transform.position, availableLocations[index].transform.rotation);

            availableLocations.RemoveAt(index);
        }
    }
}
