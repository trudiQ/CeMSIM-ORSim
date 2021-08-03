using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core;

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
    public InteractableCloth sceneCloth; // Clothing object in the scene
    public float distanceThreshold = 0.1f;
    public float angleThreshold = 30f;
    public bool equipAtStart = false;
    public bool snapOnGrab = false; // Snap to/from the model when grabbed

    private bool movedOutOfThresholdAfterUnequip = true;

    // Pair the cloth objects and subscribe to the grab event
    public void Initialize(InteractableCloth pairedCloth)
    {
        sceneCloth = pairedCloth;
        //pairedCloth.SetGrabbableState(!snapOnGrab);
        SetModelClothActive(equipAtStart);

        modelCloth.onWornClothInteracted.AddListener(OnWornClothInteracted);
        sceneCloth.onSceneClothInteracted.AddListener(OnSceneClothInteracted);
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
                movedOutOfThresholdAfterUnequip = false;

            // Sets the parent of the scene cloth if the model cloth is active
            sceneCloth.SetActiveAndParent(!state, snapOnGrab ? null : modelCloth.transform);
        }
        catch (MissingReferenceException e)
        {
            Debug.LogWarning("Missing objects in a ClothPair. \n" +
                "Model Cloth: " + modelCloth +  ", Scene Cloth: " + sceneCloth + "\n" + e.ToString());
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning("Missing objects in a ClothPair (null). \n" +
                "Model Cloth: " + modelCloth + ", Scene Cloth: " + sceneCloth + "\n" + e.ToString());
        }
    }

    // Check if the cloth in the scene aligns with the cloth on the model in both position and angle
    public void CheckIfWithinThreshold()
    {
        try
        {
            if (sceneCloth.isBeingGrabbed)
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
        float distance = Vector3.Distance(modelCloth.GetOffsetPosition(), sceneCloth.GetOffsetPosition());
        
        return  distance <= distanceThreshold;
    }

    // Check if the scene cloth is within the rotation threshold
    private bool RotationAligned()
    {
        float angle = Quaternion.Angle(modelCloth.GetRotation(), sceneCloth.GetRotation());

        return angle <= angleThreshold;
    }

    // Event for when the scene cloth is interacted with by a SteamVR controller
    /*private void OnSceneClothInteractedSteam(Hand hand)
    {
        if (snapOnGrab)
            ToggleModelCloth();
    }

    // Event for when the worn cloth is interacted with by a SteamVR controller
    private void OnWornClothInteractedSteam(Hand hand)
    {
        ToggleModelCloth();

        if(!snapOnGrab)
            sceneCloth.ManualAttachToHandSteam(hand);
    }*/

    private void OnSceneClothInteracted(HVRHandGrabber grabber, HVRGrabbable grabbable)
    {
        if (snapOnGrab)
            ToggleModelCloth();
    }

    private void OnWornClothInteracted(HVRHandGrabber grabber, HVRGrabbable grabbable)
    {
        ToggleModelCloth();

        if (!snapOnGrab)
            sceneCloth.ManualGrab(grabber);
    }
}