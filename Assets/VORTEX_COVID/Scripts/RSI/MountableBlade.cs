using HurricaneVR.Framework.Core;
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
			Vector3 projection = Vector3.Project(joint.currentForce, transform.right);
			if (projection.magnitude > breakForce) {
				Destroy(joint);
				OnJointBreak(projection.magnitude);
			}
		}

	}

	void OnJointBreak(float breakForce) {
		manager.isAttached = false;
		grabbable.Stationary = false;
		lightGroup.SetActive(false);
	}


}
