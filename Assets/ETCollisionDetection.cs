using HurricaneVR.Framework.Core;
using System;
using UnityEngine;

public class ETCollisionDetection : MonoBehaviour {
	public Collider etTriggerCollider;
	public Collider cheekCollider;
	public Transform anchor;
	public TransformHolder jawETInsertionTransform;
	public HVRGrabbable jawGrabbable;
	public GameObject jawGrabPoints;
	public HVRGrabbable headGrabbable;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void SetJawTransform() {
		transform.localPosition = jawETInsertionTransform.position;
		transform.localEulerAngles = jawETInsertionTransform.rotation;
	}

	private void OnTriggerEnter(Collider other) {
		TubeInteraction tubeInteraction = other.GetComponent<TubeInteraction>();
		if (tubeInteraction) {
			DisableComponents();
			SetJawTransform();
			GetComponentInChildren<TongueBladeInteractions>().isETInserted = true;
			other.transform.parent = this.transform;
			tubeInteraction.StartInsertionAnimation(anchor);
		}
	}

	private void DisableComponents() {
		etTriggerCollider.enabled = false;
		cheekCollider.enabled = false;
		jawGrabbable.enabled = false;
		headGrabbable.enabled = false;
		jawGrabPoints.SetActive(false);
	}

	public void ReleaseJaw() {
		jawGrabbable.ForceRelease();
		jawGrabbable.CanBeGrabbed = false;
	}
}
