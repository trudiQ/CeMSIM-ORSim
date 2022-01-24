using UnityEngine;

public class RestorableTransform : MonoBehaviour {
	// Start is called before the first frame update

	public Transform desiredTransform;

	public Vector3 originalPos { private set; get; }
	public Quaternion originalRot { private set; get; }
	public Vector3 originalScale { private set; get; }

	void Start() {
		originalPos = transform.localPosition;
		originalRot = transform.localRotation;
		originalScale = transform.localScale;
	}

	// Update is called once per frame
	void Update() {

	}
}
