using System;
using UnityEngine;
using HurricaneVR.Framework.Core.Player;
using HurricaneVR.Framework.Core;

public class AvatarComponents : MonoBehaviour
{
    [Header("HVRGlobal Components")]
    public HVRPlayerController playerController;
    public Transform rigCamera;

    [Header("Networked Components")]
    public Transform leftControllerPosition;
    public Transform rightControllerPosition;
    public Transform hmdPosition;

    [Tooltip("Reference to the calibration script that sends the scale change event.")]
    public AvatarHeightCalibration calibration;

    public void SetManagerComponents(HVRManager manager)
    {
        manager.PlayerController = playerController;
        manager.Camera = rigCamera;
    }
}
