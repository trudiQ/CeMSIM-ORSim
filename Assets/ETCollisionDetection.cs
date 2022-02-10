using UnityEngine;

public class ETCollisionDetection : MonoBehaviour {
	public Transform anchor;
	public Vector3 jawDesiredPos;
	public Vector3 jawDesiredRot;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void SetJawTransform() {
		transform.localPosition = jawDesiredPos;
		transform.localEulerAngles = jawDesiredRot;
	}

	private void OnTriggerEnter(Collider other) {
		TubeInteraction tubeInteraction = other.GetComponent<TubeInteraction>();
		if (tubeInteraction) {
			SetJawTransform();
			GetComponentInChildren<TongueBladeInteractions>().isETInserted = true;
			other.transform.parent = this.transform;
			tubeInteraction.StartInsertionAnimation(anchor);
		}
	}
}
