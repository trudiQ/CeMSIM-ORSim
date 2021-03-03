using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableClothController : MonoBehaviour
{
    // Array of object pairs.
    public ClothPair[] clothingPairs;

    void Start()
    {
        List<InteractableCloth> clothingFound = new List<InteractableCloth>(FindObjectsOfType<InteractableCloth>());

        foreach(ClothPair pair in clothingPairs)
        {
            InteractableCloth match = clothingFound.Find((x) => x.clothName == pair.clothName);
            pair.Initialize(match);
        }
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
    public string clothName;
    public WornCloth modelCloth;
    public InteractableCloth clothingInWorld;
    public bool modelClothActiveOnStart = false;
    public float distanceThreshold = 0.1f;

    public void Initialize(InteractableCloth pairedCloth)
    {
        clothingInWorld = pairedCloth;
        SetmodelClothActiveOnStart(modelCloth.isActive);
    }

    public void ToggleModelCloth()
    {
        SetmodelClothActiveOnStart(!modelCloth.isActive);
    }

    public void SetmodelClothActiveOnStart(bool state)
    {
        try
        {
            modelCloth.SetActive(state);

            if (modelCloth.isActive)
                clothingInWorld.StopGrab();

            clothingInWorld.gameObject.SetActive(!state);
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
        if (clothingInWorld.isBeingGrabbed && Vector3.Distance(modelCloth.GetOffsetPosition(), clothingInWorld.GetOffsetPosition()) < distanceThreshold)
            ToggleModelCloth();
    }
}