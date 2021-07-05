using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.Events;

// Sets the scale of the avatar based on the standing height of the user
[RequireComponent(typeof(AvatarPrefabHeightUtility))]
public class AvatarHeightCalibration : MonoBehaviour
{
    public VRIK ik;
    public UserHeightUtility userHeightUtility;

    private AvatarPrefabHeightUtility avatarHeightUtility;
    private Vector3 startingScale;
    private float startingFootDistance;
    private float startingStepThreshold;
    private float currentScaleMultiplier = 1;

    public UnityEvent<float> OnAvatarHeightChanged; // Sends new avatar scale, foot distance, and step threshold 
                                                    // as a single multiplier when calibrated. Made to send client data to server

    void Start()
    {
        avatarHeightUtility = GetComponent<AvatarPrefabHeightUtility>();

        // Store the initial data for later use
        startingScale = ik.references.root.localScale;
        startingFootDistance = ik.solver.locomotion.footDistance;
        startingStepThreshold = ik.solver.locomotion.stepThreshold;

        Calibrate();
    }

    // Must be called at runtime since the user has to be standing to get an accurate height
    public void Calibrate()
    {
        // Get the current height of the user and resize the avatar
        // New scale is based on the height difference between the current scale and the calibrated scale
        float newHeightScale = userHeightUtility.userHeight / 
                               (avatarHeightUtility.height * (ik.references.root.localScale.y / startingScale.y));

        currentScaleMultiplier *= newHeightScale;
        Calibrate(currentScaleMultiplier);

        OnAvatarHeightChanged.Invoke(currentScaleMultiplier);
    }

    // Use a given height scale to resize the avatar, used for resizing avatar on server/non-local client
    public void Calibrate(float scaleMultiplier)
    {
        currentScaleMultiplier = scaleMultiplier;

        ik.references.root.localScale = startingScale * currentScaleMultiplier;
        ik.solver.locomotion.footDistance = startingFootDistance * currentScaleMultiplier;
        ik.solver.locomotion.stepThreshold = startingStepThreshold * currentScaleMultiplier;
    }
}