using HurricaneVR.Framework.Shared.HandPoser;
using UnityEngine;

public class OxygenMaskBagSqueezeManager : MonoBehaviour {

	public SkinnedMeshRenderer meshRenderer;
	public HVRHandPoser poser;
	public bool isGrabbed { get; set; } = false;
	public bool isPrimaryButtonPressed { get; set; } = false;
	public float speed = 16;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (isGrabbed) {
			if (isPrimaryButtonPressed) {
				float value = Mathf.Clamp(meshRenderer.GetBlendShapeWeight(0) + Time.deltaTime * speed * 100, 0, 100);
				meshRenderer.SetBlendShapeWeight(0, value);
				poser.PrimaryPose.Type = BlendType.BooleanParameter;
			} else {
				float value = Mathf.Clamp(meshRenderer.GetBlendShapeWeight(0) - Time.deltaTime * speed * 100, 0, 100);
				meshRenderer.SetBlendShapeWeight(0, value);
			}
		} else {
			poser.PrimaryPose.Type = BlendType.Immediate;
		}
	}
}
