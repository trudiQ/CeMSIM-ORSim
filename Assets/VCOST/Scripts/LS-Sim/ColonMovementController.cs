using Dreamteck.Splines;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public List<SplinePositioner> followedSplinesTop;
    public List<Transform> splineFollowersTop;
    public List<SplinePositioner> followedSplinesBottom;
    public List<Transform> splineFollowersBottom;
    public bool colonMatchX; // Which axis the colon spheres should match with the spline follower
    public bool colonMatchY;
    public bool colonMatchZ;
    public List<Transform> colon0FrontSpheres;
    public List<Transform> colon1FrontSpheres;

    public Transform activeForceps; // Which forceps is the user using
    public Vector3 controllerStartPosition; // Position of the forceps or the linear stapler when user start to grab the colon with it or inserted the linear stapler while the grabbing forceps is released
    public float controllerStartPercent; // Percentage of spline follower's position when user start to grab the colon with it or inserted the linear stapler
    public Transform activeLinearStapler; // Which linear stapler is controlling the colon tilt
    public int updateMode; // How the colon sphere position should be updated?
    // 0. Do nothing
    // 1. Follow forceps
    // 2. Follow linear stapler tilt
    // 3. Follow linear stapler vertical
    //
    public List<Vector3> splineFollowersStartPositionTop; // The position of spline followers when they are at 0 percent on their spline
    public List<Vector3> splineFollowersStartPositionBottom; // The position of spline followers when they are at 0 percent on their spline
    public int controlledColon; // Whic colon is being manipulated right now
    public List<Vector3> colon0FrontSphereStartPosition; // Initial position of the colon spheres when user start lifting the colon
    public List<Vector3> colon1FrontSphereStartPosition;
    public List<float> colon0SphereWeights; // Determine how spheres will move with spline followers
    public List<float> colon1SphereWeights;

    public globalOperators globalOperators; // Main manager of the simulation
    public LinearStaplerTool linearStaplerTool; // LS manager

    // Test
    public bool isTest;
    public float testPercent;

    // Start is called before the first frame update
    void Start()
    {
        splineFollowersStartPositionTop = new List<Vector3>();
        splineFollowersStartPositionBottom = new List<Vector3>();
        colon0FrontSphereStartPosition = new List<Vector3>();
        colon1FrontSphereStartPosition = new List<Vector3>();
        colon0SphereWeights = new List<float>();
        colon1SphereWeights = new List<float>();

        for (int i = 0; i < splineFollowersTop.Count; i++)
        {
            splineFollowersStartPositionTop.Add(splineFollowersTop[i].position);
            splineFollowersStartPositionBottom.Add(splineFollowersBottom[i].position);
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
                UpdateSplineFollowers(activeForceps);
                break;
            // Follow linear stapler down
            case 2:
                UpdateSplineFollowers(activeLinearStapler);
                break;
            // Follow linear stapler up
            case 3:
                break;
        }
    }



    public void ChangeFollowStates(int newState, Transform newController = null)
    {
        switch (newState)
        {
            // Stop following anything
            case 0:
                StopFollowForceps();
                StopFollowLinearStaplerTilt();
                StopFollowLinearStaplerMove();
                break;
            // Follow forceps up
            case 1:
                StartFollowForceps(newController);
                StopFollowLinearStaplerTilt();
                StopFollowLinearStaplerMove();
                break;
            // Follow linear stapler down
            case 2:
                StartFollowLinearStaplerTilt(newController);
                StopFollowForceps();
                StopFollowLinearStaplerMove();
                break;
            // Follow linear stapler up
            case 3:
                StartFollowLinearStaplerMove(newController);
                StopFollowForceps();
                StopFollowLinearStaplerTilt();
                break;
        }

        updateMode = newState;
    }

    public void StartFollowForceps(Transform newForceps)
    {
        activeForceps = newForceps;
        controllerStartPosition = activeForceps.position;
        controllerStartPercent = (float)followedSplinesTop[0].position;
    }

    public void StopFollowForceps()
    {
        // Do nothing if colon is not currently following forceps
        if (updateMode != 1)
        {
            return;
        }
    }

    public void StartFollowLinearStaplerTilt(Transform newStapler)
    {
        activeLinearStapler = newStapler;
        controllerStartPosition = activeLinearStapler.position;
        controllerStartPercent = (float)followedSplinesTop[0].position;
    }

    public void StopFollowLinearStaplerTilt()
    {
        // Do nothing if colon is not currently following LS tilt
        if (updateMode != 2)
        {
            return;
        }
    }

    public void StartFollowLinearStaplerMove(Transform newStapler)
    {

    }

    public void StopFollowLinearStaplerMove()
    {
        // Do nothing if colon is not currently following LS vertical
        if (updateMode != 3)
        {
            return;
        }
    }

    /// <summary>
    /// Updates the spline follower's position based on the forceps/linear stapler's position
    /// </summary>
    /// <param name="controller"></param>
    public void UpdateSplineFollowers(Transform controller)
    {
        float percent = Mathf.Clamp01((controller.position.y - controllerStartPosition.y) / (8.37f - 3.33f) + controllerStartPercent); // Normalize forceps y position

        followedSplinesTop.ForEach(sp => sp.position = percent);
        followedSplinesBottom.ForEach(sp => sp.position = percent);
    }

    [ShowInInspector]
    public void InitializeColonSphereData()
    {
        // Get colon sphere initial positions
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

        // Get colon sphere initial local vertical weight
        colon0SphereWeights = GetColonSphereVerticlaWeight(colon0FrontSpheres);
        //colon0SphereWeights.Clear();
        //for (int i = 0; i < colon0FrontSpheres.Count; i += 20)
        //{
        //    colon0SphereWeights.AddRange(GetColonSphereVerticlaWeight(colon0FrontSpheres.GetRange(i, 20)));
        //}
        colon1SphereWeights = GetColonSphereVerticlaWeight(colon1FrontSpheres);
        //colon1SphereWeights.Clear();
        //for (int i = 0; i < colon1FrontSpheres.Count; i += 20)
        //{
        //    colon1SphereWeights.AddRange(GetColonSphereVerticlaWeight(colon1FrontSpheres.GetRange(i, 20)));
        //}
    }

    public void UpdateColonSpherePosition()
    {
        if (controlledColon == 0)
        {
            for (int i = 0; i < colon0FrontSpheres.Count; i++)
            {
                Vector3 topFollowerDisplacement = splineFollowersTop[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPositionTop[Mathf.FloorToInt(i / 20f)];
                Vector3 bottomFollowerDisplacement = splineFollowersBottom[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPositionBottom[Mathf.FloorToInt(i / 20f)];

                colon0FrontSpheres[i].position = colon0FrontSphereStartPosition[i] + bottomFollowerDisplacement + (topFollowerDisplacement - bottomFollowerDisplacement) * colon0SphereWeights[i];
            }
        }
        else if (controlledColon == 1)
        {
            for (int i = 0; i < colon1FrontSpheres.Count; i++)
            {
                Vector3 topFollowerDisplacement = splineFollowersTop[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPositionTop[Mathf.FloorToInt(i / 20f)];
                Vector3 bottomFollowerDisplacement = splineFollowersBottom[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPositionBottom[Mathf.FloorToInt(i / 20f)];

                colon1FrontSpheres[i].position = colon1FrontSphereStartPosition[i] + bottomFollowerDisplacement + (topFollowerDisplacement - bottomFollowerDisplacement) * colon1SphereWeights[i];
            }
        }
    }

    public void TestColonMotion()
    {
        followedSplinesTop.ForEach(sp => sp.position = testPercent);
        followedSplinesBottom.ForEach(sp => sp.position = testPercent);
        UpdateColonSpherePosition();
    }

    /// <summary>
    /// After evaluate the y position of the given spheres, return a list of weight between 0 to 1, 
    /// decides how the spheres should be based on the two spline followers
    /// </summary>
    /// <param name="evaluatedSpheres"></param>
    /// <returns></returns>
    public List<float> GetColonSphereVerticlaWeight(List<Transform> evaluatedSpheres)
    {
        float maxHeight = Mathf.Max(evaluatedSpheres.Select(t => t.position.y).ToArray());
        float minHeight = Mathf.Min(evaluatedSpheres.Select(t => t.position.y).ToArray());
        float difference = maxHeight - minHeight;

        List<float> weights = new List<float>();
        evaluatedSpheres.ForEach(t => weights.Add((t.position.y - minHeight) / difference));

        return weights;
    }
}
