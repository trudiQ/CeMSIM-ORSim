using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;

public class HVRInteractable : HVRGrabbable
{
    [Header("Interactable Options")]
    [Tooltip("A toggle that makes this object either grabbable or interactable.")]
    public bool grabbable = false;

    protected override void OnGrabbed(HVRGrabberBase grabber)
    {
        if (grabbable)
            base.OnGrabbed(grabber);
        else if (grabber.IsHandGrabber)
        {
            HandGrabbed.Invoke(grabber as HVRHandGrabber, this);
            ForceRelease();
        }
    }
}
