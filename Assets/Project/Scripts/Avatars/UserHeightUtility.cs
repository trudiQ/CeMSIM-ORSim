using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RootMotion.FinalIK;
using HurricaneVR.Framework.Core.Player;

namespace CEMSIM
{
    public class UserHeightUtility : MonoBehaviour
    {
        [Tooltip("The object that is located at the floor level.")]
        public Transform floor;
        [Tooltip("The object that follows the user camera.")]
        public new Transform camera;
        public HVRCameraRig cameraRig;
        public AvatarHeightCalibration avatarHeightCalibration;

        public float height = 1.5f;
        [HideInInspector] public bool calibrated = false;

        public void CalculateUserHeight()
        {
            if (cameraRig)
                height = cameraRig.AdjustedCameraHeight;
            else
                Debug.LogWarning("Camera Rig missing in UserHeightUtility.");


            if (avatarHeightCalibration)
            {
                avatarHeightCalibration.Calibrate();
                calibrated = true;
            }
            else
                Debug.LogWarning("Avatar Height Calibration missing in UserHeightUtility.");
        }
    }

    [CustomEditor(typeof(UserHeightUtility))]
    public class UserHeightUtilityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UserHeightUtility userHeightUtility = target as UserHeightUtility;

            if (Application.isPlaying && userHeightUtility.cameraRig && GUILayout.Button("Calculate Height"))
                (target as UserHeightUtility).CalculateUserHeight();
        }
    }
}
