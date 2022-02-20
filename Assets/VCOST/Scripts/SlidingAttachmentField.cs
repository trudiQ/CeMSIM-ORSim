using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Obi;

[RequireComponent(typeof(SphereCollider))]
public class SlidingAttachmentField : ParticleTriggerField
{
    //public Text text;
    //public Text textTwo;
    public AnimationCurve influenceFalloffCurve;

    public float stretchSensitivity = 1f;

    private Vector3 fieldDirection;
    private float radius;

    public bool isAttaching = false;
    private int activiatingParticles = 8;

    public override void Start()
    {
        base.Start();

        timeScale = Time.fixedDeltaTime / solver.GetComponent<ObiFixedUpdater>().substeps;
        radius = GetComponent<SphereCollider>().radius;
        fieldDirection = transform.up;
    }

    protected override void OnParticlesEnterField(List<int> particles)
    {
        AddNewParticlesToParticleHistory(particles);
        if (isAttaching)
            StartManipulatingParticles(particles);
    }

    protected override void OnParticlesStayInField(List<int> particles)
    {
        if (isAttaching)
            ConstrainParticleVelocities(particles);
    }

    protected override void OnParticlesExitField(List<int> particles)
    {
        if (isAttaching)
            StopManipulatingParticles(particles);
    }

    float timeScale;
    private void ConstrainParticleVelocities(List<int> particles)
    {
        //Contained particles are near the end of the rod, abort!
        /*
        if (!(particles[0] > 0) || !(particles[particles.Count - 1] < solver.positions.count))
            return;
            */
        if (particles.Count == 0) return;

        if (particles.Count == 1)
        {
            int singularParticle = particles[0];

            solver.positions[singularParticle] = SolverPositionOfPoint(
                transform.position
                );

            Quaternion averageNeighborOrientation = Quaternion.Lerp(solver.orientations[singularParticle - 1], solver.orientations[singularParticle + 1], 0.5f);
            solver.orientations[singularParticle] = averageNeighborOrientation;
            return;
        }

        int leftCount = 0;
        int rightCount = 0;
        float leftStretch = 0;
        for (int p = particles[0]; p > 0; p--)
        {
            if (p != particles[0] && solver.invMasses[p] == 0) break;

            leftStretch += Vector3.Distance(
                WorldPositionOfParticle(p),
                WorldPositionOfParticle(p - 1));
            leftCount++;
            //Debug.DrawRay(WorldPositionOfParticle(p), solver.orientations[p].eulerAngles.normalized, Color.blue);
            //Debug.DrawRay(WorldPositionOfParticle(p), Vector3.up, new Color(0, (float)p/particles[0], (float)(particles[0]-p) / particles[0]));
        }
        if (leftCount == 0)
        {
            leftStretch = 1;
        }
        else
        {
            leftStretch /= leftCount;
            //leftStretch *= 2.25f;
        }

        float rightStretch = 0;
        for (int p = particles[particles.Count - 1]; p < solver.positions.count; p++)
        {
            if (p != particles[particles.Count - 1] && solver.invMasses[p] == 0) break;

            rightStretch += Vector3.Distance(
                WorldPositionOfParticle(p),
                WorldPositionOfParticle(p + 1));
            rightCount++;
            //Debug.DrawRay(WorldPositionOfParticle(p), solver.orientations[p].eulerAngles.normalized, Color.red);
            //Debug.DrawRay(WorldPositionOfParticle(p), Vector3.up, new Color((float)(solver.positions.count - particles[particles.Count - 1] - rightCount) / (solver.positions.count - particles[particles.Count - 1]), 0, (float)rightCount / (solver.positions.count - particles[particles.Count - 1])));
        }
        if (rightCount == 0)
        {
            rightStretch = 1;
        }
        else
        {
            rightStretch /= rightCount;
        }

        float stretchCoefficient = (leftStretch - rightStretch) * stretchSensitivity;
        //text.text = leftStretch + "\n" + rightStretch;
        //textTwo.text = leftCount + "\n" + rightCount;
        foreach (int p in particles)
        {
            //https://forum.unity.com/threads/how-do-i-find-the-closest-point-on-a-line.340058/
            Vector3 particlePosition = WorldPositionOfParticle(p);
            Vector3 closestPositionToCenterLine = transform.position + fieldDirection * Vector3.Dot(particlePosition - transform.position, fieldDirection);

            float fieldInfluenceMultiplier = influenceFalloffCurve.Evaluate(
                MathHelper.Map01(
                    Vector3.Distance(transform.position, particlePosition), 0, radius));

            /*
            solver.positions[p] = 
                SolverPositionOfPoint(
                    Vector3.Lerp(closestPositionToCenterLine, particlePosition, fieldInfluenceMultiplier)) + 
                    (timeScale * fieldDirection * stretchCoefficient);
              */

            Vector3 averageNeighborPosition = (WorldPositionOfParticle(p - 1) + WorldPositionOfParticle(p + 1)) / 2;

            solver.positions[p] = SolverPositionOfPoint(
                Vector3.Lerp(closestPositionToCenterLine, averageNeighborPosition, fieldInfluenceMultiplier) + (timeScale * fieldDirection * stretchCoefficient)
                );

            Quaternion averageNeighborOrientation = Quaternion.Lerp(solver.orientations[p - 1], solver.orientations[p + 1], 0.5f);
            solver.orientations[p] = averageNeighborOrientation;
            
        }
    }
}
