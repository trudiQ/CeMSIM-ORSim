using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core;

[RequireComponent(typeof(HVRInteractable))]
[System.Serializable]
public class InteractableCloth : MonoBehaviour
{
    public string clothName; // Name to be matched at start
    [HideInInspector] public bool isBeingGrabbed = false;
    public bool isActive { get; private set; }

    public UnityEvent<HVRHandGrabber, HVRGrabbable> onSceneClothInteracted;

    private HVRHandGrabber connectedHand;
    private HVRInteractable interactable;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        interactable = GetComponent<HVRInteractable>();

        interactable.HandGrabbed.AddListener(Grabbed);
        interactable.HandReleased.AddListener(Released);
        interactable.Interacted.AddListener(Interacted);
    }

    // Returns the position in the world where the offset of the object would be
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    // Returns the rotation
    public Quaternion GetRotation()
    {
        return transform.rotation;
    }

    // Sets the active state of the cloth and changes its parent based on the state
    public void SetActiveAndParent(bool state, Transform parent)
    {
        if (!state)
        {
            StopGrab();

            if (parent)
            {
                transform.parent = parent;
                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;
            }
        }
        else
            transform.parent = null;

        gameObject.SetActive(state);
        isActive = state;
    }

    // Stores the hand grabbing this object
    public void Grabbed(HVRHandGrabber grabber, HVRGrabbable interactable)
    {
        connectedHand = grabber;
        isBeingGrabbed = true;

        onSceneClothInteracted.Invoke(grabber, interactable);
    }

    // Removes the reference to the grabbing hand
    public void Released(HVRHandGrabber grabber, HVRGrabbable interactable)
    {
        connectedHand = null;
        isBeingGrabbed = false;
    }

    // Just calls the interacted event
    public void Interacted(HVRHandGrabber grabber, HVRInteractable interactable)
    {
        onSceneClothInteracted.Invoke(grabber, interactable);
    }

    // Manually grab the object
    public void ManualGrab(HVRHandGrabber grabber)
    {
        grabber.TryGrab(interactable, true);
    }

    // Forces the release of the object from a hand
    public void StopGrab()
    {
        if (connectedHand)
            interactable.ForceRelease();

        isBeingGrabbed = false;
    }

    // Enables or disables the object from being grabbed
    public void SetGrabbableState(bool state)
    {
        interactable.grabbable = state;
        interactable.Stationary = !state;

        if (rb)
            rb.isKinematic = !state;
    }

    public HVRHandGrabber GetGrabber()
    {
        return connectedHand;
    }

    // Show the offset position as a sphere when the object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(GetPosition(), 0.01f);
    }
}
