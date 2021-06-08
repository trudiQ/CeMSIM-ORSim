using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LinearStaplerTool : Tool //inherits Tool class
{
    public GameObject FiringHandle;
    public Transform firingHandleStartPosition;
    public Transform firingHandleEndPosition;
    public GameObject LockingLever;
    public List<StaplerAttachDetection> attachValidators; // Trigger colliders that validates the tool's two parts' positions to see if they are within attaching distance
    public Transform bottomPartLockingPosition; // Where the bottom part of the tool should be when it is locked with the top part
    public GameObject topHalf;
    public GameObject bottomHalf; // Bottom half of the tool (the half without moving parts)
    public Transform topTracker;
    public Transform bottomTracker; // Tracker for the bottom half of the tool
    public Vector3 bottomPartRelativeTrackerPosition;
    public Quaternion bottomPartRelativeTrackerRotation;
    public Transform topPartDesignatedCalibrationReference; // Reference point that user have to match up to with the real tool when performing calibration for tool rotation and position
    public Transform bottomPartDesignatedCalibrationReference;
    public List<Transform> colonAopenSpheres; // Spheres that will create the insertion opening on the colon0
    public List<Transform> colonBopenSpheres;
    public Transform topHalfFrontTip; // The front of the tip of the top half of the LS tool
    public Transform bottomHalfFrontTip;

    public bool handlePushed;
    public static bool leverLocked;
    public bool inAnimation; // Is the tool currently in any animation
    public float handleReading; // Sensor input for the firing handle position (should be from 0 to 1, 1 is pushed all the way in)
    public float leverReading; // Sensor input for the locking level angle (1 should be the lock position)

    public override void Start()
    {
        base.Start();

        // Save the local position & rotation of the bottom half of the tool relative to the tracker for bottom part
        bottomPartRelativeTrackerPosition = bottomHalf.transform.localPosition;
        bottomPartRelativeTrackerRotation = bottomHalf.transform.localRotation;
    }

    void Update() //Checks status of knob, lever, and linear stapler in every frame
    {
        // Prevent user interaction if the tool is currently in an animation
        if (inAnimation)
        {
            return;
        }

        FireKnobWithKeyboardInput();
        LockLeverWithKeyboardInput();
        Enabler();
    }

    void FireKnobWithKeyboardInput()
    {
        if (Enable && Input.GetKeyUp(KeyCode.T))
        {
            // Handle translates to final position if T is pressed and linear stapler is enabled
            if (!handlePushed)
            {
                FiringHandle.transform.localPosition = firingHandleEndPosition.localPosition;
                print("Firing Handle is engaged.");
            }
            // Handle translates to initial position if T is pressed again and linear stapler is enabled
            else
            {
                FiringHandle.transform.localPosition = firingHandleStartPosition.localPosition;
                print("Firing Handle is not engaged.");
            }

            handlePushed = !handlePushed;
        }
    }

    void UpdateKnobWithSensorReading()
    {
        if (Enable)
        {
            FiringHandle.transform.localPosition = Vector3.Lerp(firingHandleStartPosition.localPosition, firingHandleEndPosition.localPosition, handleReading);

            handlePushed = handleReading > 0.95f ? true : false;
        }
    }

    void LockLeverWithKeyboardInput()
    {
        if (Enable && Input.GetKeyUp(KeyCode.Y))
        {
            //Locking lever closes if Y is pressed and linear stapler is enabled
            if (!leverLocked)
            {
                LockingLever.transform.localEulerAngles = Vector3.zero;
                print("Locking Lever is closed.");

                // If the tool parts are in valid locking position then lock the tools together
                if (ValidateToolLockingCondition())
                {
                    LockTool();
                }
            }
            //Locking lever opens if Y is pressed again and linear stapler is enabled
            else
            {
                LockingLever.transform.localEulerAngles = new Vector3(0, 15, 0);
                print("Locking Lever is open.");

                UnlockTool();
            }

            leverLocked = !leverLocked;
        }
    }

    void UpdateLeverWithSensorReading()
    {
        if (Enable)
        {
            LockingLever.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, 15, 0), leverReading);

            // Lerp bottom half model towards top half based on handle reading
            if (leverReading >= 0.03f)
            {
                bottomHalf.transform.localPosition = Vector3.Lerp(bottomHalf.transform.localPosition, Vector3.zero, leverReading);
                bottomHalf.transform.localRotation = Quaternion.Lerp(bottomHalf.transform.localRotation, Quaternion.identity, leverReading);
            }

            leverLocked = leverReading > 0.95f ? true : false;

            if (leverLocked)
            {
                bottomHalf.transform.parent = bottomPartLockingPosition;
            }
            else
            {
                bottomHalf.transform.parent = bottomTracker;
            }
        }
    }

    /// <summary>
    /// When user try to lock the tool parts together, validate it the tool parts meets the locking condition
    /// </summary>
    /// <returns></returns>
    public bool ValidateToolLockingCondition()
    {
        return !attachValidators.Find(v => !v.isTogether);
    }

    /// <summary>
    /// Lock the bottom half of the tool to the top half, stop tracking of the bottom half
    /// </summary>
    public void LockTool()
    {
        bottomHalf.transform.parent = bottomPartLockingPosition;
        inAnimation = true;
        StartCoroutine(LockToolAnimation());
    }

    /// <summary>
    /// Animation for locking the tool parts
    /// </summary>
    /// <returns></returns>
    public IEnumerator LockToolAnimation()
    {
        Vector3 startLocalPosition = bottomHalf.transform.localPosition;
        Quaternion startLocalRotation = bottomHalf.transform.localRotation;

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            yield return null;
            bottomHalf.transform.localPosition = Vector3.Lerp(startLocalPosition, Vector3.zero, t);
            bottomHalf.transform.localRotation = Quaternion.Lerp(startLocalRotation, Quaternion.identity, t);
        }

        bottomHalf.transform.localPosition = Vector3.zero;
        bottomHalf.transform.localRotation = Quaternion.identity;
        inAnimation = false;
    }

    /// <summary>
    /// Unlock the bottom half of the tool from the top half, resume tracking of the bottom half
    /// </summary>
    public void UnlockTool()
    {
        bottomHalf.transform.parent = bottomTracker;
        bottomHalf.transform.localPosition = bottomPartRelativeTrackerPosition;
        bottomHalf.transform.localRotation = bottomPartRelativeTrackerRotation;
    }

    /// <summary>
    /// After user orient the real tool to the designated orientation, update the rotation on the tool model in Unity so that it matches up with the real tool
    /// </summary>
    [ShowInInspector]
    public void CalibrateToolTopPartRotation()
    {
        topHalf.transform.rotation = topPartDesignatedCalibrationReference.rotation;
    }
    [ShowInInspector]
    public void CalibrateToolBottomPartRotation()
    {
        bottomHalf.transform.rotation = bottomPartDesignatedCalibrationReference.rotation;
    }
}
