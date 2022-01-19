using UnityEngine;

public class BladeMountManager : MonoBehaviour {

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
		MountableBlade mountable = other.gameObject.GetComponent<MountableBlade>();

		if(mountable) {
			Transform otherTransform = other.gameObject.transform;
			otherTransform.parent = transform;
			otherTransform.localPosition = mountable.desiredPosition;
			otherTransform.localRotation = Quaternion.identity;
			otherTransform.SetParent(null, true);

			GameObject otherGameObject = other.gameObject;
			FixedJoint joint = otherGameObject.AddComponent<FixedJoint>();
			joint.connectedBody = GetComponent<Rigidbody>();

			mountable.grabbable.ForceRelease();
			mountable.grabbable.Stationary = true;
			mountable.mounted = true;

			mountable.lightGroup.SetActive(true);

			isAttached = true;
		}
	}

}
