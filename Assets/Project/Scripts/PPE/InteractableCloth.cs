using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(HVRInteractable))]
[System.Serializable]
public class InteractableCloth : MonoBehaviour
{
    public string clothName; // Name to be matched at start
    public Vector3 offset; // Position offset from the object's origin to the center of the mesh
    public bool isBeingGrabbed = false;
    public bool isActive { get; private set; }

    public UnityEvent<HVRHandGrabber, HVRInteractable> onSceneClothInteracted;

    private HVRHandGrabber connectedHand;
    private HVRInteractable interactable;

    void Start()
    {
        interactable = GetComponent<HVRInteractable>();
    }

    // Returns the position in the world where the offset of the object would be
    public Vector3 GetOffsetPosition()
    {
        return transform.position + transform.rotation * offset;
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
    public void Grabbed(HVRHandGrabber grabber, HVRInteractable interactable)
    {
        connectedHand = grabber;
        isBeingGrabbed = true;

        onSceneClothInteracted.Invoke(grabber, interactable);
    }

    // Removes the reference to the grabbing hand
    public void Released(HVRHandGrabber grabber, HVRInteractable interactable)
    {
        connectedHand = null;
        isBeingGrabbed = false;
    }

    // Manually grab the object
    public void ManualGrab(HVRHandGrabber grabber)
    {
        grabber.TryGrab(interactable);
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
    }

    // Show the offset position as a sphere when the object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(GetOffsetPosition(), 0.01f);
    }
}
