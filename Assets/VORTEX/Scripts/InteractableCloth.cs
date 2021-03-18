using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Throwable))]
[RequireComponent(typeof(Collider))]
public class InteractableCloth : MonoBehaviour
{
    public string clothName; // Name to be matched at start
    public Vector3 offset; // Position offset from the object's origin to the center of the mesh
    public bool isBeingGrabbed = false;

    private Hand connectedSteamHand; // Steam hand
    private Hand.AttachmentFlags attachmentFlags; // Flags that determine what to do when attaching the object to a hand

    void Start()
    {
        // Store the attachment flags from the Steam throwable
        attachmentFlags = GetComponent<Throwable>().attachmentFlags;
    }

    // Returns the position in the world where the offset of the object would be
    public Vector3 GetOffsetPosition()
    {
        return transform.position + transform.rotation * offset;
    }

    public Quaternion GetRotation()
    {
        return transform.rotation;
    }

    // Sets the active state of the cloth and changes its parent based on the state
    public void SetActiveAndParent(bool active, Transform parent)
    {
        if (!active)
        {
            StopGrab();
            transform.parent = parent;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(active);
        }
        else
        {
            gameObject.SetActive(active);
            transform.parent = null;
        }
    }

    // Forces the release of the object from a hand
    private void StopGrab()
    {
        if(connectedSteamHand)
            connectedSteamHand.DetachObject(gameObject);

        isBeingGrabbed = false;
    }

    // XR method for when the object is selected
    public void OnSelectEnter(XRBaseInteractor interactor)
    {
        isBeingGrabbed = true;
    }

    // XR method for when the object stops being selected
    public void OnSelectExit(XRBaseInteractor interactor)
    {
        isBeingGrabbed = false;
    }

    // Steam method for when an object is grabbed
    private void OnAttachedToHand(Hand hand)
    {
        connectedSteamHand = hand;
        isBeingGrabbed = true;
    }

    // Steam method for when an object is released
    private void OnDetachedFromHand(Hand hand)
    {
        connectedSteamHand = null;
        isBeingGrabbed = false;
    }

    // Manually attach an object to the Steam hand
    public void ManualAttachToHandSteam(Hand hand)
    {
        hand.AttachObject(gameObject, GrabTypes.Pinch, attachmentFlags);
    }

    // Show the offset position as a sphere when the object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(GetOffsetPosition(), 0.01f);
    }
}
