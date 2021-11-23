using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using System.Collections.Generic;
using UnityEngine;

public class MountManager : MonoBehaviour {

	[HideInInspector]
	public bool isAttached;
	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	void OnTriggerEnter(Collider other) {
		if(isAttached) return;
		Mountable mountable = other.gameObject.GetComponent<Mountable>();
		Debug.Log(mountable);
		if(mountable) {
			Transform otherTransform = other.gameObject.transform;
			otherTransform.parent = transform;
			otherTransform.localPosition = mountable.desiredPosition;
			otherTransform.localRotation = Quaternion.identity;
			otherTransform.SetParent(null, true);

			GameObject otherGameObject = other.gameObject;
			FixedJoint joint = otherGameObject.AddComponent<FixedJoint>();
			joint.breakForce = 150f;
			joint.breakTorque = 150f;
			joint.connectedBody = GetComponent<Rigidbody>();

			mountable.grabbable.ForceRelease();
			isAttached = true;
		}
	}

}
