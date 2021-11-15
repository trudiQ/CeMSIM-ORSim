using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core;

[RequireComponent(typeof(HVRInteractable))]
[System.Serializable]
public class WornCloth : MonoBehaviour
{
    // This script needs to be placed on an object parented to the bone that the cloth with the mesh renderer is skinned to
    // It has to be different from the skinned object since its position doesn't move, only the vertices
    // The below GameObject contains the SkinnedMeshRenderer and is enabled/disabled to show/hide
    public GameObject objectWithSkinnedMesh;
    public bool isActive { get; private set; } = false;

    // Events that trigger when the user grabs the object
    public VRInteractableEvent onWornClothInteracted;

    public Transform anchor;

    private HVRInteractable interactable;

    void Start()
    {
        interactable = GetComponent<HVRInteractable>();

        interactable.Interacted.AddListener(Interacted);
    }

    // Returns the position in the world where the offset of the object would be
    public Vector3 GetPosition()
    {
		if(anchor) {
            return anchor.position;
		}
        return transform.position;
    }

    public Quaternion GetRotation()
    {
		if(anchor) {
            return anchor.rotation;
		}
        return transform.rotation;
    }

    public void SetActive(bool state, bool ignoreMeshHide)
    {
        if(objectWithSkinnedMesh && !ignoreMeshHide)
            objectWithSkinnedMesh.SetActive(state);

        gameObject.SetActive(state);
        isActive = state;
    }

    public void SetMeshVisible(bool state)
    {
        objectWithSkinnedMesh.SetActive(state);
    }

    // Calls the event when this object is interacted with
    public void Interacted(HVRHandGrabber grabber, HVRInteractable grabbable)
    {
        if (isActive)
        {
            onWornClothInteracted.Invoke(grabber, grabbable);
        }
    }

    // Show the offset position as a sphere when the object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(GetPosition(), 0.01f);
    }
}
