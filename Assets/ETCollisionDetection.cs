using UnityEngine;

public class ETCollisionDetection : MonoBehaviour {
	public Collider etTriggerCollider;
	public Collider cheekCollider;

	public Transform anchor;

	public TransformHolder jawETInsertionTransform;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void SetJawTransform() {
		transform.localPosition = jawETInsertionTransform.position;
		transform.localEulerAngles = jawETInsertionTransform.rotation;
	}

	private void OnTriggerEnter(Collider other) {
		TubeInteraction tubeInteraction = other.GetComponent<TubeInteraction>();
		if (tubeInteraction) {
			etTriggerCollider.enabled = false;
			cheekCollider.enabled = false;
			SetJawTransform();
			GetComponentInChildren<TongueBladeInteractions>().isETInserted = true;
			other.transform.parent = this.transform;
			tubeInteraction.StartInsertionAnimation(anchor);
		}
	}
}
