using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InteractableCloth : MonoBehaviour {
	public string clothName; // Name to be matched at start
	[HideInInspector] public bool isBeingGrabbed = false;
	[HideInInspector] public bool movedOutOfThresholdAfterUnequip = false;

	public UnityEvent<HVRHandGrabber, HVRGrabbable> onSceneClothInteracted;

	private HVRHandGrabber connectedHand;
	public List<HVRInteractable> interactables; // Assumed that if there is more than one interactable, first one is left, second is right hand interactable
	public Transform anchor;

	public Rigidbody rb;
	public List<Rigidbody> kinematicRigidbodies;

	void Start() 
	{
		foreach(HVRInteractable interactable in interactables) 
		{
			interactable.HandGrabbed.AddListener(Grabbed);
			interactable.HandReleased.AddListener(Released);
			interactable.Interacted.AddListener(Interacted);
		}
	}

	// Returns the position in the world where the offset of the object would be
	public Vector3 GetPosition() 
	{
		if (anchor) 
			return anchor.position;

		return transform.position;
	}

	// Returns the rotation
	public Quaternion GetRotation() 
	{
		if (anchor) 
			return anchor.rotation;

		return transform.rotation;
	}

	// Sets the active state of the cloth and changes its parent based on the state
	public void SetActiveAndParent(bool state, Transform parent) 
	{
		if (!state) 
		{
			StopGrab();

			foreach(Rigidbody rb in kinematicRigidbodies) 
				rb.isKinematic = true;

			transform.SetParent(parent, true);

		} 
		else 
		{
			foreach(Rigidbody rb in kinematicRigidbodies) 
				rb.isKinematic = false;

			transform.SetParent(null, true);
		}

		gameObject.SetActive(state);
	}

	// Stores the hand grabbing this object
	public void Grabbed(HVRHandGrabber grabber, HVRGrabbable interactable) 
	{
		connectedHand = grabber;
		isBeingGrabbed = true;
		HVRInteractable hvrInteractable = FindHvrInteractable(interactable.IsLeftHandGrabbed);

		if(!hvrInteractable.grabbable)
			onSceneClothInteracted.Invoke(grabber, interactable);
	}

	// Removes the reference to the grabbing hand
	public void Released(HVRHandGrabber grabber, HVRGrabbable interactable) 
	{
		connectedHand = null;
		isBeingGrabbed = false;
	}

	// Just calls the interacted event
	public void Interacted(HVRHandGrabber grabber, HVRInteractable interactable) 
	{
		onSceneClothInteracted.Invoke(grabber, interactable);
	}

	// Manually grab the object
	public void ManualGrab(HVRHandGrabber grabber) 
	{
		grabber.TryGrab(FindHvrInteractable(grabber.HandSide == HurricaneVR.Framework.Shared.HVRHandSide.Left), true);
	}

	// Forces the release of the object from a hand
	public void StopGrab() 
	{
		if(connectedHand) 
			foreach(HVRInteractable interactable in interactables)
				interactable.ForceRelease();

		isBeingGrabbed = false;
	}

	// Enables or disables the object from being grabbed
	public void SetGrabbableState(bool snapOnGrap) 
	{
		if(snapOnGrap) 
		{
			foreach(HVRInteractable interactable in interactables) 
			{
				interactable.grabbable = !snapOnGrap;
				interactable.Stationary = snapOnGrap;
			}

			foreach(Rigidbody rb in kinematicRigidbodies) 
				rb.isKinematic = snapOnGrap;
		}
	}

	public HVRHandGrabber GetGrabber() 
	{
		return connectedHand;
	}

	public HVRInteractable FindHvrInteractable(bool isLeftHand) 
	{
		if(interactables.Count == 1) 
			return interactables[0];
		else 
		{
			if(isLeftHand)
				return interactables[0];
			else
				return interactables[1];
		}
	}

	// Show the offset position as a sphere when the object is selected
	private void OnDrawGizmosSelected() 
	{
		Gizmos.DrawSphere(GetPosition(), 0.01f);
	}
}
