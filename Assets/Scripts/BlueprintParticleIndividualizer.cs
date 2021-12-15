using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class BlueprintParticleIndividualizer : MonoBehaviour
{
    public ObiSolver solver;
    public ObiSoftbody instance;
    public bool doDebugPrintout = false;

    private Matrix4x4 solver2World;
    /// <summary>
    /// Uses Matrices. Very cheap?
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Vector3 WorldPositionOfParticle(int p) { return solver2World.MultiplyPoint3x4(solver.positions[p]); }

    private List<int> validParticles;
    private List<int> invalidParticles;

    private void SetUpValidInvalidParticles()
    {
        validParticles = new List<int>();
        invalidParticles = new List<int>();

        List<ObiParticleGroup> groups = instance.blueprint.groups;
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

    private void Start()
    {
        if (instance == null) instance = GetComponent<ObiSoftbody>();
        if (solver == null) solver = GetComponentInParent<ObiSolver>();

        SetUpValidInvalidParticles();
        solver2World = solver.transform.localToWorldMatrix;
        instance = GetComponent<ObiSoftbody>();
        ObiSoftbodyBlueprintBase blueprint = (ObiSoftbodyBlueprintBase)instance.blueprint;
        int i = 0;
        int particles = blueprint.particleCount;
        while (i < particles)
        {
            ObiParticleGroup g = blueprint.AppendNewParticleGroup("group" + i.ToString());
            g.particleIndices = new List<int> { i };
            i++;
        }
        instance.UnloadBlueprint(solver);
        instance.softbodyBlueprint = blueprint;
        instance.LoadBlueprint(solver);
        if(doDebugPrintout)StartCoroutine(DelayedPrintout());
    }

    IEnumerator DelayedPrintout()
    {
        yield return new WaitForSeconds(1f);
        ObiSoftbody instance = GetComponent<ObiSoftbody>();
        ObiSoftbodyBlueprintBase blueprint = instance.softbodyBlueprint;
        List<ObiParticleGroup> groups = blueprint.groups;
        Debug.Log(groups.Count);
        foreach(ObiParticleGroup g in groups)
        {
            Debug.Log(g.name + ", " + g.particleIndices[0]);
        }
    }

    public float GetDistanceToClosestParticle(Vector3 position)
    {
        return Vector3.Distance(position, GetClosestParticlePosition(position));
    }

    public Vector3 GetClosestParticlePosition(Vector3 position)
    {
        float closestParticleDistance = float.MaxValue;
        int closestParticleInt = -1;
        for (int i = 0; i < solver.positions.count; i++)
        {
            float dist = Vector3.Distance(WorldPositionOfParticle(i), position);
            if (dist < closestParticleDistance)
            {
                closestParticleDistance = dist;
                closestParticleInt = i;
            }
        }

        return WorldPositionOfParticle(closestParticleInt);
    }

    /// <summary>
    /// Finds the closest particle to a position, and from that distance returns any particles within fuzz range
    /// </summary>
    /// <param name="position"></param>
    /// <param name="fuzzDistance"></param>
    /// <returns></returns>
    public List<int> GetClosestRestingSliceOfParticles(Vector3 position, float fuzzDistance, bool useLocal)
    {
        List<int> foundSlice = new List<int>();

        //Get the closest particle
        float closestParticleDistance = float.MaxValue;
        int closestParticleInt = -1;
        for (int i = 0; i < solver.positions.count; i++)
        {
            float dist = float.MaxValue;
            if(useLocal)
            {
                dist = Vector3.Distance(solver.positions[i], position);
            }
            else
            {
                dist = Vector3.Distance(WorldPositionOfParticle(i), position);
            }

            if (dist < closestParticleDistance)
            {
                closestParticleDistance = dist;
                closestParticleInt = i;
            }
        }

        //Get the fuzz particles from resting
        for (int i = 0; i < solver.restPositions.count; i++)
        {
            float dist = Mathf.Abs(solver.restPositions[i].x - solver.restPositions[closestParticleInt].x);

            if (dist < fuzzDistance)
            {
                foundSlice.Add(i);
            }
        }
        return foundSlice;
    }

    private ObiParticleGroup GetGroupWithParticle(int particle)
    {
        List<ObiParticleGroup> groups = instance.blueprint.groups;
        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i].ContainsParticle(particle))
            {
                return groups[i];
            }
        }
        return null;
    }

    private ObiParticleGroup GetClosestValidGroupToParticle(int particle, List<int> forbiddenParticles, out int foundParticle)
    {
        float closestParticleDistance = float.MaxValue;
        int closestParticleInt = -1;
        Vector3 closestParticle = Vector3.zero;
        for (int i = 0; i < solver.positions.count; i++)
        {
            if (invalidParticles.Contains(i)) continue;
            if (forbiddenParticles.Contains(i)) continue;

            float dist = Vector3.Distance(solver.restPositions[i], solver.restPositions[particle]);
            if (dist < closestParticleDistance)
            {
                closestParticleDistance = dist;
                closestParticleInt = i;
            }
        }

        foundParticle = closestParticleInt;

        return GetGroupWithParticle(closestParticleInt);
    }

    private ObiParticleGroup GetClosestValidParticleGroup(Vector3 position, out int particleIndex)
    {
        float closestParticleDistance = float.MaxValue;
        int closestParticleInt = -1;
        Vector3 closestParticle = Vector3.zero;
        for (int i = 0; i < solver.positions.count; i++)
        {
            if (invalidParticles.Contains(i)) continue;

            float dist = Vector3.Distance(WorldPositionOfParticle(i), position);
            if (dist < closestParticleDistance)
            {
                closestParticleDistance = dist;
                closestParticleInt = i;
            }
        }

        particleIndex = closestParticleInt;

        return GetGroupWithParticle(closestParticleInt);
    }

    public ObiParticleAttachment CreateNewDynamicParticleAttachmentClosestTo(Transform t) { return ParticleAttachmentCreator(t, true); }
    public ObiParticleAttachment CreateNewParticleAttachmentClosestTo(Transform t) { return ParticleAttachmentCreator(t, false); }
    private ObiParticleAttachment ParticleAttachmentCreator(Transform t, bool dynamic)
    {
        ObiParticleAttachment particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
        particleAttachment.target = t;
        particleAttachment.attachmentType = dynamic?ObiParticleAttachment.AttachmentType.Dynamic:ObiParticleAttachment.AttachmentType.Static;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;


        particleAttachment.particleGroup = GetClosestValidParticleGroup(t.position, out int foundParticle);
        particleAttachment.enabled = true;

        return particleAttachment;
    }

    public void MoveAndCreateParticleAttachmentTo(Transform t, int particle)
    {
        ObiParticleAttachment particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
        particleAttachment.particleGroup = GetGroupWithParticle(particle);
        t.position = WorldPositionOfParticle(particle);
        particleAttachment.target = t;
        particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;
        particleAttachment.enabled = true;
    }

    public void MoveAndCreateParticleAttachmentTo(Transform t)
    {
        ObiParticleAttachment particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
        particleAttachment.particleGroup = GetClosestValidParticleGroup(t.position, out int foundParticle);
        t.position = WorldPositionOfParticle(foundParticle);
        particleAttachment.target = t;
        particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;
        particleAttachment.enabled = true;
    }

    public List<ObiParticleAttachment> CreateRingAttachment(Transform center, Transform ring, int centerAmount, int ringAmount)
    {
        List<ObiParticleAttachment> obiParticleAttachments = new List<ObiParticleAttachment>();

        ObiParticleAttachment particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
        particleAttachment.target = center;
        particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;

        int centerParticle = -1;
        particleAttachment.particleGroup = GetClosestValidParticleGroup(center.position, out centerParticle);
        particleAttachment.enabled = true;

        obiParticleAttachments.Add(particleAttachment);

        List<int> foundParticles = new List<int>();
        foundParticles.Add(centerParticle);

        for (int i = 0; i < centerAmount-1; i++)
        {
            particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
            particleAttachment.target = center;
            particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
            particleAttachment.constrainOrientation = true;
            particleAttachment.compliance = 0;
            particleAttachment.breakThreshold = Mathf.Infinity;

            int ringParticle = -1;
            particleAttachment.particleGroup = GetClosestValidGroupToParticle(centerParticle, foundParticles, out ringParticle);
            foundParticles.Add(ringParticle);
            particleAttachment.enabled = true;

            obiParticleAttachments.Add(particleAttachment);
        }

        for (int i = 0; i < ringAmount; i++)
        {
            particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
            particleAttachment.target = ring;
            particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
            particleAttachment.constrainOrientation = true;
            particleAttachment.compliance = 0;
            particleAttachment.breakThreshold = Mathf.Infinity;

            int ringParticle = -1;
            particleAttachment.particleGroup = GetClosestValidGroupToParticle(centerParticle, foundParticles, out ringParticle);
            foundParticles.Add(ringParticle);
            particleAttachment.enabled = true;

            obiParticleAttachments.Add(particleAttachment);
        }
        return obiParticleAttachments;
    }
}
