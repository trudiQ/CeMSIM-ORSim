using Dreamteck.Splines;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When raising up the colon with forceps, colon move with the forceps
/// Linear stapler can move inside the colon if the colon is actively grabbed by the forceps (if forceps is released in mid-air, the forceps will stay in mid-air)
/// After inserting linear stapler (within valid insertion depth), once the forceps is released, 
/// colon will start move with the linear stapler instead of forceps, and user cannot move linear stapler inside colon anymore
/// If user want to adjust linear stapler insertion depth, need to grab the grabbing forceps again, then colon will move with the forceps and user can adjust linear stapler position inside colon
/// </summary>
public class ColonMovementController : MonoBehaviour
{
    public List<SplinePositioner> followedSplines;
    public List<Transform> splineFollowers;
    public bool colonMatchX; // Which axis the colon spheres should match with the spline follower
    public bool colonMatchY;
    public bool colonMatchZ;
    public List<Transform> colon0FrontSpheres;
    public List<Transform> colon1FrontSpheres;

    public Transform activeForceps; // Which forceps is the user using
    public Vector3 forcpesStartGrabbingPosition; // Position of the forceps when user start to grab the colon with it
    public int updateMode; // How the colon sphere position should be updated?
    // 0. Do nothing
    // 1. Follow forceps
    // 2. Follow linear stapler tilt
    // 3. Follow linear stapler vertical
    //
    public List<Vector3> splineFollowersStartPosition; // The position of spline followers when they are at 0 percent on their spline
    public int controlledColon; // Whic colon is being manipulated right now
    public List<Vector3> colon0FrontSphereStartPosition; // Initial position of the colon spheres when user start lifting the colon
    public List<Vector3> colon1FrontSphereStartPosition;

    public globalOperators globalOperators; // Main manager of the simulation
    public LinearStaplerTool linearStaplerTool; // LS manager

    // Test
    public bool isTest;
    public float testPercent;

    // Start is called before the first frame update
    void Start()
    {
        splineFollowersStartPosition = new List<Vector3>();
        colon0FrontSphereStartPosition = new List<Vector3>();
        colon1FrontSphereStartPosition = new List<Vector3>();

        for (int i = 0; i < splineFollowers.Count; i++)
        {
            splineFollowersStartPosition.Add(splineFollowers[i].position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isTest)
        {
            TestColonMotion();
        }
    }

    public void MainUpdatingLoop()
    {
        switch (updateMode)
        {
            // Do nothing
            case 0:
                break;
            // Follow forceps up
            case 1:
                break;
            // Follow linear stapler down
            case 2:
                break;
            // Follow linear stapler up
            case 3:
                break;
        }
    }

    public void ChangeFollowStates(int newState)
    {

    }

    public void StartFollowForceps()
    {

    }

    public void StopFollowForceps()
    {

    }

    public void StartFollowLinearStaplerTilt()
    {

    }

    public void StopFollowLinearStaplerTile()
    {

    }

    public void StartFollowLinearStaplerMove()
    {

    }

    public void StopFollowLinearStaplerMove()
    {

    }

    /// <summary>
    /// Updates the spline follower's position based on the forceps/linear stapler's position
    /// </summary>
    /// <param name="controller"></param>
    public void UpdateSplineFollowers(Transform controller)
    {
        float percent = Mathf.Clamp01((controller.position.y - forcpesStartGrabbingPosition.y) / (8.37f - 3.33f)); // Normalize forceps y position

        followedSplines.ForEach(sp => sp.position = percent);
    }

    [ShowInInspector]
    public void SaveColonSpherePosition()
    {
        colon0FrontSphereStartPosition.Clear();
        for (int i = 0; i < colon0FrontSpheres.Count; i++)
        {
            colon0FrontSphereStartPosition.Add(colon0FrontSpheres[i].position);
        }
        colon1FrontSphereStartPosition.Clear();
        for (int i = 0; i < colon1FrontSpheres.Count; i++)
        {
            colon1FrontSphereStartPosition.Add(colon1FrontSpheres[i].position);
        }
    }

    public void UpdateColonSpherePosition()
    {
        if (controlledColon == 0)
        {
            for (int i = 0; i < colon0FrontSpheres.Count; i++)
            {
                colon0FrontSpheres[i].position = colon0FrontSphereStartPosition[i] + splineFollowers[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPosition[Mathf.FloorToInt(i / 20f)];
            }
        }
        else if (controlledColon == 1)
        {
            for (int i = 0; i < colon1FrontSpheres.Count; i++)
            {
                colon1FrontSpheres[i].position = colon1FrontSphereStartPosition[i] + splineFollowers[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPosition[Mathf.FloorToInt(i / 20f)];
            }
        }
    }

    public void TestColonMotion()
    {
        followedSplines.ForEach(sp => sp.position = testPercent);
        UpdateColonSpherePosition();
    }
}
