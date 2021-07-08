using System;
using UnityEngine;
using HurricaneVR.Framework.Core.Player;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.UI;

public class AvatarComponents : MonoBehaviour
{
    public bool isLocalUser;

    [Header("HVRGlobal Components")]
    public HVRPlayerController playerController;
    public Transform rigCamera;

    [Header("Avatar Components")]
    [Tooltip("Reference to the calibration script that sends the scale change event.")]
    public AvatarHeightCalibration calibration;
    public Transform floor;
    public new Transform camera;
    public HVRUIPointer leftPointer;
    public HVRUIPointer rightPointer;

    public void SetHVRComponents(HVRManager manager, HVRInputModule uiInputModule)
    {
        if (isLocalUser)
        {
            manager.PlayerController = playerController;
            manager.Camera = rigCamera;

            uiInputModule.AddPointer(leftPointer);
            uiInputModule.AddPointer(rightPointer);

            // Need to set grab helper player controller manually since it is empty before the avatar spawns
            GrabHelper[] grabHelpers = FindObjectsOfType<GrabHelper>();

            foreach (GrabHelper helper in grabHelpers)
                helper.player = playerController;
        }
    }
}
