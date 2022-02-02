using System.Collections;
using UnityEngine;

public class TongueBladeInteractions : MonoBehaviour {

	public TongueSweepManager manager;
	public bool isETInserted = false;
	public Transform bladeAnchor;
	public ETCollisionDetection etCollisionDetection;

	private bool staying = false;
	private GameObject blade;
	private bool bladeLocked = false;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		CheckTongueBladeStatus();
	}

	public void FreezeBladeOnButtonPress() {
		if (blade && !bladeLocked) {
			bladeLocked = true;
			blade.transform.SetParent(bladeAnchor.parent, true);
			StartCoroutine(LerpTransform(blade.transform, bladeAnchor));
			Rigidbody rb = blade.GetComponent<Rigidbody>();
			rb.isKinematic = !rb.isKinematic;
			manager.SweepTongue(1f);
		} else if (blade && bladeLocked) {
			bladeLocked = false;
			Rigidbody rb = blade.GetComponent<Rigidbody>();
			rb.isKinematic = !rb.isKinematic;
		}
	}

	public IEnumerator LerpTransform(Transform from, Transform to) {
		float timeElapsed = 0f;
		while (timeElapsed <= 2f) {
			from.localPosition = Vector3.Lerp(from.localPosition, to.localPosition, timeElapsed / 2);
			from.localRotation = Quaternion.Lerp(from.localRotation, to.localRotation, timeElapsed / 2);
			from.localScale = Vector3.Lerp(from.localScale, to.localScale, timeElapsed / 2);
			timeElapsed += Time.deltaTime;
			yield return null;
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.GetComponent<MountableBlade>()) {
			blade = other.gameObject;
			staying = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.gameObject.GetComponent<MountableBlade>()) {
			staying = false;
		}
	}

	private void CheckTongueBladeStatus() {
		if (isETInserted) return;

		if (staying) {
			manager.SweepTongue(Time.deltaTime);
		} else {
			manager.RestoreTongue();
		}
	}

}
