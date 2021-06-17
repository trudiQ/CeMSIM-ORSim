using System;
using UnityEngine;
using HurricaneVR.Framework.Core.Player;
using HurricaneVR.Framework.Core;

public class AvatarComponents : MonoBehaviour
{
    public HVRPlayerController playerController;
    public Transform rigCamera;

    public void SetManagerComponents(HVRManager manager)
    {
        manager.PlayerController = playerController;
        manager.Camera = rigCamera;
    }
}
