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
	public SkinnedMeshRenderer pilotBalloonRenderer;
	public ConnectorSyringeAnimations connectorSyringeAnimations;

	public bool isGrabbed { get; set; } = false;
	public bool isPrimaryButtonPressed { get; set; } = false;

	private bool shouldAnimate = true;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (isGrabbed && shouldAnimate) {
			if (Vector3.Distance(plunger.transform.localPosition, plungerEndPos) < 0.003) {
				plunger.transform.localPosition = plungerEndPos;
				shouldAnimate = false;
				pilotBalloonRenderer.SetBlendShapeWeight(0, 100f);
				connectorSyringeAnimations.DetachSyringe();
			}

			if (isPrimaryButtonPressed) {
				poser.PrimaryPose.Type = BlendType.BooleanParameter;
				plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerEndPos, Time.deltaTime * speed);
				float shapeValue = Mathf.Clamp(pilotBalloonRenderer.GetBlendShapeWeight(0) + Time.deltaTime * speed * 100, 0, 100);
				pilotBalloonRenderer.SetBlendShapeWeight(0, shapeValue);
			} else {
				plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerStartPos, Time.deltaTime * speed);
			}
		} else {
			poser.PrimaryPose.Type = BlendType.Immediate;
		}
	}

}
