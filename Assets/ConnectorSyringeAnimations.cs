using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class ConnectorSyringeAnimations : MonoBehaviour {
	public TubeInteraction tubeInteraction;

	private HVRInteractable interactable;
	private Rigidbody rb;
	private Animator animator;

	// Start is called before the first frame update
	void Start() {
		interactable = GetComponent<HVRInteractable>();
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		interactable.HandGrabbed.AddListener(OnGrabbed);
		interactable.HandReleased.AddListener(OnReleased);
	}

	// Update is called once per frame
	void Update() {

	}

	private void OnGrabbed(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		if (!tubeInteraction.isEtInserted) {
			grabbable.ForceRelease();
			rb.isKinematic = false;
		} else {
			rb.isKinematic = true;
			animator.enabled = true;
			animator.Play("ETConnectorAnim");
		}
	}

	private void OnReleased(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		if (tubeInteraction.isEtInserted) {
			rb.isKinematic = true;
			GetComponent<Animator>().Play("ETConnectorRestoreAnim");
		} else {
			rb.isKinematic = false;
		}
	}

}
