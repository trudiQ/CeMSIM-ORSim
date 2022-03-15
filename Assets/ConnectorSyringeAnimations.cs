using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

public class ConnectorSyringeAnimations : MonoBehaviour {
	public TubeInteraction tubeInteraction;
	public Animator animator;
	public Collider syringeCollider;
	public Animator tapingAnimator;

	private HVRInteractable interactable;
	private Rigidbody rb;
	private bool isSyringeDetached = false;
	private HVRHandGrabber grabber;

	// Start is called before the first frame update
	void Start() {
		interactable = GetComponent<HVRInteractable>();
		rb = GetComponent<Rigidbody>();
		interactable.HandGrabbed.AddListener(OnGrabbed);
		interactable.HandReleased.AddListener(OnReleased);
	}

	// Update is called once per frame
	void Update() { }

	public void DetachSyringe() {
		rb.isKinematic = false;
		isSyringeDetached = true;
		syringeCollider.isTrigger = false;
		transform.SetParent(null);
		interactable.ForceRelease();
		grabber.TryGrab(interactable, true);
		animator.Play("ETConnectorRestoreAnim");
		Invoke(nameof(SecureET), 2f);
	}

	private void SecureET() {
		tapingAnimator.Play("SecureETAnim");
	}

	private void OnGrabbed(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		this.grabber = grabber;
		if (isSyringeDetached) return;
		if (!tubeInteraction.isEtInserted) {
			grabbable.ForceRelease();
		} else {
			animator.enabled = true;
			animator.Play("ETConnectorAnim");
		}
	}

	private void OnReleased(HVRHandGrabber grabber, HVRGrabbable grabbable) {
		if (!isSyringeDetached) {
			if (tubeInteraction.isEtInserted) {
				animator.Play("ETConnectorRestoreAnim");
			}
		} else {
			rb.isKinematic = false;
		}
	}
}