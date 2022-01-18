using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.Events;
using HurricaneVR.Framework.Core.Player;

// Sets the scale of the avatar based on the standing height of the user
[RequireComponent(typeof(AvatarPrefabHeightUtility))]
public class AvatarHeightCalibration : MonoBehaviour
{
    public VRIK ik;
    public AvatarPrefabHeightUtility avatarHeightUtility;
    public HVRCameraRig cameraRig;

    [Tooltip("A multiplier to the scale of the avatar. Used if the avatar is too short or tall after calibration.")]
    [Range(0.8f, 1.2f)] public float heightAdjustmentMultiplier = 1f;

    [HideInInspector] public UserHeightUtility userHeightUtility;
    private float calibrationScaleMultiplier = 1;

    public UnityEvent<float> onAvatarHeightChanged; // Sends new avatar scale, foot distance, and step threshold 
                                                    // as a single multiplier when calibrated. Made to send client data to server

    void Start()
    {
        if (!avatarHeightUtility)
            avatarHeightUtility = GetComponentInChildren<AvatarPrefabHeightUtility>();

        if (avatarHeightUtility)
            Calibrate();
    }

    public void Calibrate()
    {
        if (userHeightUtility && avatarHeightUtility)
        {
            // New height is based on the height difference between the user height and avatar height
            float heightDifference = avatarHeightUtility.height * heightAdjustmentMultiplier - userHeightUtility.height;

            Calibrate(heightDifference);

            onAvatarHeightChanged.Invoke(heightDifference);
        }
    }

    // Use a given height scale to resize the avatar, used for resizing avatar on server/non-local client
    public void Calibrate(float heightDifference)
    {
        cameraRig.CameraYOffset = heightDifference;
    }
}