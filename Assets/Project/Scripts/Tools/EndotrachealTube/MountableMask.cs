using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class MountableMask : MonoBehaviour {
	[HideInInspector]
	public bool mounted;

	public Collider mountableCollider;
	public HVRInteractable grabbable;
	public float breakForce = 150f;
	public VentilatorETConnection manager;

	private FixedJoint joint;
	private bool isGrabbed;

	// Start is called before the first frame update
	void Start() {
		grabbable.Grabbed.AddListener(OnGrabbed);
		grabbable.Released.AddListener(OnReleased);
		if (!manager) {
			manager = FindObjectOfType<VentilatorETConnection>();
		}
	}

	// Update is called once per frame
	void Update() {
		CheckBreakForce();
		CheckMountState();
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
		manager.isAttached = false;
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
