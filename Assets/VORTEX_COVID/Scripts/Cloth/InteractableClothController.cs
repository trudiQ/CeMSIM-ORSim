using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core;
using UnityEngine.Events;

public class InteractableClothController : MonoBehaviour
{
    // Array of object pairs.
    public List<ClothPair> clothingPairs;
    public Collider[] playerCollidersToIgnore;

    public UnityEvent<ClothPair> OnClothEquipped;
    public UnityEvent<ClothPair> OnClothUnequipped;

    // Manager for handling equipment events and teleportation
    private TeleportManager teleportManager;

    void Start()
    {
        teleportManager = FindObjectOfType<TeleportManager>();
        List<InteractableCloth> clothingFound = new List<InteractableCloth>(FindObjectsOfType<InteractableCloth>());

        // Since objects not in the prefab can't be linked, find the first cloth with the same name in the scene and pair it
        foreach (ClothPair pair in clothingPairs)
        {
            List<InteractableCloth> matches = clothingFound.FindAll((x) => x.clothName == pair.clothName);

            // Subscribe to events so there is one point that information can be sent from
            pair.OnEquip.AddListener((_) =>
            {
                CheckIfAllPPEEquipped();
                OnClothEquipped.Invoke(pair);
            });

            pair.OnUnequip.AddListener((_) =>
            {
                teleportManager.OnPPEUnequipped();
                OnClothUnequipped.Invoke(pair);
            });

            pair.Initialize(matches);
            pair.IgnorePlayerCollision(playerCollidersToIgnore);
        }

        // After setting up every pair, ignore collision between scene and model cloth (includes self collision)
        // This process includes 4 nested for loops, O(n*n*m*s)
        // n = number of cloth pairs, m = number of model colliders, s = number of scene colliders
        foreach (ClothPair pair in clothingPairs)
        {
            foreach (ClothPair otherPair in clothingPairs)
            {
                pair.IgnoreOtherCollision(otherPair);
            }
        }
    }

    void Update()
    {
        // Check if the cloth in the scene aligns with the cloth on the model
        foreach (ClothPair pair in clothingPairs)
            pair.CheckIfWithinThreshold();
    }

    private void CheckIfAllPPEEquipped()
    {
        bool allEquipped = true;
        foreach (ClothPair pair in clothingPairs)
        {
            if (!pair.hasOneEquipped)
            {
                allEquipped = false;
                break;
            }
        }
        if (allEquipped)
        {
            teleportManager.OnAllPPEsEquipped();
        }
    }

    public void AddSceneCloth(InteractableCloth sceneCloth)
    {
        ClothPair pair = clothingPairs.Find((x) => x.clothName == sceneCloth.clothName);
        pair.AddSceneCloth(sceneCloth);

        pair.IgnorePlayerCollision(playerCollidersToIgnore);

        foreach (ClothPair otherPair in clothingPairs)
            pair.IgnoreOtherCollision(otherPair);
    }

    public void RemoveSceneCloth(InteractableCloth sceneCloth)
    {
        ClothPair pair = clothingPairs.Find((x) => x.clothName == sceneCloth.clothName);
        pair.RemoveSceneCloth(sceneCloth);
    }
}

[System.Serializable]
public class ClothPair
{
    public string clothName; // Name to be matched at start
    public WornCloth modelCloth; // Clothing object on the model
    
    public float distanceThreshold = 0.2f; // Distance from the model cloth that the scene cloth needs to be to equip
    public float angleThreshold = 30f; // Angle between the model cloth that the scene cloth needs to be to equip
    public bool equipAtStart = false;
    public bool manuallyHandleEquip = false; // Use this if equipping PPE depends on external conditions
    public bool manuallyHandleUnequip = false; // Use this if unequipping PPE depends on external conditions
    public bool snapOnGrab = false; // Snap to/from the model when grabbed
    public bool canEquipMultiple = false;
    public bool ignoreAutomaticMeshHide = false; // Enable or disable automatically disabling the worn PPE when unequipping

    public UnityEvent<HVRHandGrabber> OnEquip;
    public UnityEvent<HVRHandGrabber> OnUnequip;
    public UnityEvent<HVRHandGrabber, ClothPair, InteractableCloth> OnAttemptedEquip;
    public UnityEvent<HVRHandGrabber, ClothPair> OnAttemptedUnequip;

    private List<InteractableCloth> sceneCloth = new List<InteractableCloth>(); // Clothing objects in the scene
    private Queue<InteractableCloth> currentlyEquippedCloth = new Queue<InteractableCloth>();
    public int equipCount { get { return currentlyEquippedCloth.Count; } private set { } }
    public bool hasOneEquipped { get { return equipCount > 0; } private set { } }

    private List<Collider> modelClothColliders = new List<Collider>();
    private List<Collider> sceneClothColliders = new List<Collider>();

    public void Initialize(List<InteractableCloth> pairedCloth)
    {
        sceneCloth.AddRange(pairedCloth); // Pair the cloth objects

        modelClothColliders.AddRange(modelCloth.gameObject.GetComponentsInChildren<Collider>()); // Get all colliders from the model cloth
        modelCloth.onWornClothInteracted.AddListener(OnWornClothInteracted); // Subscribe to the grab event
        
        foreach (InteractableCloth cloth in sceneCloth)
        {
            cloth.SetGrabbableState(snapOnGrab); // Toggle the cloth between grabbable and interactable based on snap
            sceneClothColliders.AddRange(cloth.gameObject.GetComponentsInChildren<Collider>()); // Get all colliders from the scene cloth
            cloth.onSceneClothInteracted.AddListener((x, y) => OnSceneClothInteracted(x, y, cloth)); // Subscribe to the grab event
        }

        // Make sure any events are triggered based on what is equipped at start
        if (equipAtStart && sceneCloth.Count > 0)
            Equip(null, sceneCloth[0]);
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
        foreach (Collider sceneCollider in sceneClothColliders)
        {
            foreach (Collider modelCollider in other.modelClothColliders)
            {
                Physics.IgnoreCollision(sceneCollider, modelCollider);
            }
        }
    }

    // Sets the model cloth to a specific state, sets the model and scene cloth to opposite state
    public void ToggleClothVisibility(bool equipping, InteractableCloth sceneCloth)
    {
        if (equipping && currentlyEquippedCloth.Count == 1)
            modelCloth.SetActive(true, ignoreAutomaticMeshHide);
        else if (!equipping && currentlyEquippedCloth.Count == 0)
            modelCloth.SetActive(false, ignoreAutomaticMeshHide);

        if (!equipping)
            sceneCloth.movedOutOfThresholdAfterUnequip = false;

        // Sets the parent of the scene cloth if the model cloth is active
        sceneCloth.SetActiveAndParent(!equipping, snapOnGrab ? null : modelCloth.transform);
    }

    // Check if the cloth in the scene aligns with the cloth on the model in both position and angle
    public void CheckIfWithinThreshold()
    {
        foreach (InteractableCloth cloth in sceneCloth)
        {
            if (cloth.isBeingGrabbed)
            {
                if (!canEquipMultiple && equipCount > 0)
                    return;

                if (cloth.movedOutOfThresholdAfterUnequip && InThresholdDistance(cloth) && RotationAligned(cloth))
                {
                    HVRHandGrabber grabber = cloth.GetGrabber();

                    if (!manuallyHandleEquip)
                        Equip(grabber, cloth);
                    else
                        OnAttemptedEquip.Invoke(grabber, this, cloth);
                }
                else if (!InThresholdDistance(cloth))
                {
                    cloth.movedOutOfThresholdAfterUnequip = true;
                }
            }
        }
    }

    public void ManuallyEquip(HVRHandGrabber grabber, InteractableCloth sceneCloth)
    {
        if (manuallyHandleEquip)
            Equip(grabber, sceneCloth);
    }

    public void ManuallyUnequip(HVRHandGrabber grabber)
    {
        if (manuallyHandleUnequip)
            Unequip(grabber);
    }

    public void AddSceneCloth(InteractableCloth cloth)
    {
        if (!sceneCloth.Contains(cloth))
        {
            sceneCloth.Add(cloth);
            cloth.SetGrabbableState(snapOnGrab);
            sceneClothColliders.AddRange(cloth.GetComponentsInChildren<Collider>());
            cloth.onSceneClothInteracted.AddListener((x, y) => OnSceneClothInteracted(x, y, cloth));
        }
    }
    public void RemoveSceneCloth(InteractableCloth cloth)
    {
        if (sceneCloth.Contains(cloth))
        {
            sceneCloth.Remove(cloth);

            Collider[] colliders = cloth.GetComponentsInChildren<Collider>();

            foreach (Collider collider in colliders)
                sceneClothColliders.Remove(collider);
        }
    }

    private void Equip(HVRHandGrabber grabber, InteractableCloth sceneCloth)
    {
        currentlyEquippedCloth.Enqueue(sceneCloth);

        ToggleClothVisibility(true, sceneCloth);
        OnEquip.Invoke(grabber);
    }

    private void Unequip(HVRHandGrabber grabber)
    {
        InteractableCloth nextSceneCloth = currentlyEquippedCloth.Dequeue();

        ToggleClothVisibility(false, nextSceneCloth);
        OnUnequip.Invoke(grabber);

        if (!snapOnGrab)
            nextSceneCloth.ManualGrab(grabber);
    }

    // Check if the scene cloth is within the distance threshold
    private bool InThresholdDistance(InteractableCloth sceneCloth)
    {
        float distance = Vector3.Distance(modelCloth.GetPosition(), sceneCloth.GetPosition());

        return distance <= distanceThreshold;
    }

    // Check if the scene cloth is within the rotation threshold
    private bool RotationAligned(InteractableCloth sceneCloth)
    {
        float angle = Quaternion.Angle(modelCloth.GetRotation(), sceneCloth.GetRotation());

        return angle <= angleThreshold;
    }

    // Called when the scene cloth sends an event and snap is enabled, disables scene cloth and enables worn cloth
    private void OnSceneClothInteracted(HVRHandGrabber grabber, HVRGrabbable grabbable, InteractableCloth sceneCloth)
    {
        if (snapOnGrab)
        {
            if (manuallyHandleEquip)
                OnAttemptedEquip.Invoke(grabber, this, sceneCloth);
            else
                Equip(grabber, sceneCloth);
        }
    }

    // Called when the worn cloth sends an event, disables worn cloth and enables scene cloth
    private void OnWornClothInteracted(HVRHandGrabber grabber, HVRGrabbable grabbable)
    {
        if (manuallyHandleUnequip)
            OnAttemptedUnequip.Invoke(grabber, this);
        else
            Unequip(grabber);
    }
}