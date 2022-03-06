using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class TubeInteraction : MonoBehaviour {
	public Collider tubeIgnoreCollider;
	public Collider styletIgnoreCollider;
	public Collider tubeInsertCollider;
	public Transform connector;
	public TransformHolder connectorInsertedTransform;

	private HVRGrabbable grabbable;
	private HVRHandGrabber grabber;

	private Animator animator;
	private Rigidbody rigidBody;
	
	[HideInInspector]
	public bool isEtInserted = false;
	[HideInInspector]
	public bool isGrabbed;

	// Start is called before the first frame update
	void Start() {
		grabbable = GetComponent<HVRGrabbable>();
		grabbable.HandGrabbed.AddListener(OnGrabbed);
		grabbable.HandReleased.AddListener(OnReleased);

		animator = GetComponent<Animator>();
		rigidBody = GetComponent<Rigidbody>();

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
		if (grabber) {
			grabber.ForceRelease();
			grabber.TryGrab(grabbable, true);
		}	
	}

	public void StartInsertionAnimation(Transform etInsertionStartTransform) {
		tubeInsertCollider.enabled = false;
		SetTubeTransform(etInsertionStartTransform);
		animator.enabled = true;
		animator.Play("ETInsertionAnim");
	}

	private void SetTubeTransform(Transform etInsertionStartTransform) {
		transform.localPosition = etInsertionStartTransform.localPosition;
		transform.localRotation = etInsertionStartTransform.localRotation;
	}

	private void SetConnectorTransform() {
		connector.localPosition = connectorInsertedTransform.position;
		connector.localEulerAngles = connectorInsertedTransform.rotation;
	}

	public void OnInsertionAnimationCompleted() {
		SetConnectorTransform();
		isEtInserted = true;
		foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = true;
		}
		animator.enabled = false;
	}


	private void OnGrabbed(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		this.grabber = grabber;
		isGrabbed = true;
	}

	private void OnReleased(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		if (isEtInserted) {
			rigidBody.isKinematic = true;
		}
		isGrabbed = false;
	}

}
