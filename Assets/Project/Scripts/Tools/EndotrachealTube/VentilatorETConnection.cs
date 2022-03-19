using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class VentilatorETConnection : MonoBehaviour {
	public TransformHolder connectedTransform;
	public HVRInteractable interactable;

	[HideInInspector]
	public bool isMaskAttached;

	private Rigidbody rb;
	private bool isGrabbed;
	private MountableMask mask;

	// Start is called before the first frame update
	void Start() {
		rb = GetComponent<Rigidbody>();
		interactable.Grabbed.AddListener(OnGrabbed);
		interactable.Released.AddListener(OnReleased);
	}

	// Update is called once per frame
	void Update() {

	}

	void OnTriggerEnter(Collider other) {
		MountableMask mountableMask = other.gameObject.GetComponent<MountableMask>();
		TubeInteraction tubeInteraction = other.gameObject.GetComponent<TubeInteraction>();

		if (tubeInteraction) {
			AttachTube(tubeInteraction, other.gameObject.transform);
		} else if (mountableMask && !isMaskAttached) {
			AttachMask(mountableMask, other.gameObject);
		}
	}

	public void OnLeftPrimaryButtonPressed() {
		if(isMaskAttached && isGrabbed && mask) {
			mask.DetachMaskFromPatient();
		}
	}

	private void AttachMask(MountableMask mountableMask, GameObject otherGameObject) {
		TransformHolder transformHolder = mountableMask.ventilatorAttachmentTransform;
		Transform otherTransform = otherGameObject.transform;
		otherTransform.parent = transform;
		otherTransform.localPosition = transformHolder.position;
		otherTransform.localEulerAngles = transformHolder.rotation;
		otherTransform.SetParent(null, true);

		FixedJoint joint = otherGameObject.AddComponent<FixedJoint>();
		joint.connectedBody = rb;
		mountableMask.mounted = true;
		mountableMask.grabbable.Stationary = true;
		isMaskAttached = true;
		mask = mountableMask;
	}

	private void AttachTube(TubeInteraction tubeInteraction, Transform tubeTransform) {
		foreach (Collider c in GetComponents<Collider>()) {
			c.enabled = false;
		}

		var parent = transform.parent;
		transform.SetParent(tubeTransform);
		transform.localPosition = connectedTransform.position;
		transform.localEulerAngles = connectedTransform.rotation;
		transform.parent = parent;

		interactable.ForceRelease();
		interactable.grabbable = false;

		rb.isKinematic = true;
	}

	private void OnGrabbed(HVRGrabberBase grabber, HVRGrabbable grabbable) {
		isGrabbed = true;
	}

	private void OnReleased(HVRGrabberBase grabber, HVRGrabbable grabbable) {
		isGrabbed = false;
	}
}