using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiRod))]
public class RodStretchNormalizer : MonoBehaviour
{
    private ObiSolver solver;
    private ObiRod rod;
    
    private float minimumStretchFactor = 1.5f;

    private ObiNativeFloatList oldInvMasses;

    int lowerParticleLimit = -1;
    int higherParticleLimit = -1;

    public void EnableAndLimitBetweenExclusively(int lower, int higher)
    {
        this.enabled = true;
        lowerParticleLimit = lower;
        higherParticleLimit = higher;

        solver = GetComponentInParent<ObiSolver>();
        rod = GetComponent<ObiRod>();

        oldInvMasses = new ObiNativeFloatList();
        foreach (float f in solver.invMasses)
        {
            oldInvMasses.Add(f);
        }
    }

    private void FixedUpdate()
    {
        for(int i = lowerParticleLimit+2; i < higherParticleLimit-2; i++)
        {
            float rightStretchFactor =
                Vector3.Distance(
                        solver.positions[i],
                        solver.positions[i + 1])
                        /
                Vector3.Distance(
                    solver.restPositions[i],
                    solver.restPositions[i + 1]);
            float leftStretchFactor =
                Vector3.Distance(
                        solver.positions[i],
                        solver.positions[i - 1])
                        /
                Vector3.Distance(
                    solver.restPositions[i],
                    solver.restPositions[i - 1]);
            if (rightStretchFactor > minimumStretchFactor || 
                leftStretchFactor > minimumStretchFactor && 
                solver.invMasses[i]!=0)
            {
                solver.invMasses[i] = 0;
                solver.velocities[i] = Vector4.zero;
                solver.angularVelocities[i] = Vector4.zero;
                solver.positions[i] = (solver.positions[i - 1] + solver.positions[i + 1]) / 2;
            }
            else
            {
                solver.invMasses[i] = oldInvMasses[i];
            }
        }
    }
}
