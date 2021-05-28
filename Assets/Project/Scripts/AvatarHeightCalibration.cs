using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

// Sets the scale of the avatar based on the standing height of the user
public class AvatarHeightCalibration : MonoBehaviour
{
    public VRIK ik;

    // Must be called to calibrate since the user has to be standing to get an accurate height
    public void Calibrate()
    {
        float newHeightScale = (ik.solver.spine.headTarget.position.y - ik.references.root.position.y) / 
            (ik.references.head.position.y - ik.references.root.position.y);

        ik.references.root.localScale *= newHeightScale;
    }
}
