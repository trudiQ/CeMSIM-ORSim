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
    private InteractableClothController clothController;

    private void Start()
    {
        interactable = GetComponent<HVRInteractable>();
        
        interactable.Interacted.AddListener(OnInteracted);
    }

    private void OnInteracted(HVRHandGrabber grabber, HVRInteractable interactable)
    {
        StartCoroutine(SpawnGlove(grabber));
    }

    private IEnumerator SpawnGlove(HVRHandGrabber grabber)
    {
        if (!clothController)
            clothController = FindObjectOfType<InteractableClothController>();

        GameObject glove;

        if (grabber.HandSide == HVRHandSide.Left)
            glove = Instantiate(rightGlovePrefab, grabber.transform.position, grabber.transform.rotation);
        else
            glove = Instantiate(leftGlovePrefab, grabber.transform.position, grabber.transform.rotation);

        InteractableCloth gloveCloth = glove.GetComponent<InteractableCloth>();
        HVRInteractable gloveInteractable = gloveCloth.FindHvrInteractable(grabber.HandSide == HVRHandSide.Left);

        yield return null; // Wait until physics poser Start() is finished
        grabber.TryGrab(gloveInteractable, true);

        if (clothController)
        {
            InteractableCloth cloth;
            cloth = glove.GetComponent<InteractableCloth>();
            clothController.AddSceneCloth(cloth);
        }
    }
}
