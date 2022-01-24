using UnityEngine;

public class TongueSweepManager : MonoBehaviour {

	private RestorableTransform[] transforms;

	// Start is called before the first frame update
	void Start() {
		transforms = GetComponentsInChildren<RestorableTransform>();
	}

	// Update is called once per frame
	void Update() {

	}

	public void SweepTongue() {
		foreach (RestorableTransform restorableTransform in transforms) {
			LerpTransform(restorableTransform.transform, restorableTransform.desiredTransform, Time.deltaTime);
		}
	}

	public void RestoreTongue() {
		foreach (RestorableTransform restorableTransform in transforms) {
			LerpTransform(restorableTransform.transform, restorableTransform.originalPos, restorableTransform.originalRot, restorableTransform.originalScale, Time.deltaTime * 3);
		}
	}

	private void LerpTransform(Transform from, Transform to, float time) {
		from.localPosition = Vector3.Lerp(from.localPosition, to.localPosition, time);
		from.localRotation = Quaternion.Lerp(from.localRotation, to.localRotation, time);
		from.localScale = Vector3.Lerp(from.localScale, to.localScale, time);
	}

	private void LerpTransform(Transform from, Vector3 pos, Quaternion rot, Vector3 scale, float time) {
		from.localPosition = Vector3.Lerp(from.localPosition, pos, time);
		from.localRotation = Quaternion.Lerp(from.localRotation, rot, time);
		from.localScale = Vector3.Lerp(from.localScale, scale, time);
	}
}
