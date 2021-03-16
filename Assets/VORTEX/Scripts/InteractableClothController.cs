using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

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

    private bool movedOutOfThresholdAfterUnequip = true;

    public void Initialize(InteractableCloth pairedCloth)
    {
        clothingInWorld = pairedCloth;
        SetModelClothActive(modelCloth.isActive);

        modelCloth.onWornClothInteractedSteam += OnWornClothInteractedSteam;
    }

    public void ToggleModelCloth()
    {
        SetModelClothActive(!modelCloth.isActive);
    }

    public void SetModelClothActive(bool state)
    {
        try
        {
            modelCloth.SetActive(state);

            if (modelCloth.isActive)
            {
                movedOutOfThresholdAfterUnequip = false;
            }

            clothingInWorld.SetActiveAndParent(!state, modelCloth.transform, modelCloth.offset);
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
        if (clothingInWorld.isBeingGrabbed)
        {
            if (movedOutOfThresholdAfterUnequip && Vector3.Distance(modelCloth.GetOffsetPosition(), clothingInWorld.GetOffsetPosition()) < distanceThreshold)
            {
                ToggleModelCloth();
            }
            else if (Vector3.Distance(modelCloth.GetOffsetPosition(), clothingInWorld.GetOffsetPosition()) > distanceThreshold)
            {
                movedOutOfThresholdAfterUnequip = true;
            }
        }
    }

    private void OnWornClothInteractedSteam(Hand hand)
    {
        Debug.Log("Steam grab message received");
        ToggleModelCloth();
        clothingInWorld.ManualAttachToHand(hand);

    }
}