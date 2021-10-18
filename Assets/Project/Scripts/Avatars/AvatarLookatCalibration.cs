using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AvatarLookatCalibration : MonoBehaviour
{
    public UserHeightUtility heightUtility;
    public RoleMenu menu;
    public GameObject focusReticlePrefab;
    public UnityEngine.UI.Button calibrateButton;
    public float reticleDistance = 2f;
    public float focusThresholdAngle = 2f;
    public float timerDuration = 3f;

    private Transform focusReticle;
    private ReticleInstruction reticleInstruction;
    private bool calibrationStarted = false;
    private bool calibrationFinished = false;
    private bool timerStarted = false;
    private Coroutine timer;

    void LateUpdate()
    {
        if (calibrationStarted)
        {
            // Move the reticle directly in front of the camera but on a horizontal plane aligned with its y position
            Vector3 horizontalForward = Vector3.ProjectOnPlane(heightUtility.camera.forward, Vector3.up).normalized;
            focusReticle.position = heightUtility.camera.transform.position + horizontalForward * reticleDistance;

            focusReticle.LookAt(heightUtility.camera.transform); // Aim the reticle at the camera
            
            if (IsReticleInFocus() && !timerStarted) // Start the timer if the reticle is in focus
            {
                reticleInstruction.ToggleFocus();
                timer = StartCoroutine(Timer());
            }
            else if (!IsReticleInFocus() && timerStarted && !calibrationFinished) // If the reticle moves out of focus, reset
            {
                reticleInstruction.ToggleFocus();
                StopCoroutine(timer);
                timerStarted = false;
            }
            
        }
    }

    public void StartCalibration()
    {
        if (!calibrationStarted)
        {
            calibrateButton.interactable = false; // Disable the calibration button while calibration is happening
            calibrationStarted = true;
            calibrationFinished = false;

            // Get the forward axis of the camera so the reticle can be spawned directly in front
            Vector3 horizontalForward = Vector3.ProjectOnPlane(heightUtility.camera.forward, Vector3.up).normalized;
            Vector3 startPos = heightUtility.camera.transform.position + horizontalForward * reticleDistance;

            focusReticle = Instantiate(original: focusReticlePrefab,
                                       position: startPos,
                                       rotation: Quaternion.LookRotation(heightUtility.camera.transform.position - startPos, Vector3.up)).transform;

            reticleInstruction = focusReticle.GetComponent<ReticleInstruction>();
        }
    }

    bool IsReticleInFocus()
    {
        // Find the horizontal direction of the reticle and reverse its y rotation
        Vector3 reticleHorizontalBackward = Quaternion.Euler(0, 180, 0) * Vector3.ProjectOnPlane(focusReticle.forward, Vector3.up).normalized;

        // Find the angle between where the camera is pointing and the reticle
        float angle = Vector3.Angle(reticleHorizontalBackward, heightUtility.camera.transform.forward);

        return angle < focusThresholdAngle;
    }

    // Countdown for time user focused on the reticle
    IEnumerator Timer()
    {
        timerStarted = true;
        float currentTimer = timerDuration;

        while (currentTimer > 0)
        {
            reticleInstruction.SetTimerNumber((int)currentTimer);
            yield return new WaitForSeconds(1f);
            currentTimer -= 1f;
        }

        // Calculate user height when the timer hits 0
        heightUtility.CalculateUserHeight();
        reticleInstruction.ShowConfirmation();
        calibrationFinished = true;

        yield return new WaitForSeconds(1f);
        
        calibrationStarted = false;
        timerStarted = false;

        Destroy(focusReticle.gameObject);

        // Reactivate the calibrate button and allow avatar swapping
        calibrateButton.interactable = true;
        menu.SetRoleDropdownsInteractable(true);
    }
}
