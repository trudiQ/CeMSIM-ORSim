using HurricaneVR.Framework.Core;
using UnityEngine;

public class Mountable : MonoBehaviour {
	public Vector3 desiredPosition;
	public HVRGrabbable grabbable;
	public MountManager manager;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		
	}

	void OnJointBreak(float breakForce) {
		manager.isAttached = false;
	}

}
