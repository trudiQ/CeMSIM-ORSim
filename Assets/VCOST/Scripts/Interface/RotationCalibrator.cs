using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calibrate one starTrak tracker to its Unity counter-part
/// </summary>
public class RotationCalibrator : MonoBehaviour
{
    public Transform trackerTransform;
    public Vector3 xAngleMultiplier;
    public Vector3 yAngleMultiplier;
    public Vector3 zAngleMultiplier;

    /// <summary>
    /// Translate the raw rotation data from the tracker and update the tracker transform so the model attached to the tracker transform rotates the same way it is rotated in the real world
    /// </summary>
    /// <param name="inputEulerAngles"></param>
    public void TranslateRotation(Vector3 inputEulerAngles)
    {
        trackerTransform.eulerAngles = inputEulerAngles.x * xAngleMultiplier + inputEulerAngles.y * yAngleMultiplier + inputEulerAngles.z * zAngleMultiplier;
    }

    /// <summary>
    /// Get the translate vectors by calibrating the current tracker angle readings with the desired orientation angles
    /// </summary>
    /// <param name="currentTrackerAngles"></param>
    /// <param name="childDesiredDefaultOrientation"></param> The target child world rotation in eularangles, which the user will try to match the real world tool towards
    public void CalibrateRotation(Vector3 currentTrackerAngles, Vector3 childDesiredDefaultOrientation)
    {

    }
}
