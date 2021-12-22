using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class SyringeMountManager : MonoBehaviour {

	[HideInInspector]
	public static bool isAttached;

	public Rigidbody rb;
	public float angleThreshold;

	private HVRGrabbable grabbable;
	private HVRHandGrabber grabber;

	// Start is called before the first frame update
	void Start() {
		grabbable = GetComponent<HVRGrabbable>();
		grabbable.HandGrabbed.AddListener(OnGrabbed);
	}

	// Update is called once per frame
	void Update() {

	}

	void OnTriggerEnter(Collider other) {
		if (isAttached) return;
		MountableSyringe mountable = other.gameObject.GetComponent<MountableSyringe>();

		if (mountable) {
			float angle = Quaternion.Angle(transform.rotation, other.gameObject.transform.rotation);
			if (angle > angleThreshold) {
				return;
			}
			Transform otherTransform = other.gameObject.transform;
			otherTransform.parent = transform;
			otherTransform.localPosition = mountable.desiredPosition;
			otherTransform.localEulerAngles = mountable.desiredRotation;
			otherTransform.SetParent(null, true);

			GameObject otherGameObject = other.gameObject;
			FixedJoint joint = otherGameObject.AddComponent<FixedJoint>();

			joint.connectedBody = rb;
			rb.isKinematic = true;

			mountable.mounted = true;
			isAttached = true;
		}
	}

	public void Regrab() {
		rb.isKinematic = false;
		grabber.ForceRelease();
		grabber.TryGrab(grabbable, true);
	}

	private void OnGrabbed(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		this.grabber = grabber;
	}
}
