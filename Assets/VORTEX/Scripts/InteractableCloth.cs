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
    private Throwable throwable;

    void Start()
    {
        throwable = GetComponent<Throwable>();
    }

    public Vector3 GetOffsetPosition()
    {
        return transform.position + transform.rotation * offset;
    }

    public void SetActiveAndParent(bool active, Transform parent, Vector3 localOffset)
    {
        if (!active)
        {
            StopGrab();
            transform.parent = parent;
            transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 180f));
            transform.localPosition = Quaternion.Euler(Vector3.left * -90f) * offset + localOffset;
            gameObject.SetActive(active);
        }
        else
        {
            gameObject.SetActive(active);
            transform.parent = null;
        }
    }

    private void StopGrab()
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

    public void ManualAttachToHand(Hand hand)
    {
        hand.AttachObject(gameObject, GrabTypes.Pinch, throwable.attachmentFlags);
        isBeingGrabbed = true;
    }
}
