using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Shared;

[RequireComponent(typeof(HVRInteractable))]
public class GloveBox : MonoBehaviour
{
    public GameObject leftGlovePrefab;
    public GameObject rightGlovePrefab;

    private HVRInteractable interactable;

    private void Start()
    {
        interactable = GetComponent<HVRInteractable>();

        interactable.Interacted.AddListener(OnInteracted);
    }

    private void OnInteracted(HVRHandGrabber grabber, HVRInteractable interactable)
    {
        if (grabber.HandSide == HVRHandSide.Left)
        {
            GameObject glove = Instantiate(rightGlovePrefab, grabber.transform.position, grabber.transform.rotation);
            HVRInteractable gloveInteractable = glove.GetComponent<HVRInteractable>();
            grabber.TryGrab(gloveInteractable, true);
        }
        else
        {
            GameObject glove = Instantiate(leftGlovePrefab, grabber.transform.position, grabber.transform.rotation);
            HVRInteractable gloveInteractable = glove.GetComponent<HVRInteractable>();
            grabber.TryGrab(gloveInteractable, true);
        }
    }
}
