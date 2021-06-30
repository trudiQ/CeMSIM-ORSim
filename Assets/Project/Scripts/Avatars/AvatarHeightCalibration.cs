using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEditor;
using UnityEngine.Events;

// Sets the scale of the avatar based on the standing height of the user
[RequireComponent(typeof(AvatarPrefabHeightUtility))]
public class AvatarHeightCalibration : MonoBehaviour
{
    public VRIK ik;
    private AvatarPrefabHeightUtility heightUtility;
    private Vector3 startingScale;
    private float startingFootDistance;
    private float startingStepThreshold;
    private float currentScaleMultiplier = 1;

    public UnityEvent<float> OnAvatarHeightChanged; // Sends new avatar scale, foot distance, and step threshold 
                                                    // as a single float when calibrated. Made to send client data to server

    void Start()
    {
        heightUtility = GetComponent<AvatarPrefabHeightUtility>();
        startingScale = ik.references.root.localScale;
        startingFootDistance = ik.solver.locomotion.footDistance;
        startingStepThreshold = ik.solver.locomotion.stepThreshold;
    }

    // Must be called at runtime since the user has to be standing to get an accurate height
    public void Calibrate()
    {
        // Get the current height of the head target and resize the avatar
        // New scale is based on the height difference between the current scale and the calibrated scale
        float newHeightScale = (ik.solver.spine.headTarget.position.y - ik.references.root.position.y) / 
                               (heightUtility.height * (ik.references.root.localScale.y / startingScale.y));

        currentScaleMultiplier *= newHeightScale;
        Calibrate(currentScaleMultiplier);

        OnAvatarHeightChanged.Invoke(currentScaleMultiplier);
    }

    // Use a given height scale to resize the avatar, used for resizing avatar on server/non-local client
    public void Calibrate(float scaleMultiplier)
    {
        currentScaleMultiplier = scaleMultiplier;

        ik.references.root.localScale = startingScale * currentScaleMultiplier;
        ik.solver.locomotion.footDistance = startingFootDistance * currentScaleMultiplier;
        ik.solver.locomotion.stepThreshold = startingStepThreshold * currentScaleMultiplier;
    }
}

[CustomEditor(typeof(AvatarHeightCalibration))]
public class AvatarHeightCalibrationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (Application.isPlaying && GUILayout.Button("Calibrate"))
            (target as AvatarHeightCalibration).Calibrate();
    }
}
