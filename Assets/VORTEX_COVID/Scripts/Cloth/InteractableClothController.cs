using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core;

public class InteractableClothController : MonoBehaviour
{
    // Array of object pairs.
    public ClothPair[] clothingPairs;
    public Collider[] playerCollidersToIgnore;

    void Start()
    {
        List<InteractableCloth> clothingFound = new List<InteractableCloth>(FindObjectsOfType<InteractableCloth>());

        // Since objects not in the prefab can't be linked, find the first cloth with the same name in the scene and pair it
        // NOTE: If there is more than one object in the scene with the same name, it will only choose the first one
        foreach(ClothPair pair in clothingPairs)
        {
            InteractableCloth match = clothingFound.Find((x) => x.clothName == pair.clothName);
            pair.Initialize(match, playerCollidersToIgnore);
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
    public float distanceThreshold = 0.1f; // Distance from the model cloth that the scene cloth needs to be to equip
    public float angleThreshold = 30f; // Angle between the model cloth that the scene cloth needs to be to equip
    public bool equipAtStart = false;
    public bool snapOnGrab = false; // Snap to/from the model when grabbed

    private bool movedOutOfThresholdAfterUnequip = true;

    // Pair the cloth objects and subscribe to the grab event
    public void Initialize(InteractableCloth pairedCloth, Collider[] playerColliders)
    {
        sceneCloth = pairedCloth;
        pairedCloth.SetGrabbableState(!snapOnGrab);
        SetModelClothActive(equipAtStart);
        IgnoreSelfAndPlayerCollision(playerColliders);

        modelCloth.onWornClothInteracted.AddListener(OnWornClothInteracted);
        sceneCloth.onSceneClothInteracted.AddListener(OnSceneClothInteracted);
    }

    public void IgnoreSelfAndPlayerCollision(Collider[] playerColliders)
    {
        Collider[] modelClothColliders = modelCloth.gameObject.GetComponentsInChildren<Collider>();
        Collider[] sceneClothColliders = sceneCloth.gameObject.GetComponentsInChildren<Collider>();

        foreach (Collider modelCollider in modelClothColliders)
        {
            foreach(Collider sceneCollider in sceneClothColliders)
            {
                Physics.IgnoreCollision(modelCollider, sceneCollider); // Ignore collision with model and scene cloth colliders if both are active
            }
        }

        foreach(Collider collider in playerColliders)
        {
            foreach (Collider modelCollider in modelClothColliders)
                Physics.IgnoreCollision(modelCollider, collider); // Ignore collision between player colliders and model PPE
            
            foreach (Collider sceneCollider in sceneClothColliders)
                Physics.IgnoreCollision(sceneCollider, collider); // Ignore collision between player colliders and scene PPE
        }
    }

    // Toggles the active state of both cloth
    public void ToggleModelCloth()
    {
        SetModelClothActive(!modelCloth.isActive);
    }

    // Toggles between snapping and grabbing
    public void ToggleGrabSnap()
    {
        sceneCloth.SetGrabbableState(snapOnGrab);
        snapOnGrab = !snapOnGrab;
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