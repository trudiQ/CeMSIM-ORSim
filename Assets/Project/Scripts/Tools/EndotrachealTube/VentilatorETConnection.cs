using UnityEngine;

public class VentilatorETConnection : MonoBehaviour {
	public TransformHolder connectedTransform;
	public HVRInteractable interactable;

	[HideInInspector]
	public bool isAttached;

	private Rigidbody rb;
	// Start is called before the first frame update
	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update() {

	}

	void OnTriggerEnter(Collider other) {
		MountableMask mountableMask = other.gameObject.GetComponent<MountableMask>();
		TubeInteraction tubeInteraction = other.gameObject.GetComponent<TubeInteraction>();

		if (tubeInteraction) {
			AttachTube(tubeInteraction, other.gameObject.transform);
		} else if (mountableMask && !isAttached) {
			AttachMask(mountableMask, other.gameObject);
		}
	}

	private void AttachMask(MountableMask mountableMask, GameObject otherGameObject) {
		TransformHolder transformHolder = otherGameObject.GetComponent<TransformHolder>();
		Transform otherTransform = otherGameObject.transform;
		otherTransform.parent = transform;
		otherTransform.localPosition = transformHolder.position;
		otherTransform.localEulerAngles = transformHolder.rotation;
		otherTransform.SetParent(null, true);

		FixedJoint joint = otherGameObject.AddComponent<FixedJoint>();
		joint.connectedBody = rb;
		mountableMask.mounted = true;
		mountableMask.grabbable.Stationary = true;
		isAttached = true;
	}

	private void AttachTube(TubeInteraction tubeInteraction, Transform transform) {
		foreach (Collider c in GetComponents<Collider>()) {
			c.enabled = false;
		}

		var parent = transform.parent;
		transform.SetParent(transform);
		transform.localPosition = connectedTransform.position;
		transform.localEulerAngles = connectedTransform.rotation;
		transform.parent = parent;

		interactable.ForceRelease();
		interactable.grabbable = false;

		rb.isKinematic = true;
	}
}