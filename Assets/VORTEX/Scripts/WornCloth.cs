using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(Interactable))]
public class WornCloth : MonoBehaviour
{
    public Vector3 offset;
    // This script needs to be placed on the skeleton bone that contains the cloth to be controlled
    // The below GameObject contains the SkinnedMeshRenderer and is enabled/disabled to show/hide
    public GameObject objectWithSkinnedMesh;
    public bool isActive { get; private set; }

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

    void Update()
    {
        
    }

    public Vector3 GetOffsetPosition()
    {
        return transform.position + transform.rotation * offset;
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

    public void OnSelectEnter(XRBaseInteractor interactor)
    {
        Debug.Log("Worn cloth grabbed");
    }

    public void OnSelectExit(XRBaseInteractor interactor)
    {
        
    }

    private void OnAttachedToHand(Hand hand)
    {
        Debug.Log("Worn cloth grabbed");
    }

    private void OnDetachedFromHand(Hand hand)
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position + transform.rotation * offset, 0.01f);
    }
}
