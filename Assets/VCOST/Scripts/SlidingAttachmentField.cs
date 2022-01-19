using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingAttachmentField : ParticleTriggerField
{
    private Vector3 fieldDirection;

    public override void Start()
    {
        base.Start();

        fieldDirection = transform.up;
    }

    protected override void OnParticlesEnterField(List<int> particles)
    {
        ConstrainParticleVelocities(particles);
    }

    protected override void OnParticlesStayInField(List<int> particles)
    {
        ConstrainParticleVelocities(particles);
    }

    private void ConstrainParticleVelocities(List<int> particles)
    {
        foreach (int p in particles)
        {
            //https://forum.unity.com/threads/how-do-i-find-the-closest-point-on-a-line.340058/
            Vector3 closestPositionToCenterLine = transform.position + fieldDirection * Vector3.Dot(WorldPositionOfParticle(p) - transform.position, fieldDirection);
            Vector3 constrainingVelocity = closestPositionToCenterLine - WorldPositionOfParticle(p);
            solver.velocities[p] = Vector3.Project(solver.velocities[p], fieldDirection) + (constrainingVelocity * 5f);
            solver.angularVelocities[p] = Vector4.zero;
            solver.positions[p] = SolverPositionOfPoint(closestPositionToCenterLine);
        }
    }
}
