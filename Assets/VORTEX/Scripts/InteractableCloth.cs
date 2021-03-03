using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Throwable))]
public class InteractableCloth : MonoBehaviour
{
    public string clothName;
    public Vector3 offset;
    public bool isBeingGrabbed = false;

    private Hand connectedSteamHand;

    void Start()
    {
        
    }

    public Vector3 GetOffsetPosition()
    {
        return transform.position + transform.rotation * offset;
    }

    public void StopGrab()
    {
        if(connectedSteamHand)
            connectedSteamHand.DetachObject(gameObject);

        isBeingGrabbed = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position + transform.rotation * offset, 0.01f);
    }

    public void OnSelectEnter(XRBaseInteractor interactor)
    {
        isBeingGrabbed = true;
    }

    public void OnSelectExit(XRBaseInteractor interactor)
    {
        isBeingGrabbed = false;
    }

    private void OnAttachedToHand(Hand hand)
    {
        connectedSteamHand = hand;
        isBeingGrabbed = true;
    }

    private void OnDetachedFromHand(Hand hand)
    {
        connectedSteamHand = null;
        isBeingGrabbed = false;
    }
}
