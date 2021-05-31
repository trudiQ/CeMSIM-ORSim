using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Grabbers;

public class AvatarSwapper : MonoBehaviour
{
    [Header("HVR Rig Components")]
    public HVRHandGrabber leftHandGrabber;
    public Transform leftHandIKTarget;

    public HVRHandGrabber rightHandGrabber;
    public Transform rightHandIKTarget;

    public Transform headIKTarget;

    [Header("Avatars")]
    public int activeAvatar = 0;
    public List<AvatarComponents> avatarComponents = new List<AvatarComponents>();

    void Start()
    {
        SwapAvatar(activeAvatar);
    }

    // Activate the chosen avatar and deactivate all others
    private void SetAvatarVisilbilities(int activeIndex)
    {
        for (int i = 0; i < avatarComponents.Count; i++)
        {
            if (i == activeIndex)
                avatarComponents[i].avatarRoot.gameObject.SetActive(true);
            else
                avatarComponents[i].avatarRoot.gameObject.SetActive(false);
        }
    }

    // Activate the avatar GameObject and swap the components needed in the HVRHandGrabber scripts
    public void SwapAvatar(int index)
    {
        SetAvatarVisilbilities(index);

        var _avatarComponent = avatarComponents[index];

        leftHandGrabber.HandAnimator = _avatarComponent.leftHandAnimator;
        leftHandGrabber.PhysicsPoser = _avatarComponent.leftHandPhysicsPoser;

        rightHandGrabber.HandAnimator = _avatarComponent.rightHandAnimator;
        rightHandGrabber.PhysicsPoser = _avatarComponent.rightHandPhysicsPoser;

        leftHandIKTarget.localPosition = _avatarComponent.leftHandIKTargetLocalPositionOffset;
        leftHandIKTarget.localRotation = Quaternion.Euler(_avatarComponent.leftHandIKTargetLocalRotationOffset);

        rightHandIKTarget.localPosition = _avatarComponent.rightHandIKTargetLocalPositionOffset;
        rightHandIKTarget.localRotation = Quaternion.Euler(_avatarComponent.rightHandIKTargetLocalRotationOffset);

        headIKTarget.localPosition = _avatarComponent.headIKTargetLocalPositionOffset;
        headIKTarget.localRotation = Quaternion.Euler(_avatarComponent.headIKTargetLocalRotationOffset);
    }
}