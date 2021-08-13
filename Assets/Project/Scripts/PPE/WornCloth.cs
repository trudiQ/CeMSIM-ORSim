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
    public Vector3 offset; // Position offset from the object's origin to the center of the mesh
    public bool isActive { get; private set; }

    // Events that trigger when the user grabs the object
    public VRInteractableEvent onWornClothInteracted;

    private HVRInteractable interactable;

    void Start()
    {
        interactable = GetComponent<HVRInteractable>();

        interactable.Interacted.AddListener(Interacted);
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

    public void SetActive(bool state)
    {
        objectWithSkinnedMesh.SetActive(state);
        gameObject.SetActive(state);
        isActive = state;
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
        Gizmos.DrawSphere(GetOffsetPosition(), 0.01f);
    }
}
