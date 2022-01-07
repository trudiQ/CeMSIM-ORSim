using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiRod))]
public class RodStretchNormalizer : MonoBehaviour
{
    private ObiSolver solver;
    private ObiRod rod;

    private int total;
    private float stretchFactor = 1.5f;

    private ObiNativeFloatList oldInvMasses;

    int lowerParticleLimit = -1;
    int higherParticleLimit = -1;

    public void EnableAndLimitBetweenExclusively(int lower, int higher)
    {
        this.enabled = true;
        lowerParticleLimit = lower + 1;
        higherParticleLimit = higher - 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        solver = GetComponentInParent<ObiSolver>();
        rod = GetComponent<ObiRod>();

        total = solver.restPositions.count;

        oldInvMasses = new ObiNativeFloatList();
        foreach(float f in solver.invMasses)
        {
            oldInvMasses.Add(f);
        }
    }

    private void FixedUpdate()
    {
        for(int i = lowerParticleLimit+1; i < higherParticleLimit-1; i++)
        {
            float rDistDiff =
                Vector3.Distance(
                        solver.positions[i],
                        solver.positions[i + 1])
                        /
                Vector3.Distance(
                    solver.restPositions[i],
                    solver.restPositions[i + 1]);
            float lDistDiff =
                Vector3.Distance(
                        solver.positions[i],
                        solver.positions[i - 1])
                        /
                Vector3.Distance(
                    solver.restPositions[i],
                    solver.restPositions[i - 1]);
            if (rDistDiff > stretchFactor || lDistDiff > stretchFactor && solver.invMasses[i]!=0)
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
