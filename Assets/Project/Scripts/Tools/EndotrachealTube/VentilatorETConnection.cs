using UnityEngine;

public class VentilatorETConnection : MonoBehaviour {
	public TransformHolder connectedTransform;
	public HVRInteractable interactable;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.GetComponent<TubeInteraction>()) {
			foreach (Collider c in GetComponents<Collider>()) {
				c.enabled = false;
			}
			transform.localPosition = connectedTransform.position;
			transform.localEulerAngles = connectedTransform.rotation;
			interactable.ForceRelease();
			interactable.grabbable = false;
		}
	}
}