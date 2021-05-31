using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

// Manually applies the rotation limits with a VRIK component since it has its own constraints
public class AvatarRotationLimiter : MonoBehaviour
{
    public VRIK ik;
    public RotationLimit[] rotationLimits;

    void Start()
    {
        foreach (RotationLimit limit in rotationLimits)
            limit.enabled = false;

        ik.solver.OnPostUpdate += AfterVRIK;
    }

    private void AfterVRIK()
    {
        foreach (RotationLimit limit in rotationLimits)
            limit.Apply();
    }
}
