using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class TubeInteraction : MonoBehaviour {
	public Collider tubeIgnoreCollider;
	public Collider styletIgnoreCollider;
	public Collider tubeInsertCollider;

	public Vector3 insertionStartPos;
	public Vector3 insertionStartRot;

	private HVRGrabbable grabbable;
	private HVRHandGrabber grabber;

	private Animator animator;

	// Start is called before the first frame update
	void Start() {
		grabbable = GetComponent<HVRGrabbable>();
		grabbable.HandGrabbed.AddListener(OnGrabbed);

		animator = GetComponent<Animator>();

		Physics.IgnoreCollision(tubeIgnoreCollider, styletIgnoreCollider);
	}

	// Update is called once per frame
	void Update() {

	}

	void OnTriggerEnter(Collider other) {
		StyletInteraction styletInteraction = other.gameObject.GetComponent<StyletInteraction>();

		if (styletInteraction == null || styletInteraction.isDetached) return;

		styletInteraction.RemoveJoint();
		styletInteraction.isDetached = true;
		Regrab();
	}

	public void Regrab() {
		grabber.ForceRelease();
		grabber.TryGrab(grabbable, true);
	}

	public void StartInsertionAnimation(Transform anchor) {
		tubeInsertCollider.enabled = false;
		SetTubeTransform(anchor);
		animator.enabled = true;
		animator.Play("ETInsertionAnim");
	}

	private void SetTubeTransform(Transform anchor) {
		transform.localPosition = anchor.localPosition;
		transform.localRotation = anchor.localRotation;
	}

	public void OnInsertionAnimationCompleted() {
		GetComponent<Rigidbody>().isKinematic = true;
	}


	private void OnGrabbed(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		this.grabber = grabber;
	}

}
