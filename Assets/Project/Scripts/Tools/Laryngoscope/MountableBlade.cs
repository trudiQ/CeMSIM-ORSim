using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class MountableBlade : MonoBehaviour {
	public Vector3 desiredPosition;
	public HVRGrabbable grabbable;
	public BladeMountManager manager;
	public float breakForce = 150f;
	public GameObject lightGroup;

	[HideInInspector]
	public bool mounted;

	private FixedJoint joint;
	private bool isGrabbed;

	// Start is called before the first frame update
	void Start() {
		grabbable.Grabbed.AddListener(OnGrabbed);
		grabbable.Released.AddListener(OnReleased);
		if (!manager) {
			manager = FindObjectOfType<BladeMountManager>();
		}
	}

	// Update is called once per frame
	void Update() {
		CheckMountState();
		CheckBreakForce();
	}

	void OnJointBreak(float breakForce) {
		manager.isAttached = false;
		grabbable.Stationary = false;
		lightGroup.SetActive(false);
	}

	private void CheckMountState() {
		if (mounted) {
			joint = GetComponent<FixedJoint>();
			mounted = false;
		}
	}

	private void CheckBreakForce() {
		if (joint && isGrabbed) {
			Vector3 projection = Vector3.Project(joint.currentForce, transform.right);
			if (projection.magnitude > breakForce) {
				Destroy(joint);
				OnJointBreak(projection.magnitude);
			}
		}
	}
	private void OnGrabbed(HVRGrabberBase grabber, HVRGrabbable grabbable) {
		isGrabbed = true;
	}

	private void OnReleased(HVRGrabberBase grabber, HVRGrabbable grabbable) {
		isGrabbed = false;
	}

}
