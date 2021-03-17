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

        // Since objects not in the prefab can't be linked, find the first cloth with the same name in the scene and pair it
        // NOTE: If there is more than one object in the scene with the same name, it will only choose the first one
        foreach(ClothPair pair in clothingPairs)
        {
            InteractableCloth match = clothingFound.Find((x) => x.clothName == pair.clothName);
            pair.Initialize(match);
        }
    }

    void Update()
    {
        // Check if the cloth in the scene aligns with the cloth on the model
        foreach (ClothPair pair in clothingPairs)
            pair.CheckIfWithinThreshold();
    }
}

[System.Serializable]
public class ClothPair
{
    public string clothName; // Name to be matched at start
    public WornCloth modelCloth; // Clothing object on the model
    public InteractableCloth clothingInWorld; // Clothing object in the scene
    public float distanceThreshold = 0.1f;
    public float angleThreshold = 30f;

    private bool movedOutOfThresholdAfterUnequip = true;

    // Pair the cloth objects and subscribe to the grab event
    public void Initialize(InteractableCloth pairedCloth)
    {
        clothingInWorld = pairedCloth;
        SetModelClothActive(modelCloth.isActive);

        modelCloth.onWornClothInteractedSteam += OnWornClothInteractedSteam;
    }

    // Toggles the active state of both cloth
    public void ToggleModelCloth()
    {
        SetModelClothActive(!modelCloth.isActive);
    }

    // Sets the model cloth to a specific state, sets the model and scene cloth to opposite state
    public void SetModelClothActive(bool state)
    {
        try
        {
            modelCloth.SetActive(state);

            if (modelCloth.isActive)
            {
                movedOutOfThresholdAfterUnequip = false;
            }

            // Sets the parent of the scene cloth if the model cloth is active
            clothingInWorld.SetActiveAndParent(!state, modelCloth.transform);
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

    // Check if the cloth in the scene aligns with the cloth on the model in both position and angle
    public void CheckIfWithinThreshold()
    {
        try
        {
            if (clothingInWorld.isBeingGrabbed)
            {
                if (movedOutOfThresholdAfterUnequip && InThresholdDistance() && RotationAligned())
                {
                    ToggleModelCloth();
                }
                else if (!InThresholdDistance())
                {
                    movedOutOfThresholdAfterUnequip = true;
                }
            }
        }
        catch (MissingReferenceException)
        {
            // Do nothing additional, message already printed at start
        }
        catch (System.NullReferenceException)
        {
            // Do nothing additional, message already printed at start
        }

    }

    // Check if the scene cloth is within the distance threshold
    private bool InThresholdDistance()
    {
        float distance = Vector3.Distance(modelCloth.GetOffsetPosition(), clothingInWorld.GetOffsetPosition());
        
        return  distance <= distanceThreshold;
    }

    // Check if the scene cloth is within the rotation threshold
    private bool RotationAligned()
    {
        float angle = Quaternion.Angle(modelCloth.GetRotation(), clothingInWorld.GetRotation());

        return angle <= angleThreshold;
    }

    // Event for when the worn cloth is interacted with by a SteamVR controller
    private void OnWornClothInteractedSteam(Hand hand)
    {
        ToggleModelCloth();
        clothingInWorld.ManualAttachToHandSteam(hand);
    }
}