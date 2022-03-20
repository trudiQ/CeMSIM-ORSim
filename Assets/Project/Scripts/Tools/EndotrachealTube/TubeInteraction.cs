using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using System.Collections;
using UnityEngine;

public class TubeInteraction : MonoBehaviour {
	public Collider tubeIgnoreCollider;
	public Collider styletIgnoreCollider;
	public Collider tubeInsertCollider;
	public Collider ventilatorCollider;
	public Transform connector;
	public TransformHolder connectorInsertedTransform;
	public GameObject tape;

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
		ventilatorCollider.enabled = true;
		Regrab();
	}

	public void OnRightPrimaryButtonPressed() {
		if (isGrabbed && isEtInserted) {
			animator.enabled = true;
			animator.Play("ETExtubationAnim");
			tape.SetActive(false);
		}
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

	public void OnAnimationCompleted() {
		if (!isEtInserted) {
			SetConnectorTransform();
			isEtInserted = true;
			foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
				rb.isKinematic = true;
			}
		}
		animator.enabled = false;
	}

	public void OnExtubationCompleted() {
		foreach(Animator anim in GetComponentsInChildren<Animator>()) {
			anim.enabled = false;
		}
		isEtInserted = false;
		StartCoroutine(CompleteExtubation());
	}

	public IEnumerator CompleteExtubation() {
		yield return null;
		rigidBody.constraints = RigidbodyConstraints.None;
		foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = false;
		}

		transform.SetParent(null, true);
		grabber.TryGrab(grabbable, true);
	}

	private void OnGrabbed(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		this.grabber = grabber;
		isGrabbed = true;
	}

	private void OnReleased(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		rigidBody.isKinematic = isEtInserted;
		isGrabbed = false;
	}

}
