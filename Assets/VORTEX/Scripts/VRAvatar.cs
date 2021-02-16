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

    public VRMap head;
    public VRMap rightHand;
    public VRMap leftHand;

    [Range(0,1)] public float footIKWeight = 1;
    [Range(0, 1)] public float footRotationWeight = 1;
    public Vector3 footOffset;

    private Animator animator;

    void Start()
    {
        headToRootVerticalDistance = rigHead.position.y - rigRoot.position.y;
        animator = GetComponentInChildren<Animator>();
    }

    public void SetFloorTargetAndCamera(Transform target, Transform camera)
    {
        floorTarget = target;
        viewCamera = camera;
    }

    public void SetIKTargets(Transform _head = null, Transform _leftHand = null, Transform _rightHand = null)
    {
        head.Map(_head);
        leftHand.Map(_leftHand);
        rightHand.Map(_rightHand);
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

            if (headIKActive && head.rigTarget)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(head.rigTarget.position + head.rigTarget.forward);
            }
            else
            {
                animator.SetLookAtWeight(0);
            }

            if (rightHandIKActive && rightHand.rigTarget)
            {
                SetAnimatorIKValues(
                    IKGoal: AvatarIKGoal.RightHand,
                    position: rightHand.rigTarget.position,
                    rotation: rightHand.rigTarget.rotation,
                    weight: 1);
            }
            else
            {
                ResetAnimatorIKWeights(AvatarIKGoal.RightHand);
            }

            if (leftHandIKActive && leftHand.rigTarget)
            {
                SetAnimatorIKValues(
                    IKGoal: AvatarIKGoal.LeftHand,
                    position: leftHand.rigTarget.position,
                    rotation: leftHand.rigTarget.rotation,
                    weight: 1);
            }
            else
            {
                ResetAnimatorIKWeights(AvatarIKGoal.LeftHand);
            }

            Vector3 rightFootPosition = animator.GetIKPosition(AvatarIKGoal.RightFoot);

            if (Physics.Raycast(
                origin: rightFootPosition + Vector3.up,
                direction: Vector3.down,
                maxDistance: int.MaxValue,
                layerMask: LayerMask.GetMask("Ground"),
                hitInfo: out RaycastHit hit))
            {
                Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rigRoot.forward, hit.normal), hit.normal);

                SetAnimatorIKValues(
                    IKGoal: AvatarIKGoal.RightFoot,
                    position: hit.point + footOffset,
                    rotation: footRotation,
                    weight: footIKWeight);
            }
            else
            {
                ResetAnimatorIKWeights(AvatarIKGoal.RightFoot);
            }

            Vector3 leftFootPosition = animator.GetIKPosition(AvatarIKGoal.LeftFoot);

            if (Physics.Raycast(
                origin: leftFootPosition + Vector3.up, 
                direction: Vector3.down,
                maxDistance: int.MaxValue,
                layerMask: LayerMask.GetMask("Ground"), 
                hitInfo: out hit))
            {
                Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rigRoot.forward, hit.normal), hit.normal);

                SetAnimatorIKValues(
                    IKGoal: AvatarIKGoal.LeftFoot,
                    position: hit.point + footOffset,
                    rotation: footRotation,
                    weight: footIKWeight);
            }
            else
            {
                ResetAnimatorIKWeights(AvatarIKGoal.LeftFoot);
            }
        }
    }

    private void SetAnimatorIKValues(AvatarIKGoal IKGoal, Vector3 position, Quaternion rotation, float weight)
    {
        animator.SetIKPositionWeight(IKGoal, weight);
        animator.SetIKRotationWeight(IKGoal, weight);
        animator.SetIKPosition(IKGoal, position);
        animator.SetIKRotation(IKGoal, rotation);
    }

    private void ResetAnimatorIKWeights(AvatarIKGoal IKGoal)
    {
        animator.SetIKPositionWeight(IKGoal, 0);
        animator.SetIKRotationWeight(IKGoal, 0);
    }

    [System.Serializable]
    public class VRMap
    {
        public Transform VRTarget;
        public Transform rigTarget;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        public void Map(Transform _VRTarget)
        {
            VRTarget = _VRTarget;

            rigTarget.SetParent(VRTarget);

            rigTarget.SetPositionAndRotation(
                position: VRTarget.TransformPoint(positionOffset),
                rotation: VRTarget.rotation * Quaternion.Euler(rotationOffset));
        }
    }
}
