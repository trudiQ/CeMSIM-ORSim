using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(ToggleThrowable))]
[RequireComponent(typeof(Collider))]
[System.Serializable]
public class InteractableCloth : MonoBehaviour
{
    public string clothName; // Name to be matched at start
    public Vector3 offset; // Position offset from the object's origin to the center of the mesh
    public bool isBeingGrabbed = false;
    public bool isActive { get; private set; }

    public delegate void OnSceneClothInteractedSteam(Hand hand);
    public delegate void OnSceneClothInteractedXR(XRBaseInteractor hand);
    public OnSceneClothInteractedSteam onSceneClothInteractedSteam;
    public OnSceneClothInteractedXR onSceneClothInteractedXR;

    private Hand connectedSteamHand; // Steam hand
    public ToggleThrowable throwable { get; private set; }

    void Start()
    {
        throwable = GetComponent<ToggleThrowable>();
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

    // Steam method that updates while the hand is within the collider
    private void HandHoverUpdate(Hand hand)
    {
        if (!isActive) return;

        GrabTypes startingGrabType = hand.GetGrabStarting();

        if (hand.AttachedObjects.Count == 0 && startingGrabType == GrabTypes.Pinch)
        {
            connectedSteamHand = hand;
            onSceneClothInteractedSteam.Invoke(hand);
        }
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
        hand.AttachObject(gameObject, GrabTypes.Pinch, throwable.attachmentFlags);
    }

    // Show the offset position as a sphere when the object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(GetOffsetPosition(), 0.01f);
    }
}
