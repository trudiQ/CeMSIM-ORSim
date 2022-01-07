using HurricaneVR.Framework.Shared.HandPoser;
using UnityEngine;

public class OxygenMaskBagSqueezeManager : MonoBehaviour {

	public SkinnedMeshRenderer meshRenderer;
	public HVRHandPoser leftHandPoser;
	public HVRHandPoser rightHandPoser;

	public bool isLeftPrimaryButtonPressed { get; set; } = false;
	public bool isRightPrimaryButtonPressed { get; set; } = false;
	public float speed = 16;

	private int grabCount = 0;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (grabCount != 0) {
			if (isLeftPrimaryButtonPressed) {
				float value = Mathf.Clamp(meshRenderer.GetBlendShapeWeight(0) + Time.deltaTime * speed * 100, 0, 100);
				meshRenderer.SetBlendShapeWeight(0, value);
				leftHandPoser.PrimaryPose.Type = BlendType.BooleanParameter;
			}
			if (isRightPrimaryButtonPressed) {
				float value = Mathf.Clamp(meshRenderer.GetBlendShapeWeight(0) + Time.deltaTime * speed * 100, 0, 100);
				meshRenderer.SetBlendShapeWeight(0, value);
				rightHandPoser.PrimaryPose.Type = BlendType.BooleanParameter;
			}
			if (!isLeftPrimaryButtonPressed && !isRightPrimaryButtonPressed) {
				float value = Mathf.Clamp(meshRenderer.GetBlendShapeWeight(0) - Time.deltaTime * speed * 100, 0, 100);
				meshRenderer.SetBlendShapeWeight(0, value);
			}
		} else {
			meshRenderer.SetBlendShapeWeight(0, 0);
			leftHandPoser.PrimaryPose.Type = BlendType.Immediate;
			rightHandPoser.PrimaryPose.Type = BlendType.Immediate;
		}
	}

	public void Grabbed() => grabCount++;

	public void Released() => grabCount--;

}
