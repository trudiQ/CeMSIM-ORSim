using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class StyletInteraction : MonoBehaviour {
	[HideInInspector]
	public bool isDetached = false;

	public GameObject tube;
	public Transform anchor;
	public Collider collider;
	public bool useMotor = false;

	private Rigidbody tubeRigidbody;
	private Rigidbody styletRigidbody;
	private TubeInteraction tubeInteractions;
	private HVRInteractable interactable;

	// Start is called before the first frame update
	void Start() {
		tubeRigidbody = tube.GetComponent<Rigidbody>();
		styletRigidbody = GetComponent<Rigidbody>();
		tubeInteractions = tube.GetComponent<TubeInteraction>();
		interactable = GetComponent<HVRInteractable>();
		interactable.HandGrabbed.AddListener(OnGrabbed);
		interactable.HandReleased.AddListener(OnReleased);
	}

	// Update is called once per frame
	void Update() {

	}

	private void OnGrabbed(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		if (!tubeInteractions.isGrabbed) {
			grabbable.ForceRelease();
			return;
		}
		if (!isDetached) {
			tubeRigidbody.constraints = RigidbodyConstraints.FreezeAll;
			HingeJoint joint = gameObject.AddComponent<HingeJoint>();
			joint.anchor = anchor.transform.localPosition;
			joint.axis = Vector3.forward;
			if (useMotor) {
				var motor = joint.motor;
				motor.force = 50;
				motor.targetVelocity = -80;
				motor.freeSpin = false;
				joint.motor = motor;
				joint.useMotor = true;
			}
		}
	}

	private void OnReleased(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		if (!isDetached) {
			tubeInteractions.Regrab();
			styletRigidbody.isKinematic = true;
			RemoveJoint();
		} else {
			collider.isTrigger = false;
			styletRigidbody.isKinematic = false;
			transform.SetParent(null, true);
		}
	}

	public void RemoveJoint() {
		Destroy(GetComponent<HingeJoint>());
	}
}
