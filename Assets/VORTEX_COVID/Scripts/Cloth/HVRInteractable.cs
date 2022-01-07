using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;

public class HVRInteractable : HVRGrabbable
{
    [Header("Interactable Options")]
    [Tooltip("A toggle that makes this object either grabbable or interactable.")]
    public bool grabbable = false;
    public VRInteractableEvent Interacted = new VRInteractableEvent();
    public VRGrabberEvent ForceInteracted = new VRGrabberEvent();

    public bool shouldSwitchColliders = false;
    public List<Collider> switchColliders;

    protected override void OnGrabbed(HVRGrabberBase grabber)
    {
        if (grabbable) {
            base.OnGrabbed(grabber);
            SwithCollider(false);
        }
        else if (grabber.IsHandGrabber)
        {
            ForceRelease();
            Interacted.Invoke(grabber as HVRHandGrabber, this);
        }
        else // This is separated in case different actions need to be done to a grabber that isn't a hand grabber
        {
            ForceRelease();
            ForceInteracted.Invoke(grabber, this);
        }
    }

	protected override void OnReleased(HVRGrabberBase grabber) {
		base.OnReleased(grabber);
        SwithCollider(true);
    }

    private void SwithCollider(bool enabled) {
        foreach (Collider col in switchColliders) {
            col.enabled = enabled;
        }
    }
}

[System.Serializable]
public class VRInteractableEvent : UnityEvent<HVRHandGrabber, HVRInteractable> { }