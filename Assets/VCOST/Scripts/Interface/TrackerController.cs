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
    
    [Header("Filter")]
    public bool filter=true;
    public int filterSteps=5;

    private Vector3 rawPosition;
    private Vector3 rawAngle;
    private Vector3[] rawPositions;
    private Vector3 filteredSum;
    private Vector3 filteredPosition;
    private Quaternion orientation;
    private Vector3 offsetAngle = new Vector3(0, 0, 0);
    
    void Start()
    {
        rawPositions = new Vector3[filterSteps];
        filteredSum = new Vector3(0,0,0);
    }

    void Update()
    {
        //Read tracker data from the TrakSTAR
        ReadData();
        
        //Filters the position data
        UpdateFilter();

        //Rotate the transforms based on the angles
        RotateFrame();
        
        //Applies the position and rotation change to the object
        ApplyKinematics();
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
    /// Applies a Moving Average Filter to the position
    /// </summary>  
    private void UpdateFilter()
    {
        filteredSum -= rawPositions[0];

        for(int i=0;i< filterSteps - 1;i++)
        {
            rawPositions[i] = rawPositions[i+1];
        }

        rawPositions[filterSteps - 1] = rawPosition;

        filteredSum += rawPositions[filterSteps - 1];

        filteredPosition = filteredSum/ filterSteps;
    }

    /// <summary>
    /// Calculates the frame orientation of this object based on the raw angles.
    /// </summary>
    private void RotateFrame()
    {
        orientation = Quaternion.Euler(rawAngle-offsetAngle);
    }
    
    /// <summary>
    /// Applies the position and rotation to the transform
    /// </summary>
    private void ApplyKinematics()
    {
        if(filter)
        {
            this.transform.SetPositionAndRotation(filteredPosition, orientation);
        }
        else
        {
            this.transform.SetPositionAndRotation(rawPosition, orientation);
        }
    }

    /// <summary>
    /// Sets the current angle as an offset
    /// </summary>
    public void ZeroOrientation()
    {
        offsetAngle = rawAngle-calibratedAngle;
    }
}
