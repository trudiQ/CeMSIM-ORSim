using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for storing kinematic Tracker informaiton
/// </summary>
public class Tracker
{
    public Vector3 positions;   //Position along each axis
    public Vector3 angles;      //Angle about each axis
    public Quaternion rotation; // Rotation from the tracker's real orientation relative to the Unity world coordinate system
}