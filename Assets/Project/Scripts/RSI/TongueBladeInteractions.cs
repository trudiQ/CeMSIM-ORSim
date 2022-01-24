using UnityEngine;

public class TongueBladeInteractions : MonoBehaviour {

	public TongueSweepManager manager;

	private bool staying = false;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		CheckTongueBladeStatus();
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.GetComponent<MountableBlade>() != null) {
			staying = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.gameObject.GetComponent<MountableBlade>() != null) {
			staying = false;
		}
	}

	private void CheckTongueBladeStatus() {
		if (staying) {
			manager.SweepTongue();
		} else {
			manager.RestoreTongue();
		}
	}

}
