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

    protected override void OnGrabbed(HVRGrabberBase grabber)
    {
        if (grabbable)
            base.OnGrabbed(grabber);
        else if (grabber.IsHandGrabber)
        {
            ForceRelease();
            Interacted.Invoke(grabber as HVRHandGrabber, this);
        }
        else // This is separated in case different actions need to be done to a grabber that isn't a hand grabber
        {
            ForceRelease();
            Interacted.Invoke(grabber as HVRHandGrabber, this);
        }
    }
}

[System.Serializable]
public class VRInteractableEvent : UnityEvent<HVRHandGrabber, HVRInteractable> { }