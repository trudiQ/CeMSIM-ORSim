using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class VRAvatar : MonoBehaviour
{
    public bool headIKActive = true;
    public bool leftHandIKActive = true;
    public bool rightHandIKActive = true;

    public Transform rigRoot;
    public Transform rigHead;
    private float headToRootVerticalDistance;

    private Transform floorTarget;
    private Transform viewCamera;
    private Transform headTarget;
    private Transform leftHandTarget;
    private Transform rightHandTarget;

    private Animator animator;

    void Start()
    {
        headToRootVerticalDistance = rigHead.position.y - rigRoot.position.y;
        animator = GetComponent<Animator>();
    }

    public void SetFloorTargetAndCamera(Transform target, Transform camera)
    {
        floorTarget = target;
        viewCamera = camera;
    }

    public void SetIKTargets(Transform head = null, Transform leftHand = null, Transform rightHand = null)
    {
        headTarget = head;

        leftHandTarget = leftHand;

        rightHandTarget = rightHand;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(animator)
        {
            if (floorTarget && viewCamera)
            {
                animator.bodyPosition = new Vector3(floorTarget.position.x, viewCamera.position.y - headToRootVerticalDistance, floorTarget.position.z);
                animator.bodyRotation = floorTarget.rotation;
            }

            if (headIKActive && headTarget)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(headTarget.position + headTarget.forward);

                Debug.Log(animator.bodyPosition);
            }
            else
            {
                animator.SetLookAtWeight(0);
            }

            if (rightHandIKActive && rightHandTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }

            if (leftHandIKActive && leftHandTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }

            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
        }
    }
}
