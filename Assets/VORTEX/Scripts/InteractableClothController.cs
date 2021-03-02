using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableClothController : MonoBehaviour
{
    // Array of object pairs.
    public ClothPair[] clothingPairs;

    void Start()
    {
        foreach(ClothPair pair in clothingPairs)
            pair.Initialize();
    }

    void Update()
    {
        foreach (ClothPair pair in clothingPairs)
            pair.CheckIfWithinThreshold();
    }
}

[System.Serializable]
public class ClothPair
{
    public GameObject modelCloth;
    public InteractableCloth clothingInWorld;
    public bool modelClothActive = false;
    public float distanceThreshold = 0.1f;

    public void Initialize()
    {
        SetModelClothActive(modelClothActive);
    }

    public void ToggleModelCloth()
    {
        SetModelClothActive(!modelClothActive);
    }

    public void SetModelClothActive(bool state)
    {
        modelClothActive = state;

        try
        {
            modelCloth.SetActive(state);
            clothingInWorld.SetActive(!state);
        }
        catch (MissingReferenceException e)
        {
            Debug.LogWarning("Missing objects in a ClothPair." + e.ToString());
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning("Missing objects in a ClothPair." + e.ToString());
        }
    }

    public void CheckIfWithinThreshold()
    {
        if (Vector3.Distance(modelCloth.transform.position, clothingInWorld.transform.position) < distanceThreshold)
            ToggleModelCloth();
    }
}