using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class BlueprintParticleIndividualizer : MonoBehaviour
{
    public ObiSolver solver;
    public ObiSoftbody instance;
    public ObiSoftbodySkinner softbodySkinner;
    public ObiCollider activeColliderSnapshot;
    ObiSoftbodyBlueprintBase blueprint;
    public bool doDebugPrintout = false;

    private Matrix4x4 solver2World;
    /// <summary>
    /// Uses Matrices. Very cheap?
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Vector3 WorldPositionOfParticle(int p) { return solver2World.MultiplyPoint3x4(solver.positions[p]); }

    private List<int> currentlyUsedParticles;
    private void UpdateCurrentlyUsedParticles()
    {
        currentlyUsedParticles = new List<int>();

        ObiParticleAttachment[] obiParticleAttachments = GetComponents<ObiParticleAttachment>();
        foreach (ObiParticleAttachment particleAttachment in obiParticleAttachments)
        {
            if(particleAttachment.particleGroup!=null)
                currentlyUsedParticles.AddRange(particleAttachment.particleGroup.particleIndices);
        }
    }
    private bool IsParticleCurrentlyUsed(int particle)
    {
        if (currentlyUsedParticles.Contains(particle)) return true;
        return false;
    }

    private void Start()
    {
        if (instance == null) instance = GetComponent<ObiSoftbody>();
        if (solver == null) solver = GetComponentInParent<ObiSolver>();
        
        solver2World = solver.transform.localToWorldMatrix;

        instance = GetComponent<ObiSoftbody>();
        blueprint = (ObiSoftbodyBlueprintBase)instance.blueprint;

        UpdateCurrentlyUsedParticles();
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

    private ObiParticleGroup CreateGroupOfUnusedParticlesClosestTo(Vector3 position, int amountOfParticles)
    {
        List<int> foundParticles = new List<int>();
        for (int o = 0; o < amountOfParticles; o++)
        {
            float closestParticleDistance = float.MaxValue;
            int closestParticleInt = -1;
            Vector3 closestParticle = Vector3.zero;
            for (int i = 0; i < solver.positions.count; i++)
            {
                if (currentlyUsedParticles.Contains(i)) continue;
                if (foundParticles.Contains(i)) continue;

                float dist = Vector3.Distance(WorldPositionOfParticle(i), position);
                if (dist < closestParticleDistance)
                {
                    closestParticleDistance = dist;
                    closestParticleInt = i;
                }
            }
            foundParticles.Add(closestParticleInt);
        }

        ObiParticleGroup group = ScriptableObject.CreateInstance<ObiParticleGroup>();
        group.SetSourceBlueprint(blueprint);
        group.particleIndices = foundParticles;

        return group;
    }

    public ObiParticleAttachment CreateNewDynamicParticleAttachmentClosestTo(Transform t, int amountOfParticles) { return ParticleAttachmentCreator(t, amountOfParticles, true); }
    public ObiParticleAttachment CreateNewDynamicParticleAttachmentClosestTo(Transform t) { return ParticleAttachmentCreator(t, 1, true); }
    public ObiParticleAttachment CreateNewParticleAttachmentClosestTo(Transform t) { return ParticleAttachmentCreator(t, 1, false); }
    public ObiParticleAttachment CreateNewParticleAttachmentClosestTo(Transform t, bool dynamic) { return ParticleAttachmentCreator(t, 1, dynamic); }
    private ObiParticleAttachment ParticleAttachmentCreator(Transform t, int amount, bool dynamic)
    {
        //Important, other scripts may add particle attachments!
        UpdateCurrentlyUsedParticles();

        ObiParticleAttachment particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
        particleAttachment.target = t;
        particleAttachment.attachmentType = dynamic?ObiParticleAttachment.AttachmentType.Dynamic:ObiParticleAttachment.AttachmentType.Static;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;

        particleAttachment.particleGroup = CreateGroupOfUnusedParticlesClosestTo(t.position, amount);
        particleAttachment.enabled = true;

        return particleAttachment;
    }

    public ObiParticleGroup GetParticlesWithinCollider(ObiCollider collider)
    {
        return null;
    }

    /// <summary>
    /// Moves transform to the closest particle and creates a static particle attachment to that particle.
    /// </summary>
    /// <param name="t"></param>
    public ObiParticleAttachment MoveAndCreateParticleAttachmentTo(Transform t)
    {
        return MoveAndCreateParticleAttachmentTo(t, -1);
    }

    /// <summary>
    /// Move transform to particle and create a static particle attachment between them.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="particle"></param>
    public ObiParticleAttachment MoveAndCreateParticleAttachmentTo(Transform t, int particle)
    {
        //Important, other scripts may add particle attachments!
        UpdateCurrentlyUsedParticles();

        ObiParticleAttachment particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();

        ObiParticleGroup group;

        //If the closest particle is unknown
        if (particle == -1)
        {
            group = CreateGroupOfUnusedParticlesClosestTo(t.position, 1);
        }
        else
        {
            if (currentlyUsedParticles.Contains(particle))
            {
                Debug.LogError("Tried to move&create particle attachment to particle " + particle + " but it is already in use!");
                return null;
            }
            group = ScriptableObject.CreateInstance<ObiParticleGroup>();
            group.SetSourceBlueprint(blueprint);
            group.particleIndices = new List<int>() { particle };
        }

        particleAttachment.particleGroup = group;

        t.position = WorldPositionOfParticle(group.particleIndices[0]);
        particleAttachment.target = t;
        particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;
        particleAttachment.enabled = true;

        return particleAttachment;
    }

    public List<ObiParticleAttachment> CreateRingAttachment(Transform center, Transform ring, int centerAmount, int ringAmount)
    {
        //Important, other scripts may add particle attachments!
        UpdateCurrentlyUsedParticles();

        List<ObiParticleAttachment> obiParticleAttachments = new List<ObiParticleAttachment>();

        //First, make an attachment for the center particles
        ObiParticleAttachment particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
        particleAttachment.target = center;
        particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;
        
        particleAttachment.particleGroup = CreateGroupOfUnusedParticlesClosestTo(center.position, centerAmount);
        //Add the recently found particles to the used set as we're going to be excluding them from the ring search
        currentlyUsedParticles.AddRange(particleAttachment.particleGroup.particleIndices);
        particleAttachment.enabled = true;

        obiParticleAttachments.Add(particleAttachment);

        //Now again for the surrounding ring
        particleAttachment = gameObject.AddComponent<ObiParticleAttachment>();
        particleAttachment.target = ring;
        particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        particleAttachment.constrainOrientation = true;
        particleAttachment.compliance = 0;
        particleAttachment.breakThreshold = Mathf.Infinity;
        
        particleAttachment.particleGroup = CreateGroupOfUnusedParticlesClosestTo(center.position, ringAmount);
        currentlyUsedParticles.AddRange(particleAttachment.particleGroup.particleIndices);
        particleAttachment.enabled = true;

        obiParticleAttachments.Add(particleAttachment);

        return obiParticleAttachments;
    }

    public GameObject CreateColliderSnapshot(ObiCollider colliderReference)
    {
        GameObject colliderSnapshot = Instantiate(
                colliderReference.gameObject,
                Vector3.zero,
                Quaternion.identity,
                transform
                );

        ObiCollider obiCollider = colliderSnapshot.GetComponent<ObiCollider>();
        MeshCollider meshCollider = colliderSnapshot.GetComponent<MeshCollider>();
        SkinnedMeshRenderer meshRenderer = softbodySkinner.GetComponent<SkinnedMeshRenderer>();
        Mesh sMesh = new Mesh();
        meshRenderer.BakeMesh(sMesh);
        meshCollider.sharedMesh = sMesh;
        obiCollider.sourceCollider = meshCollider;
        colliderSnapshot.transform.position = meshRenderer.transform.position;
        colliderSnapshot.transform.rotation = meshRenderer.transform.rotation;

        activeColliderSnapshot = obiCollider;

        return colliderSnapshot;
    }
}
