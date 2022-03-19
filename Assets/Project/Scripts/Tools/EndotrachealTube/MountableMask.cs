using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class MountableMask : MonoBehaviour {
	[HideInInspector]
	public bool mounted;

	public Collider mountableCollider;
	public Collider patientCollider;
	public HVRInteractable grabbable;
	public float breakForce = 150f;
	public VentilatorETConnection manager;
	public TransformHolder ventilatorAttachmentTransform;
	public TransformHolder patientAttachmentTransform;

	private FixedJoint joint;
	private bool isGrabbed;
	private Rigidbody rb;

	// Start is called before the first frame update
	void Start() {
		grabbable.Grabbed.AddListener(OnGrabbed);
		grabbable.Released.AddListener(OnReleased);
		if (!manager) {
			manager = FindObjectOfType<VentilatorETConnection>();
		}
		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update() {
		CheckBreakForce();
		CheckMountState();
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.collider == patientCollider) {
			AttachMaskToPatient(collision.transform);
		}
	}

	public void DetachMaskFromPatient() {
		rb.isKinematic = false;
		Invoke(nameof(SwitchColliders), 1f);
	}

	private void AttachMaskToPatient(Transform patientTransform) {
		transform.parent = patientTransform;
		transform.localPosition = patientAttachmentTransform.position;
		transform.localEulerAngles = patientAttachmentTransform.rotation;
		transform.SetParent(null, true);
		SwitchColliders();
		rb.isKinematic = true;
	}

	private void SwitchColliders() {
		foreach (Collider c in GetComponents<Collider>()) {
			if (c != mountableCollider) {
				c.enabled = !c.enabled;
			}
		}
	}

	private void CheckBreakForce() {
		if (joint && isGrabbed) {
			Vector3 projection = Vector3.Project(joint.currentForce, transform.right);
			if (projection.magnitude > breakForce) {
				Destroy(joint);
				Invoke(nameof(OnJointBreak), 1f);
			}
		}
	}

	void OnJointBreak() {
		manager.isMaskAttached = false;
		grabbable.Stationary = false;
		mountableCollider.enabled = true;
	}

	private void CheckMountState() {
		if (mounted) {
			mountableCollider.enabled = false;
			joint = GetComponent<FixedJoint>();
			mounted = false;
		}
	}

	private void OnGrabbed(HVRGrabberBase grabber, HVRGrabbable grabbable) {
		isGrabbed = true;
	}

	private void OnReleased(HVRGrabberBase grabber, HVRGrabbable grabbable) {
		isGrabbed = false;
	}
}
