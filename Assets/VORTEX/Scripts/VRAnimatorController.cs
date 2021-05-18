using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRAnimatorController : MonoBehaviour
{
    // Avatar classes and target
    private Animator animator;
    private VRAvatar vrAvatar;
    private Transform vrTarget;

    // Position and rotation variables
    private Vector3 previousPosition;
    private Quaternion previousRotation;

    // Thresholds that control the minimum value needed for animations to trigger
    public float movementSpeedThreshold = 0.1f;
    // Rotation in degrees per second
    public float rotationSpeedThreshold = 5.0f;

    IEnumerator Start()
    {
        animator = GetComponent<Animator>();
        vrAvatar = GetComponent<VRAvatar>();

        // Wait until a target exists
        yield return new WaitUntil(() => vrAvatar.floorTarget);

        // Initialize values
        vrTarget = vrAvatar.floorTarget;
        previousPosition = vrTarget.position;
        previousRotation = vrTarget.rotation;
    }

    void LateUpdate()
    {
        if (vrTarget)
        {
            // Get the head's local movement speed in units per second
            Vector3 hmdMovementSpeed = (vrTarget.position - previousPosition) / Time.deltaTime;
            hmdMovementSpeed.y = 0;
            Vector3 hmdLocalMovementSpeed = transform.InverseTransformDirection(hmdMovementSpeed);
            previousPosition = vrTarget.position;

            // Get the head's rotation speed in degrees per second
            float hmdRotationSpeed = Mathf.DeltaAngle(previousRotation.eulerAngles.y, vrTarget.rotation.eulerAngles.y) / Time.deltaTime;
            previousRotation = vrTarget.rotation;

            // Apply values to animator parameters
            animator.SetBool("isMoving", hmdLocalMovementSpeed.magnitude > movementSpeedThreshold);
            animator.SetFloat("moveX", Mathf.Clamp(hmdLocalMovementSpeed.x, -1, 1), 0.1f, Time.deltaTime);
            animator.SetFloat("moveY", Mathf.Clamp(hmdLocalMovementSpeed.z, -1, 1), 0.1f, Time.deltaTime);

            animator.SetBool("isRotating", Mathf.Abs(hmdRotationSpeed) > rotationSpeedThreshold);
            // Divide rotation speed by quarter rotations since the animations take approximately 1 second
            animator.SetFloat("Rotation", Mathf.Clamp(hmdRotationSpeed / 90f, -1, 1), 0.1f, Time.deltaTime);
        }
    }
}
