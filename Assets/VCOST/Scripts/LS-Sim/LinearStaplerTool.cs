using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LinearStaplerTool : MonoBehaviour //inherits Tool class
{
    public GameObject FiringHandle;
    public Transform firingHandleStartPosition;
    public Transform firingHandleEndPosition;
    public GameObject LockingLever;
    public List<StaplerAttachDetection> attachValidators; // Trigger colliders that validates the tool's two parts' positions to see if they are within attaching distance
    public float attachDepthDifference; // How close (on a 0-1 scale) the two LS parts needs to be during insertion for them to be able to be locked together
    public Transform bottomPartLockingPosition; // Where the bottom part of the tool should be when it is locked with the top part in the insertion phase
    public Transform bottomPartFullyLockingPosition; // Where the bottom part of the tool should be when it is locked with the top part in the cutting phase
    public GameObject topHalf;
    public GameObject bottomHalf; // Bottom half of the tool (the half without moving parts)
    public Transform topTracker;
    public Transform bottomTracker; // Tracker for the bottom half of the tool
    public Transform topPartDesignatedCalibrationReference; // Reference point that user have to match up to with the real tool when performing calibration for tool rotation and position
    public Transform bottomPartDesignatedCalibrationReference;
    // For LS tool insertion detection
    public List<Transform> colonAopenSpheres; // Spheres that will create the insertion opening on the colon0
    public List<Transform> colonBopenSpheres;
    public Transform topHalfFrontTip; // The front of the tip of the top half of the LS tool
    public Transform bottomHalfFrontTip;
    public float tipProximityCondition;
    public List<Transform> colonAsecondLayerSpheres;
    public List<Transform> colonAlastLayerSpheres;
    public List<Transform> colonBsecondLayerSpheres;
    public List<Transform> colonBlastLayerSpheres;
    public float angleDifferenceCondition; // What's the maximum allowing angle difference from the LS tool to the colon direction for the LS tool to enter insertion phase
    public float tipExitProximityMultiplier; // How many times the tip exiting proximity range it is for the LS tool to exit insertion phase
    // For LS tool after joining colon action
    public List<Transform> joinedColonFirstLayerSpheres;
    public List<Transform> joinedColonLastLayerSpheres;
    public float joiningPhaseToolMovingAxisDifference; // After the colons are joined, how much the top tool should move down
    // For LS tool in closure phase
    public Transform bottomPartLastPhaseProximityDetector; // Use to verify if bottom part enters target position for last closure phase
    public List<Transform> joinedColonFirstLayerLowerSpheres;
    public List<Transform> joinedColonSecondLayerLowerSpheres;
    public List<Transform> joinedColonThirdLayerLowerSpheres;
    public List<Transform> joinedColonForthLayerLowerSpheres;
    public List<Transform> joinedColonFifthLayerLowerSpheres;
    public List<Transform> joinedColonSixthLayerLowerSpheres;
    public float cuttingPhaseToolMovingAxisDifference; // During the cutting phase, how much the bottom tool should move down
    public float cuttingTipHorizontalProximityCondition; // How close the tip has to be to the joined colon center on the x axis to enter cutting moving plane

    public bool handlePushed;
    public static bool leverLocked;
    public bool inAnimation; // Is the tool currently in any animation
    public float handleReading; // Sensor input for the firing handle position (should be from 0 to 1, 1 is pushed all the way in)
    public float leverReading; // Sensor input for the locking level angle (1 should be the lock position)
    public bool topHalfInserting; // Is top half inserting into colon ### May not be needed
    public bool bottomHalfInserting;
    public Vector3 bottomPartRelativeTrackerPosition;
    public Quaternion bottomPartRelativeTrackerRotation;
    public Vector3 topPartRelativeTrackerPosition;
    public Quaternion topPartRelativeTrackerRotation;
    // Simulation states
    public int simStates; // Which step the simulation is at
    // Colon info
    public Vector3 colonAopeningPos;
    public Vector3 colonBopeningPos;
    //public Vector3 colonAsecondLayerPos;
    //public Vector3 colonBsecondLayerPos;
    //public Vector3 colonAlastLayerPos;
    //public Vector3 colonBlastLayerPos;
    public List<float> insertionDepthInspector;
    // Tool moving axis
    public List<Transform> topPartMovingAxisStart;
    public List<Transform> topPartMovingAxisEnd;
    public List<Transform> bottomPartMovingAxisStart;
    public List<Transform> bottomPartMovingAxisEnd;
    public Vector3 topPartMovingAxisStartPoint;
    public Vector3 topPartMovingAxisEndPoint;
    public Vector3 bottomPartMovingAxisStartPoint;
    public Vector3 bottomPartMovingAxisEndPoint;
    public Vector3 joinedColonFirstLayerLowerSpheresPosition;
    public Vector3 joinedColonSecondLayerLowerSpheresPosition;
    public Vector3 joinedColonThirdLayerLowerSpheresPosition;
    public Vector3 joinedColonForthLayerLowerSpheresPosition;
    public Vector3 joinedColonFifthLayerLowerSpheresPosition;
    public Vector3 joinedColonSixthLayerLowerSpheresPosition;
    public bool isBottomHalfMovingInCuttingPlane; // Is the bottom half tool locked onto the moving plane for the last cutting step

    public void Start()
    {
        SaveToolLocalPositionRotation();
        insertionDepthInspector = new List<float>(globalOperators.m_insertDepth);
        simStates = 0;
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
        insertionDepthInspector[0] = globalOperators.m_insertDepth[0];
        insertionDepthInspector[1] = globalOperators.m_insertDepth[1];

        //Enabler();
    }

    private void LateUpdate()
    {
        // Update colon info
        colonAopeningPos = GetPositionMean(colonAopenSpheres);
        colonBopeningPos = GetPositionMean(colonBopenSpheres);
        //colonAsecondLayerPos = GetPositionMean(colonAsecondLayerSpheres);
        //colonBsecondLayerPos = GetPositionMean(colonBsecondLayerSpheres);
        //colonAlastLayerPos = GetPositionMean(colonAlastLayerSpheres);
        //colonBlastLayerPos = GetPositionMean(colonBlastLayerSpheres);
        // Update tool moving axis info
        topPartMovingAxisStartPoint = GetPositionMean(topPartMovingAxisStart);
        topPartMovingAxisEndPoint = GetPositionMean(topPartMovingAxisEnd);
        bottomPartMovingAxisStartPoint = GetPositionMean(bottomPartMovingAxisStart);
        bottomPartMovingAxisEndPoint = GetPositionMean(bottomPartMovingAxisEnd);

        if (simStates == 1)
        {
            topPartMovingAxisStartPoint += Vector3.up * joiningPhaseToolMovingAxisDifference;
            topPartMovingAxisEndPoint += Vector3.up * joiningPhaseToolMovingAxisDifference;
        }
        else if (simStates > 1)
        {
            bottomPartMovingAxisStartPoint += Vector3.up * cuttingPhaseToolMovingAxisDifference;
            bottomPartMovingAxisEndPoint += Vector3.up * cuttingPhaseToolMovingAxisDifference;
        }

        if (simStates < 2)
        {
            CheckAndUpdateLStoolInsertionStates();
        }
        if (globalOperators.m_bInsert[0] == 1)
        {
            globalOperators.m_insertDepth[0] = LockToolMovement(topHalf.transform, topPartMovingAxisStartPoint, topPartMovingAxisEndPoint, Vector3.up * Mathf.Sign(topHalf.transform.up.y));
        }
        if (globalOperators.m_bInsert[1] == 1)
        {
            globalOperators.m_insertDepth[1] = LockToolMovement(topHalf.transform, topPartMovingAxisStartPoint, topPartMovingAxisEndPoint, Vector3.up * Mathf.Sign(topHalf.transform.up.y));
        }

        // If bottom part is locked with top part then stop updating it
        if (bottomHalf.transform.parent == bottomTracker)
        {
            if (globalOperators.m_bInsert[0] == 2)
            {
                globalOperators.m_insertDepth[0] = LockToolMovement(bottomHalf.transform, bottomPartMovingAxisStartPoint, bottomPartMovingAxisEndPoint, Vector3.up * Mathf.Sign(bottomHalf.transform.up.y));
            }
            if (globalOperators.m_bInsert[1] == 2)
            {
                globalOperators.m_insertDepth[1] = LockToolMovement(bottomHalf.transform, bottomPartMovingAxisStartPoint, bottomPartMovingAxisEndPoint, Vector3.up * Mathf.Sign(bottomHalf.transform.up.y));
            }
        }

        // Check for bottom part enter cutting plane
        if (simStates >= 2)
        {
            CheckAndUpdateLStoolTransverseStates();
        }

    }

    void FireKnobWithKeyboardInput()
    {
        if (Input.GetKeyUp(KeyCode.T))
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
        FiringHandle.transform.localPosition = Vector3.Lerp(firingHandleStartPosition.localPosition, firingHandleEndPosition.localPosition, handleReading);

        handlePushed = handleReading > 0.95f ? true : false;
    }

    void LockLeverWithKeyboardInput()
    {
        if (Input.GetKeyUp(KeyCode.Y))
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

    /// <summary>
    /// When user try to lock the tool parts together, validate it the tool parts meets the locking condition
    /// </summary>
    /// <returns></returns>
    public bool ValidateToolLockingCondition()
    {
        return Mathf.Abs(globalOperators.m_insertDepth[0] - globalOperators.m_insertDepth[1]) <= attachDepthDifference;
        //return !attachValidators.Find(v => !v.isTogether);
    }

    /// <summary>
    /// Lock the bottom half of the tool to the top half, stop tracking of the bottom half
    /// </summary>
    public void LockTool()
    {
        // If the tool is locked during insertion phase then dont let them come too much close together
        if (simStates == 0)
        {
            bottomHalf.transform.parent = bottomPartLockingPosition;
        }
        else
        {
            bottomHalf.transform.parent = bottomPartFullyLockingPosition;
        }
        inAnimation = true;
        StartCoroutine(LockToolAnimation());
    }

    /// <summary>
    /// 
    /// </summary>
    public void JoinColonToolLogic()
    {
        bottomHalf.transform.parent = bottomPartFullyLockingPosition;
        bottomHalf.transform.localPosition = Vector3.zero;
        simStates = 1; // Update the simulation step state
        topPartMovingAxisStart = joinedColonFirstLayerSpheres;
        topPartMovingAxisEnd = joinedColonLastLayerSpheres;
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
        if (globalOperators.m_bInsert[0] != 2 && globalOperators.m_bInsert[1] != 2) // If the bottom part is not inserted in colon
        {
            bottomHalf.transform.localPosition = bottomPartRelativeTrackerPosition;
            bottomHalf.transform.localRotation = bottomPartRelativeTrackerRotation;
        }
    }

    /// <summary>
    /// Make sure the tool move along the colon during insertion
    /// </summary>
    /// <param name="controlledObject"></param>
    /// <param name="startPosition"></param>
    /// <param name="endPosition"></param>
    /// <param name="rotateUpDir"></param> Controller object's up direction when align it along the movement direction
    /// <returns></returns> Return float number that indicate if the controlled object is out of controlled path (if outside of 0-1 meaning it moves out of controlled path)
    public float LockToolMovement(Transform controlledObject, Vector3 startPosition, Vector3 endPosition, Vector3 rotateUpDir)
    {
        Vector3 objectToStartVector = controlledObject.position - startPosition;
        float movementRange = Vector3.Distance(startPosition, endPosition);
        float objectNormalToStartDistance = Vector3.Dot(objectToStartVector, Vector3.Normalize(endPosition - startPosition));
        controlledObject.position = startPosition + (endPosition - startPosition).normalized * objectNormalToStartDistance; // Place object on the direction vector

        // Rotate the LS tool to align it with the colon
        controlledObject.LookAt(controlledObject.position + (endPosition - startPosition).normalized, rotateUpDir);
        controlledObject.Rotate(0, -90 * Mathf.Sign(rotateUpDir.y), 0, Space.Self);

        return objectNormalToStartDistance / movementRange;
    }


    public float LockBottomToolMovementInPlaneDuringLastStep(Transform controlledObject, Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 objectToStartVector = controlledObject.position - startPosition;
        float movementRange = Vector3.Distance(startPosition, endPosition);
        float objectNormalToStartDistance = Vector3.Dot(objectToStartVector, Vector3.Normalize(endPosition - startPosition));
        controlledObject.position = startPosition + (endPosition - startPosition).normalized * objectNormalToStartDistance; // Place object on the direction vector
        // See which movement segment the object should belong to
        if (controlledObject.position.z > joinedColonSecondLayerLowerSpheresPosition.z)
        {
            if (controlledObject.position.z < joinedColonThirdLayerLowerSpheresPosition.z)
            {

            }
            else if (controlledObject.position.z < joinedColonForthLayerLowerSpheresPosition.z)
            {

            }
            else if (controlledObject.position.z < joinedColonFifthLayerLowerSpheresPosition.z)
            {

            }
            else
            {

            }
        }

        // Rotate the LS tool to align it with the colon
        controlledObject.LookAt(controlledObject.position + Vector3.down, Vector3.back);
        //controlledObject.Rotate(0, -90 * Mathf.Sign(rotateUpDir.y), 0, Space.Self);

        return objectNormalToStartDistance / movementRange;
    }

    /// <summary>
    /// Check if either part of the LS tool meets the condition to enter colon model through the opening
    /// </summary>
    public void CheckAndUpdateLStoolInsertionStates()
    {
        // Get the distance from each LS tip to each colon opening
        float topToColonA = Vector3.Distance(topHalfFrontTip.position, colonAopeningPos);
        float topToColonB = Vector3.Distance(topHalfFrontTip.position, colonBopeningPos);
        float bottomToColonA = Vector3.Distance(bottomHalfFrontTip.position, colonAopeningPos);
        float bottomToColonB = Vector3.Distance(bottomHalfFrontTip.position, colonBopeningPos);

        // If colon is not joined yet
        if (simStates == 0)
        {
            // If colon0 is not being inserted
            if (globalOperators.m_bInsert[0] == 0)
            {
                // Check for angle
                if (topToColonA <= tipProximityCondition)
                {
                    topPartMovingAxisStart = colonAsecondLayerSpheres;
                    topPartMovingAxisEnd = colonAlastLayerSpheres;
                    if (Vector3.Angle(topHalf.transform.right, GetPositionMean(topPartMovingAxisStart) - GetPositionMean(topPartMovingAxisEnd)) < angleDifferenceCondition)
                    {
                        globalOperators.m_bInsert[0] = 1;
                        topHalfInserting = true;
                    }
                }

                if (bottomToColonA <= tipProximityCondition)
                {
                    bottomPartMovingAxisStart = colonAsecondLayerSpheres;
                    bottomPartMovingAxisEnd = colonAlastLayerSpheres;
                    if (Vector3.Angle(bottomHalf.transform.right, GetPositionMean(bottomPartMovingAxisStart) - GetPositionMean(bottomPartMovingAxisEnd)) < angleDifferenceCondition)
                    {
                        globalOperators.m_bInsert[0] = 2;
                        bottomHalfInserting = true;
                    }
                }

            }
            // If colon1 is not being inserted
            if (globalOperators.m_bInsert[1] == 0)
            {
                // Check for angle
                if (topToColonB <= tipProximityCondition)
                {
                    topPartMovingAxisStart = colonBsecondLayerSpheres;
                    topPartMovingAxisEnd = colonBlastLayerSpheres;
                    if (Vector3.Angle(topHalf.transform.right, GetPositionMean(topPartMovingAxisStart) - GetPositionMean(topPartMovingAxisEnd)) < angleDifferenceCondition)
                    {
                        globalOperators.m_bInsert[1] = 1;
                        topHalfInserting = true;
                    }
                }

                if (bottomToColonB <= tipProximityCondition)
                {
                    bottomPartMovingAxisStart = colonBsecondLayerSpheres;
                    bottomPartMovingAxisEnd = colonBlastLayerSpheres;
                    if (Vector3.Angle(bottomHalf.transform.right, GetPositionMean(bottomPartMovingAxisStart) - GetPositionMean(bottomPartMovingAxisEnd)) < angleDifferenceCondition)
                    {
                        globalOperators.m_bInsert[1] = 2;
                        bottomHalfInserting = true;
                    }
                }
            }
        }

        // Check for LS tool removal
        if (globalOperators.m_bInsert[0] == 1) // If top part is entering colon0
        {
            if (topToColonA >= tipProximityCondition * 2) // If tool is far away from colon opening and outside of the colon
            {
                if (Vector3.Angle(colonAopeningPos - topHalfFrontTip.position, GetPositionMean(topPartMovingAxisStart) - GetPositionMean(topPartMovingAxisEnd)) > 90)
                {
                    globalOperators.m_bInsert[0] = 0;
                    topHalfInserting = false;

                    // Put tool back to default local position and rotation
                    topHalf.transform.localPosition = topPartRelativeTrackerPosition;
                    topHalf.transform.localRotation = topPartRelativeTrackerRotation;
                }
            }
        }
        if (globalOperators.m_bInsert[0] == 2)
        {
            if (bottomToColonA >= tipProximityCondition * 2) // If tool is far away from colon opening and outside of the colon
            {
                if (Vector3.Angle(colonAopeningPos - bottomHalfFrontTip.position, GetPositionMean(bottomPartMovingAxisStart) - GetPositionMean(bottomPartMovingAxisEnd)) > 90)
                {
                    globalOperators.m_bInsert[0] = 0;
                    bottomHalfInserting = false;

                    // Put tool back to default local position and rotation
                    bottomHalf.transform.localPosition = bottomPartRelativeTrackerPosition;
                    bottomHalf.transform.localRotation = bottomPartRelativeTrackerRotation;
                }
            }
        }
        if (globalOperators.m_bInsert[1] == 1)
        {
            if (topToColonA >= tipProximityCondition * 2) // If tool is far away from colon opening and outside of the colon
            {
                if (Vector3.Angle(colonBopeningPos - topHalfFrontTip.position, GetPositionMean(topPartMovingAxisStart) - GetPositionMean(topPartMovingAxisEnd)) > 90)
                {
                    globalOperators.m_bInsert[1] = 0;
                    topHalfInserting = false;

                    // Put tool back to default local position and rotation
                    topHalf.transform.localPosition = topPartRelativeTrackerPosition;
                    topHalf.transform.localRotation = topPartRelativeTrackerRotation;
                }
            }
        }
        if (globalOperators.m_bInsert[1] == 2)
        {
            if (bottomToColonA >= tipProximityCondition * 2) // If tool is far away from colon opening and outside of the colon
            {
                if (Vector3.Angle(colonBopeningPos - bottomHalfFrontTip.position, GetPositionMean(bottomPartMovingAxisStart) - GetPositionMean(bottomPartMovingAxisEnd)) > 90)
                {
                    globalOperators.m_bInsert[1] = 0;
                    bottomHalfInserting = false;

                    // Put tool back to default local position and rotation
                    bottomHalf.transform.localPosition = bottomPartRelativeTrackerPosition;
                    bottomHalf.transform.localRotation = bottomPartRelativeTrackerRotation;
                }
            }
        }

        // If user finish joining and moved both LS parts out of the colon then enter next phase
        if (simStates == 1 && globalOperators.m_bInsert[0] == 0 && globalOperators.m_bInsert[1] == 0)
        {
            simStates = 2;
        }
    }

    /// <summary>
    /// Check if the tool bottom part is ready to do the last step
    /// </summary>
    public void CheckAndUpdateLStoolTransverseStates()
    {
        // Check if bottom part reach target height and z distance towards joined colon, and right axis is aligned with world right, and tip is within the colon width range
        if (!isBottomHalfMovingInCuttingPlane &&
            Mathf.Abs(bottomHalfFrontTip.position.y - (joinedColonFirstLayerLowerSpheresPosition.y + cuttingPhaseToolMovingAxisDifference)) < tipProximityCondition &&
            Mathf.Abs(bottomHalfFrontTip.position.z - joinedColonFirstLayerLowerSpheresPosition.z) < tipProximityCondition &&
            Mathf.Abs(bottomHalfFrontTip.position.x - joinedColonFirstLayerLowerSpheresPosition.x) < cuttingTipHorizontalProximityCondition &&
            Vector3.Angle(bottomHalf.transform.right, Vector3.right) < angleDifferenceCondition)
        {
            isBottomHalfMovingInCuttingPlane = true;
        }
        // Check if bottom part exit cutting plane
        if (isBottomHalfMovingInCuttingPlane &&
            Vector3.Angle((bottomHalfFrontTip.position - joinedColonFirstLayerLowerSpheresPosition), (joinedColonSixthLayerLowerSpheresPosition - joinedColonFirstLayerLowerSpheresPosition)) > 90 &&
            Mathf.Abs(bottomHalfFrontTip.position.z - joinedColonFirstLayerLowerSpheresPosition.z) > tipProximityCondition * tipExitProximityMultiplier)
        {
            isBottomHalfMovingInCuttingPlane = false;
        }
    }

    public Vector3 GetPositionMean(List<Transform> positions)
    {
        Vector3 sum = Vector3.zero;
        positions.ForEach(t => sum += t.position);

        return sum / positions.Count;
    }

    /// <summary>
    /// After user orient the real tool to the designated orientation, update the rotation on the tool model in Unity so that it matches up with the real tool
    /// </summary>
    [ShowInInspector]
    public void CalibrateToolTopPartRotation()
    {
        topHalf.transform.rotation = topPartDesignatedCalibrationReference.rotation;
        SaveToolLocalPositionRotation();
    }
    [ShowInInspector]
    public void CalibrateToolBottomPartRotation()
    {
        bottomHalf.transform.rotation = bottomPartDesignatedCalibrationReference.rotation;
        SaveToolLocalPositionRotation();
    }

    public void SaveToolLocalPositionRotation()
    {
        // Save the local position & rotation of the bottom half of the tool relative to the tracker for bottom part
        bottomPartRelativeTrackerPosition = bottomHalf.transform.localPosition;
        bottomPartRelativeTrackerRotation = bottomHalf.transform.localRotation;
        topPartRelativeTrackerPosition = topHalf.transform.localPosition;
        topPartRelativeTrackerRotation = topHalf.transform.localRotation;
    }
}
