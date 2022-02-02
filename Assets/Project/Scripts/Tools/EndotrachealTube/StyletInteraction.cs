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
	// Start is called before the first frame update
	void Start() {
		tubeRigidbody = tube.GetComponent<Rigidbody>();
		styletRigidbody = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update() {

	}

	public void OnStyletGrabbed() {
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

	public void OnStyletReleased() {
		if (!isDetached) {
			tube.GetComponent<TubeInteraction>().Regrab();
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
