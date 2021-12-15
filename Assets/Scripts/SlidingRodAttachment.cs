using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class SlidingRodAttachment : MonoBehaviour
{
    public ObiSolver solver;
    public ObiRod attachedRod;
    ObiParticleAttachment particleAttachment;

    public float minimumDistanceToSlide = 0.01f;

    private int particlesToAttach = 3;
    //private List<int> attachedParticles;
    public int attachedParticle = -1;
    private static List<int> validParticles;
    public static List<int> invalidParticles;
    public static List<int> totalAttachedParticles;

    private Matrix4x4 solver2Attachment;
    private Matrix4x4 attachment2Solver;
    private Matrix4x4 solver2World;

    private void SetUpValidInvalidParticles()
    {
        validParticles = new List<int>();
        invalidParticles = new List<int>();
        totalAttachedParticles = new List<int>();

        List<ObiParticleGroup> groups = attachedRod.blueprint.groups;
        for (int i = 0; i < groups.Count; i++)
        {
            List<int> particles = groups[i].particleIndices;
            foreach (int p in particles)
            {
                if (validParticles.Contains(p))
                {
                    if (invalidParticles.Contains(p)) continue;
                    invalidParticles.Add(p);
                }
                else
                {
                    validParticles.Add(p);
                }
            }
        }

        foreach (int p in invalidParticles)
        {
            validParticles.Remove(p);
        }
    }

    private bool ParticleCanAttach(int p)
    {
        if (invalidParticles.Contains(p) || totalAttachedParticles.Contains(p)) return false;
        return true;
    }

    public void Init(ObiSolver s, ObiRod r)
    {
        solver = s;
        attachedRod = r;

        if (validParticles == null) SetUpValidInvalidParticles();

        solver2Attachment = transform.worldToLocalMatrix * solver.transform.localToWorldMatrix;
        attachment2Solver = solver2Attachment.inverse;
        solver2World = solver.transform.localToWorldMatrix;

        float smallestMagnitude = float.MaxValue;
        int particleCount = solver.positions.count;
        for(int i = 0; i < particleCount; i++)
        {
            if (!ParticleCanAttach(i)) continue;
            float mag = Vector3.Magnitude(solver2Attachment.MultiplyPoint3x4(solver.positions[i]));
            if (mag < smallestMagnitude)
            {
                smallestMagnitude = mag;
                attachedParticle = i;
            }
        }
        totalAttachedParticles.Add(attachedParticle);

        particleAttachment = attachedRod.gameObject.AddComponent<ObiParticleAttachment>();
        particleAttachment.target = transform;
        particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;


        particleAttachment.particleGroup = GetGroupWithParticle(attachedParticle);
        particleAttachment.enabled = true;
    }

    private Vector3 WorldPositionOfParticle(int p) { return solver2World.MultiplyPoint3x4(solver.positions[p]); }

    private void CheckAndDoSlide()
    {
        //Check forward and backward positions
        Vector3 forwardPos;
        Vector3 backwardPos;
        Vector3 thisPos = WorldPositionOfParticle(attachedParticle);
        if(attachedParticle+1 == solver.positions.count)
        {
            forwardPos = WorldPositionOfParticle(attachedParticle);
        }
        else
        {
            forwardPos = WorldPositionOfParticle(attachedParticle+1);
        }

        if (attachedParticle - 1 < 0)
        {
            backwardPos = WorldPositionOfParticle(attachedParticle);
        }
        else
        {
            backwardPos = WorldPositionOfParticle(attachedParticle-1);
        }

        float dir = Vector3.Distance(forwardPos, thisPos) - Vector3.Distance(backwardPos, thisPos);
        if (Mathf.Abs(dir) > minimumDistanceToSlide)
        {
            if (dir > 0 && ParticleCanAttach(attachedParticle+1))
            {
                totalAttachedParticles.Remove(attachedParticle);
                ++attachedParticle;
                totalAttachedParticles.Add(attachedParticle);
                particleAttachment.particleGroup = GetGroupWithParticle(attachedParticle);
                solver.positions[attachedParticle] = transform.position;
                Debug.Log("Slid to " + attachedParticle);
            }
            else if(dir < 0 && ParticleCanAttach(attachedParticle-1))
            {
                totalAttachedParticles.Remove(attachedParticle);
                --attachedParticle;
                totalAttachedParticles.Add(attachedParticle);
                particleAttachment.particleGroup = GetGroupWithParticle(attachedParticle);
                solver.positions[attachedParticle] = transform.position;
                Debug.Log("Slid to " + attachedParticle);
            }
        }
        solver.positions[attachedParticle] = transform.position;
    }

    private ObiParticleGroup GetGroupWithParticle(int particle)
    {
        List<ObiParticleGroup> groups = attachedRod.blueprint.groups;
        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i].ContainsParticle(particle) && groups[i].name.Contains("group"))
            {
                return groups[i];
            }
        }
        return null;
    }

    private void FixedUpdate()
    {
        if (particleAttachment != null) CheckAndDoSlide();
    }
}
