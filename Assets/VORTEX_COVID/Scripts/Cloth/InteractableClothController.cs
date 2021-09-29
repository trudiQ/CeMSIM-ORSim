using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core;
using UnityEngine.Events;

public class InteractableClothController : MonoBehaviour
{
    // Array of object pairs.
    public ClothPair[] clothingPairs;
    public Collider[] playerCollidersToIgnore;

    public UnityEvent<ClothPair> OnClothEquipped;
    public UnityEvent<ClothPair> OnClothUnequipped;

    void Start()
    {
        List<InteractableCloth> clothingFound = new List<InteractableCloth>(FindObjectsOfType<InteractableCloth>());

        // Since objects not in the prefab can't be linked, find the first cloth with the same name in the scene and pair it
        // NOTE: If there is more than one object in the scene with the same name, it will only choose the first one
        foreach(ClothPair pair in clothingPairs)
        {
            InteractableCloth match = clothingFound.Find((x) => x.clothName == pair.clothName);

            pair.Initialize(match);
            pair.IgnorePlayerCollision(playerCollidersToIgnore);

            // Subscribe to events so there is one point that information can be sent from
            pair.OnEquip.AddListener((_) => OnClothEquipped.Invoke(pair));
            pair.OnUnequip.AddListener((_) => OnClothUnequipped.Invoke(pair));
        }

        // After setting up every pair, ignore collision between scene and model cloth (includes self collision)
        // This process includes 4 nested for loops, O(n*n*m*s)
        // n = number of cloth pairs, m = number of model colliders, s = number of scene colliders
        foreach(ClothPair pair in clothingPairs)
        {
            foreach(ClothPair innerPair in clothingPairs)
            {
                pair.IgnoreOtherCollision(innerPair);
            }
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
    public float distanceThreshold = 0.2f; // Distance from the model cloth that the scene cloth needs to be to equip
    public float angleThreshold = 30f; // Angle between the model cloth that the scene cloth needs to be to equip
    public bool equipAtStart = false;
    public bool snapOnGrab = false; // Snap to/from the model when grabbed

    public UnityEvent<HVRHandGrabber> OnEquip;
    public UnityEvent<HVRHandGrabber> OnUnequip;

    private bool movedOutOfThresholdAfterUnequip = true;

    public Collider[] modelClothColliders { get; private set; }
    public Collider[] sceneClothColliders { get; private set; }

    public void Initialize(InteractableCloth pairedCloth)
    {
        sceneCloth = pairedCloth; // Pair the cloth objects
        pairedCloth.SetGrabbableState(!snapOnGrab); // Toggle the cloth between grabbable and interactable based on snap
        SetModelClothActive(equipAtStart);

        modelClothColliders = modelCloth.gameObject.GetComponentsInChildren<Collider>(); // Get all colliders from the model cloth
        sceneClothColliders = sceneCloth.gameObject.GetComponentsInChildren<Collider>(); // Get all colliders from the scene cloth

        // Make sure any events are triggered based on what is equipped at start
        if (equipAtStart)
            OnEquip.Invoke(null);
        else
            OnUnequip.Invoke(null);

        modelCloth.onWornClothInteracted.AddListener(OnWornClothInteracted); // Subscribe to the grab event
        sceneCloth.onSceneClothInteracted.AddListener(OnSceneClothInteracted); // Subscribe to the grab event
    }

    // Ignore collision between player colliders and scene PPE
    public void IgnorePlayerCollision(Collider[] playerColliders)
    {
        foreach (Collider collider in playerColliders)
        {
            foreach (Collider modelCollider in modelClothColliders)
                Physics.IgnoreCollision(modelCollider, collider);

            foreach (Collider sceneCollider in sceneClothColliders)
                Physics.IgnoreCollision(sceneCollider, collider);
        }
    }

    // Ignore the collision between scene cloth and an other pair's worn cloth
    public void IgnoreOtherCollision(ClothPair other)
    {
        foreach(Collider sceneCollider in sceneClothColliders)
        {
            foreach(Collider modelCollider in other.modelClothColliders)
            {
                Physics.IgnoreCollision(sceneCollider, modelCollider);
            }
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
        modelCloth.SetActive(state);

        if (modelCloth.isActive)
            movedOutOfThresholdAfterUnequip = false;

        // Sets the parent of the scene cloth if the model cloth is active
        sceneCloth.SetActiveAndParent(!state, snapOnGrab ? null : modelCloth.transform);
    }

    // Check if the cloth in the scene aligns with the cloth on the model in both position and angle
    public void CheckIfWithinThreshold()
    {
        if (sceneCloth.isBeingGrabbed)
        {
            if (movedOutOfThresholdAfterUnequip && InThresholdDistance() && RotationAligned())
            {
                // Get a reference to which grabber held the object, then invoke the equip event with it after the ForceRelease
                HVRHandGrabber grabber = sceneCloth.GetGrabber();

                ToggleModelCloth();
                OnEquip.Invoke(grabber);
            }
            else if (!InThresholdDistance())
            {
                movedOutOfThresholdAfterUnequip = true;
            }
        }
    }

    // Check if the scene cloth is within the distance threshold
    private bool InThresholdDistance()
    {
        float distance = Vector3.Distance(modelCloth.GetPosition(), sceneCloth.GetPosition());

        return  distance <= distanceThreshold;
    }

    // Check if the scene cloth is within the rotation threshold
    private bool RotationAligned()
    {
        float angle = Quaternion.Angle(modelCloth.GetRotation(), sceneCloth.GetRotation());

        return angle <= angleThreshold;
    }

    // Called when the scene cloth sends an event and snap is enabled, disables scene cloth and enables worn cloth
    private void OnSceneClothInteracted(HVRHandGrabber grabber, HVRGrabbable grabbable)
    {
        if (snapOnGrab)
        {
            ToggleModelCloth();
            OnEquip.Invoke(grabber);
        }
    }

    // Called when the worn cloth sends an event, disables worn cloth and enables scene cloth
    private void OnWornClothInteracted(HVRHandGrabber grabber, HVRGrabbable grabbable)
    {
        ToggleModelCloth();
        OnUnequip.Invoke(grabber);

        if (!snapOnGrab)
            sceneCloth.ManualGrab(grabber);
    }
}