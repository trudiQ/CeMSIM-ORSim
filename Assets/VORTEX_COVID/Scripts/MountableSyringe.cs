using HurricaneVR.Framework.Core;
using UnityEngine;

public class MountableSyringe : MonoBehaviour {

	public Vector3 desiredPosition;
	public Vector3 desiredRotation;
	public HVRGrabbable grabbable;
	public SyringeMountManager manager;
	public float breakForce;

	[HideInInspector]
	public bool mounted;

	private FixedJoint joint;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (mounted) {
			joint = GetComponent<FixedJoint>();
			mounted = false;
		}
		if (joint) {
			Vector3 projection = Vector3.Project(joint.currentForce, transform.forward);
			if (projection.magnitude > breakForce) {
				Debug.Log(projection.magnitude);
				Destroy(joint);
				OnJointBreak(projection.magnitude);
			}
		}
	}

	void OnJointBreak(float breakForce) {
		Invoke(nameof(CallDelayed), 1f);
		manager.Regrab();
	}

	private void CallDelayed() {
		SyringeMountManager.isAttached = false;
	}

}
