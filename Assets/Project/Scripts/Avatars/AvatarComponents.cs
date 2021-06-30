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

    public void SetManagerComponents(HVRManager manager)
    {
        manager.PlayerController = playerController;
        manager.Camera = rigCamera;
    }
}
