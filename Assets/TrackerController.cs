using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerController : MonoBehaviour
{
    [Header("References")]
    public TestAPI trakStar;

    [Range(1,4)]
    public int trackerNumber=1;
    public Vector3 calibratedAngle = new Vector3(0, 0, 0);

    private Vector3 rawPosition;
    private Vector3 rawAngle;
    private Quaternion orientation;
    private Vector3 offsetAngle = new Vector3(0, 0, 0);

    void Update()
    {
        //Read tracker data from the TrakSTAR
        ReadData();

        //Rotate the transforms based on the angles
        RotateFrame();
    }

    /// <summary>
    /// Reads the data from the TrakSTAR game object.
    /// </summary>
    private void ReadData()
    {
        rawPosition = trakStar.trackers[trackerNumber-1].positions;
        rawAngle = trakStar.trackers[trackerNumber-1].angles;
    }

    /// <summary>
    /// Rotates the frame of this object based on the raw angles.
    /// </summary>
    private void RotateFrame()
    {
        orientation = Quaternion.Euler(rawAngle-offsetAngle);
        this.transform.SetPositionAndRotation(rawPosition, orientation);
    }

    public void ZeroOrientation()
    {
        offsetAngle = rawAngle-calibratedAngle;
    }
}
