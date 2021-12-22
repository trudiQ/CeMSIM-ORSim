using HurricaneVR.Framework.Shared.HandPoser;
using UnityEngine;

public class SyringeInteractions : MonoBehaviour {

	public GameObject plunger;
	public Vector3 plungerStartPos;
	public Vector3 plungerEndPos;
	public float speed = 16;
	public HVRHandPoser poser;
	public Transform balloon;
	public Vector3 balloonInflatedSize;

	public bool isGrabbed { get; set; } = false;
	public bool isPrimaryButtonPressed { get; set; } = false;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (isGrabbed) {
			if (isPrimaryButtonPressed) {
				CheckBalloonStatus();
				poser.PrimaryPose.Type = BlendType.BooleanParameter;
				plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerEndPos, Time.deltaTime * speed);
			} else {
				plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerStartPos, Time.deltaTime * speed);
			}
		} else {
			poser.PrimaryPose.Type = BlendType.Immediate;
		}
	}

	private void CheckBalloonStatus() {
		if (SyringeMountManager.isAttached) {
			balloon.localScale = Vector3.Lerp(balloon.localScale, balloonInflatedSize, Time.deltaTime * speed);
		}
	}
}
