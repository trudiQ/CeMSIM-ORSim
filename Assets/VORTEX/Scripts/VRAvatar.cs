using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VRAvatar : MonoBehaviour
{
    // Variables to turn IK on or off
    public bool headIKActive = true;
    public bool leftHandIKActive = false;
    public bool rightHandIKActive = false;

    // Head to hip distance variables
    public Transform rigRoot;
    public Transform rigHead;
    private float headToRootVerticalDistance;

    // Camera and body position variables
    public Transform floorTarget { get; private set; }
    private Transform viewCamera;
    public Vector3 bodyOffset;

    // VR input IK object containers
    public VRMap head;
    public VRMap rightHand;
    public VRMap leftHand;

    // Foot IK variables
    [Range(0, 1)] public float footIKWeight = 1;
    [Range(0, 1)] public float footRotationWeight = 1;
    public Vector3 footOffset;

    private Animator animator;

    void Start()
    {
        // Get the vertical distance from head to hip.
        headToRootVerticalDistance = rigHead.position.y - rigRoot.position.y;

        animator = GetComponentInChildren<Animator>();
    }

    // Set the camera that is in use and the floor position object of the camera.
    public void SetFloorTargetAndCamera(Transform target, Transform camera)
    {
        floorTarget = target;
        viewCamera = camera;
    }

    // Map the head, left hand, and right hand IK targets to the parameter transforms, deactivate them if null
    public void SetIKTargets(Transform _head = null, Transform _leftHand = null, Transform _rightHand = null)
    {
        if (_head)
        {
            headIKActive = true;
            head.Map(_head);
        }
        else headIKActive = false;

        if (_leftHand)
        {
            leftHandIKActive = true;
            leftHand.Map(_leftHand);
        }
        else leftHandIKActive = false;

        if (_rightHand)
        {
            rightHandIKActive = true;
            rightHand.Map(_rightHand);
        }
        else rightHandIKActive = false;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(animator)
        {
            // Set the hip position to be a set distance below the camera, based on head to hip distance
            // Rotate the hip rotation to face the camera forward
            if (floorTarget && viewCamera)
            {
                animator.bodyPosition = new Vector3(floorTarget.position.x, viewCamera.position.y - headToRootVerticalDistance, floorTarget.position.z) + bodyOffset;
                animator.bodyRotation = floorTarget.rotation;
            }

            // Set the head lookat direction
            if (headIKActive && head.rigTarget)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(head.rigTarget.position + head.rigTarget.forward);
            }
            else
            {
                animator.SetLookAtWeight(0);
            }

            // Set the right hand IK position and rotation
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

            // Set the left hand IK position and rotation
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

            // Set the right foot IK position to be flat on the floor according to where the animation moves it
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

            // Set the left foot IK position to be flat on the floor according to where the animation moves it
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

    // Set the weights, position, and rotation of an IKGoal
    private void SetAnimatorIKValues(AvatarIKGoal IKGoal, Vector3 position, Quaternion rotation, float weight)
    {
        animator.SetIKPositionWeight(IKGoal, weight);
        animator.SetIKRotationWeight(IKGoal, weight);
        animator.SetIKPosition(IKGoal, position);
        animator.SetIKRotation(IKGoal, rotation);
    }

    // Set the weights of an IKGoal to 0
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

        // Set the rig target to be a child of the VR target, and reset its position/rotation
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
