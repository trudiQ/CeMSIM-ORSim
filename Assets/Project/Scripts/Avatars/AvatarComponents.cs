using System;
using UnityEngine;
using HurricaneVR.Framework.Shared.HandPoser;

[Serializable]
public class AvatarComponents
{
    public string name;
    public GameObject avatarRoot; // Base GameObject of the avatar in the scene

    [Header("Head")]
    public Vector3 headIKTargetLocalPositionOffset;
    public Vector3 headIKTargetLocalRotationOffset;

    [Header("Left Hand")]
    public HVRHandAnimator leftHandAnimator;
    public HVRPhysicsPoser leftHandPhysicsPoser;
    public Vector3 leftHandIKTargetLocalPositionOffset;
    public Vector3 leftHandIKTargetLocalRotationOffset;

    [Header("Right Hand")]
    public HVRHandAnimator rightHandAnimator;
    public HVRPhysicsPoser rightHandPhysicsPoser;
    public Vector3 rightHandIKTargetLocalPositionOffset;
    public Vector3 rightHandIKTargetLocalRotationOffset;
}
