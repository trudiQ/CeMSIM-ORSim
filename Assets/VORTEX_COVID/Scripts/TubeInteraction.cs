using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class TubeInteraction : MonoBehaviour {
	public Collider tubeIgnoreCollider;
	public Collider styletIgnoreCollider;

	private HVRGrabbable grabbable;
	private HVRHandGrabber grabber;
	// Start is called before the first frame update
	void Start() {
		grabbable = GetComponent<HVRGrabbable>();
		grabbable.HandGrabbed.AddListener(OnGrabbed);

		Physics.IgnoreCollision(tubeIgnoreCollider, styletIgnoreCollider);
	}

	// Update is called once per frame
	void Update() {

	}

	void OnTriggerEnter(Collider other) {
		StyletInteraction styletInteraction = other.gameObject.GetComponent<StyletInteraction>();

		if (styletInteraction == null || styletInteraction.isDetached) return;

		styletInteraction.RemoveJoint();
		styletInteraction.isDetached = true;
		Regrab();
	}

	public void Regrab() {
		grabber.ForceRelease();
		grabber.TryGrab(grabbable, true);
	}

	private void OnGrabbed(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		this.grabber = grabber;
	}

}
