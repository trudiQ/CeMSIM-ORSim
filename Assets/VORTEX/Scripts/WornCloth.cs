using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(Interactable))]
public class WornCloth : MonoBehaviour
{
    // This script needs to be placed on an object parented to the bone that the cloth with the mesh renderer is skinned to
    // It has to be different from the skinned object since its position doesn't move, only the vertices
    // The below GameObject contains the SkinnedMeshRenderer and is enabled/disabled to show/hide
    public GameObject objectWithSkinnedMesh;
    public Vector3 offset; // Position offset from the object's origin to the center of the mesh
    public bool isActive { get; private set; }

    public delegate void OnWornClothInteractedSteam(Hand hand);
    public delegate void OnWornClothInteractedXR(XRBaseInteractor hand);
    public OnWornClothInteractedSteam onWornClothInteractedSteam;
    public OnWornClothInteractedXR onWornClothInteractedXR;

    private XRSimpleInteractable xrInteractable;
    private Interactable steamInteractable;
    private new Rigidbody rigidbody;
    private new Collider collider;

    void Start()
    {
        xrInteractable = GetComponent<XRSimpleInteractable>();
        steamInteractable = GetComponent<Interactable>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponentInChildren<Collider>();
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
        isActive = state;

        xrInteractable.enabled = state;
        steamInteractable.enabled = state;
        rigidbody.detectCollisions = state;
        collider.enabled = state;
    }

    // XR method for when the object is selected
    public void OnSelectEnter(XRBaseInteractor interactor)
    {
        Debug.Log("Worn cloth grabbed");
        onWornClothInteractedXR.Invoke(interactor);
    }

    // XR method for when the object stops being selected
    public void OnSelectExit(XRBaseInteractor interactor)
    {
        Debug.Log("Worn cloth let go");
        onWornClothInteractedXR.Invoke(interactor);
    }

    // Steam method that updates while the hand is within the collider
    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();

        if(hand.AttachedObjects.Count == 0 && startingGrabType == GrabTypes.Pinch)
        {
            onWornClothInteractedSteam.Invoke(hand);
        }
    }

    // Show the offset position as a sphere when the object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(GetOffsetPosition(), 0.01f);
    }
}
