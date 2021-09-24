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
    public List<SplinePositioner> followedSplinesTop0;
    public List<Transform> splineFollowersTop0;
    public List<SplinePositioner> followedSplinesBottom0;
    public List<Transform> splineFollowersBottom0;
    public List<SplinePositioner> followedSplinesTop1;
    public List<Transform> splineFollowersTop1;
    public List<SplinePositioner> followedSplinesBottom1;
    public List<Transform> splineFollowersBottom1;
    public bool colonMatchX; // Which axis the colon spheres should match with the spline follower
    public bool colonMatchY;
    public bool colonMatchZ;
    public List<Transform> colon0FrontSpheres;
    public List<Transform> colon1FrontSpheres;
    public float linearStaplerControlInsertionThreshold; // How deep the LS need to be inserted into the colon in order for the stapler to take over control of the colon motion

    public Transform activeForceps; // Which forceps is the user using
    public List<Vector3> controllerStartPosition; // Position of the forceps or the linear stapler when user start to grab the colon with it or inserted the linear stapler while the grabbing forceps is released
    public List<float> controllerStartPercent; // Percentage of spline follower's position when user start to grab the colon with it or inserted the linear stapler
    public List<int> updateMode; // How the colon sphere position should be updated?
    // 0. Do nothing
    // 1. Follow forceps
    // 2. Follow linear stapler tilt
    // 3. Follow linear stapler vertical
    //
    public List<Vector3> splineFollowersStartPositionTop; // The position of spline followers when they are at 0 percent on their spline
    public List<Vector3> splineFollowersStartPositionBottom; // The position of spline followers when they are at 0 percent on their spline
    public List<Transform> colonController; // The controller transform for each colon
    public List<Vector3> colon0FrontSphereStartPosition; // Initial position of the colon spheres when user start lifting the colon
    public List<Vector3> colon1FrontSphereStartPosition;
    public List<float> colon0SphereWeights; // Determine how spheres will move with spline followers
    public List<float> colon1SphereWeights;

    public globalOperators globalOperators; // Main manager of the simulation
    public LinearStaplerTool linearStaplerTool; // LS manager
    public static ColonMovementController instance;

    // Test
    public bool isTest;
    public float testPercent;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        splineFollowersStartPositionTop = new List<Vector3>();
        splineFollowersStartPositionBottom = new List<Vector3>();
        colon0FrontSphereStartPosition = new List<Vector3>();
        colon1FrontSphereStartPosition = new List<Vector3>();
        colon0SphereWeights = new List<float>();
        colon1SphereWeights = new List<float>();
        updateMode = new List<int>();
        updateMode.Add(0);
        updateMode.Add(0);
        controllerStartPosition = new List<Vector3>();
        controllerStartPosition.Add(Vector3.zero);
        controllerStartPosition.Add(Vector3.zero);
        controllerStartPercent = new List<float>();
        controllerStartPercent.Add(0);
        controllerStartPercent.Add(0);
        colonController = new List<Transform>();
        colonController.Add(null);
        colonController.Add(null);

        for (int i = 0; i < splineFollowersTop0.Count; i++)
        {
            splineFollowersStartPositionTop.Add(splineFollowersTop0[i].position);
            splineFollowersStartPositionBottom.Add(splineFollowersBottom0[i].position);
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
        UpdateSplineFollowers();
        UpdateColonSpherePosition();
        //switch (updateMode)
        //{
        //    // Do nothing
        //    case 0:
        //        break;
        //    // Follow forceps up
        //    case 1:
        //        UpdateSplineFollowers(activeForceps);
        //        break;
        //    // Follow linear stapler down
        //    case 2:
        //        UpdateSplineFollowers(activeLinearStapler);
        //        break;
        //    // Follow linear stapler up
        //    case 3:
        //        break;
        //}
    }



    public void ChangeFollowStates(int controlledColon, int newState, Transform newController = null)
    {
        switch (newState)
        {
            // Stop following anything
            case 0:
                StopFollowForceps(controlledColon);
                StopFollowLinearStaplerTilt(controlledColon);
                StopFollowLinearStaplerMove(controlledColon);
                break;
            // Follow forceps up
            case 1:
                StartFollowForceps(controlledColon, newController);
                StopFollowLinearStaplerTilt(controlledColon);
                StopFollowLinearStaplerMove(controlledColon);
                break;
            // Follow linear stapler down
            case 2:
                StartFollowLinearStaplerTilt(controlledColon, newController);
                StopFollowForceps(controlledColon);
                StopFollowLinearStaplerMove(controlledColon);
                break;
            // Follow linear stapler up
            case 3:
                StartFollowLinearStaplerMove(controlledColon, newController);
                StopFollowForceps(controlledColon);
                StopFollowLinearStaplerTilt(controlledColon);
                break;
        }

        updateMode[controlledColon] = newState;
    }

    public void StartFollowForceps(int controlledColon, Transform newForceps)
    {
        colonController[controlledColon] = newForceps;
        controllerStartPosition[controlledColon] = activeForceps.position;
        controllerStartPercent[controlledColon] = (float)followedSplinesTop0[0].position;
    }

    public void StopFollowForceps(int controlledColon)
    {
        // Do nothing if colon is not currently following forceps
        if (updateMode[controlledColon] != 1)
        {
            return;
        }
    }

    public void StartFollowLinearStaplerTilt(int controlledColon, Transform newStapler)
    {
        colonController[controlledColon] = newStapler;
        controllerStartPosition[controlledColon] = newStapler.position;
        controllerStartPercent[controlledColon] = (float)followedSplinesTop0[0].position;
    }

    public void StopFollowLinearStaplerTilt(int controlledColon)
    {
        // Do nothing if colon is not currently following LS tilt
        if (updateMode[controlledColon] != 2)
        {
            return;
        }
    }

    public void StartFollowLinearStaplerMove(int controlledColon, Transform newStapler)
    {

    }

    public void StopFollowLinearStaplerMove(int controlledColon)
    {
        // Do nothing if colon is not currently following LS vertical
        if (updateMode[controlledColon] != 3)
        {
            return;
        }
    }

    /// <summary>
    /// Updates the spline follower's position based on the forceps/linear stapler's position
    /// </summary>
    public void UpdateSplineFollowers()
    {
        // Colon0
        if (colonController[0] != null)
        {
            float percent = Mathf.Clamp01((colonController[0].position.y - controllerStartPosition[0].y) / (8.37f - 3.33f) + controllerStartPercent[0]); // Normalize forceps y position

            followedSplinesTop0.ForEach(sp => sp.position = percent);
            followedSplinesBottom0.ForEach(sp => sp.position = percent);
        }
        // Colon1
        if (colonController[1] != null)
        {
            float percent = Mathf.Clamp01((colonController[1].position.y - controllerStartPosition[1].y) / (8.37f - 3.33f) + controllerStartPercent[1]); // Normalize forceps y position

            followedSplinesTop1.ForEach(sp => sp.position = percent);
            followedSplinesBottom1.ForEach(sp => sp.position = percent);
        }
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
        if (colonController[0] != null)
        {
            for (int i = 0; i < colon0FrontSpheres.Count; i++)
            {
                Vector3 topFollowerDisplacement = splineFollowersTop0[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPositionTop[Mathf.FloorToInt(i / 20f)];
                Vector3 bottomFollowerDisplacement = splineFollowersBottom0[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPositionBottom[Mathf.FloorToInt(i / 20f)];

                colon0FrontSpheres[i].position = colon0FrontSphereStartPosition[i] + bottomFollowerDisplacement + (topFollowerDisplacement - bottomFollowerDisplacement) * colon0SphereWeights[i];
            }
        }
        if (colonController[1] != null)
        {
            for (int i = 0; i < colon1FrontSpheres.Count; i++)
            {
                Vector3 topFollowerDisplacement = splineFollowersTop0[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPositionTop[Mathf.FloorToInt(i / 20f)];
                Vector3 bottomFollowerDisplacement = splineFollowersBottom0[Mathf.FloorToInt(i / 20f)].position - splineFollowersStartPositionBottom[Mathf.FloorToInt(i / 20f)];

                colon1FrontSpheres[i].position = colon1FrontSphereStartPosition[i] + bottomFollowerDisplacement + (topFollowerDisplacement - bottomFollowerDisplacement) * colon1SphereWeights[i];
            }
        }
    }

    public void TestColonMotion()
    {
        followedSplinesTop0.ForEach(sp => sp.position = testPercent);
        followedSplinesBottom0.ForEach(sp => sp.position = testPercent);
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
