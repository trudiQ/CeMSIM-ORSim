using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEditor;

// Sets the scale of the avatar based on the standing height of the user
[RequireComponent(typeof(AvatarPrefabHeightUtility))]
public class AvatarHeightCalibration : MonoBehaviour
{
    public VRIK ik;
    private AvatarPrefabHeightUtility heightUtility;
    private Vector3 startingScale;

    void Start()
    {
        heightUtility = GetComponent<AvatarPrefabHeightUtility>();
        startingScale = ik.references.root.localScale;
    }

    // Must be called at runtime since the user has to be standing to get an accurate height
    public void Calibrate()
    {
        // Get the current height of the head target and resize the avatar
        // New scale is based on the height difference between the current scale and the calibrated scale
        float newHeightScale = (ik.solver.spine.headTarget.position.y - ik.references.root.position.y) / 
                               (heightUtility.height * (ik.references.root.localScale.y / startingScale.y));

        Debug.Log(newHeightScale);

        ik.references.root.localScale *= newHeightScale;
        ik.solver.locomotion.footDistance *= newHeightScale;
        ik.solver.locomotion.stepThreshold *= newHeightScale;
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
