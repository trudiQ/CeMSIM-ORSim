using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using Utility;

namespace Utility
{
    public enum Direction
    {
        X, Y, Z, nX, nY, nZ
    }
}

public class BlueprintParticleIndividualizer : MonoBehaviour
{
    public ObiSolver solver;
    public skeleton_withRotation attachPointHolder;
    public GameObject p_attachPoint;
    public GameObject p_attachPointChildren;
    public float surfaceRange = 0.1f;
    public float bufferDistance = 0.1f;
    public Direction surfaceDirection = Direction.X;
    public Direction layerDirection = Direction.X;
    public int layers = 2;
    public float layerOffset = 1;
    public float distanceLimit = 1;

    struct PositionAndGroup
    {
        public Vector3 position;
        public ObiParticleGroup group;
    }

    private Vector3 EvaluateDirection(Direction direction)
    {
        Vector3 start = Vector3.zero;
        switch (direction)
        {
            case Direction.X:
                start += Vector3.right;
                break;
            case Direction.nX:
                start += Vector3.left;
                break;
            case Direction.Y:
                start += Vector3.up;
                break;
            case Direction.nY:
                start += Vector3.down;
                break;
            case Direction.Z:
                start += Vector3.forward;
                break;
            case Direction.nZ:
                start += Vector3.back;
                break;
        }
        return start;
    }

    private void Start()
    {
        ObiSoftbody softbody = GetComponent<ObiSoftbody>();
        ObiSoftbodyBlueprintBase blueprint = (ObiSoftbodyBlueprintBase)softbody.blueprint;
        Dictionary<int, PositionAndGroup> particleData = new Dictionary<int, PositionAndGroup>();
        int i = 0;
        int particles = blueprint.particleCount;
        while (i < particles)
        {
            ObiParticleGroup group = blueprint.AppendNewParticleGroup("group" + i.ToString());
            group.particleIndices = new List<int> { i };

            int solverIndex = softbody.solverIndices[i];
            Vector3 position = solver.restPositions[solverIndex];

            PositionAndGroup pg = new PositionAndGroup() { position = position, group = group };

            particleData.Add(solverIndex, pg);

            i++;
        }
        softbody.UnloadBlueprint(solver);
        softbody.softbodyBlueprint = blueprint;

        //Loop through particle data
        //Find the highest in Y
        //Make attachment points for each

        Vector3 start = EvaluateDirection(surfaceDirection);

        //Find highest Y value
        float highestValue = float.MinValue;
        foreach (KeyValuePair<int, PositionAndGroup> pair in particleData)
        {
            Vector3 adjusted = Vector3.Scale(pair.Value.position, start);
            float value = Vector3.Scale(pair.Value.position, start).magnitude;
            if (adjusted.x < 0 || adjusted.y < 0 || adjusted.z < 0) value *= -1;
            if (value > highestValue) highestValue = value;
        }
        

        //Add particles in range to list of desired
        List<KeyValuePair<int, PositionAndGroup>> particlesWithinDist = new List<KeyValuePair<int, PositionAndGroup>>();
        foreach (KeyValuePair<int, PositionAndGroup> pair in particleData)
        {
            Vector3 adjusted = Vector3.Scale(pair.Value.position, start);
            float value = Vector3.Scale(pair.Value.position, start).magnitude;
            if (adjusted.x < 0 || adjusted.y < 0 || adjusted.z < 0) value *= -1;
            if (value > highestValue - surfaceRange)
            {
                particlesWithinDist.Add(pair);
            }
        }

        //Trim particles that are too close to each other to get final list of desired
        List<KeyValuePair<int, PositionAndGroup>> desiredParticles = new List<KeyValuePair<int, PositionAndGroup>>();
        desiredParticles.Add(particlesWithinDist[0]);
        foreach(KeyValuePair<int, PositionAndGroup> pair in particlesWithinDist)
        {
            if (Vector3.Distance(desiredParticles[0].Value.position, pair.Value.position) > distanceLimit) continue;

            bool desired = true;

            foreach(KeyValuePair<int, PositionAndGroup> desiredPair in desiredParticles)
            {
                if(Vector3.Distance(pair.Value.position, desiredPair.Value.position) < bufferDistance)
                {
                    desired = false;
                    break;
                }
            }

            if (desired) desiredParticles.Add(pair);
        }

        Vector3 layerVector = EvaluateDirection(layerDirection);

        //Make and attach each attachment point
        foreach (KeyValuePair<int, PositionAndGroup> pair in desiredParticles) {
            Transform firstLayerParent = transform;
            for (int y = 0; y < layers; y++)
            {
                GameObject attachPoint;
                if (y == 0)
                {
                    attachPoint = Instantiate(p_attachPoint, 
                        transform.TransformPoint(pair.Value.position) + (layerVector * layerOffset * y), 
                        transform.localRotation, 
                        attachPointHolder.transform);
                    firstLayerParent = attachPoint.transform;
                    ObiParticleAttachment pa = gameObject.AddComponent<ObiParticleAttachment>();
                    pa.target = attachPoint.transform;
                    pa.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
                    pa.constrainOrientation = true;
                    pa.compliance = 0;
                    pa.breakThreshold = Mathf.Infinity;
                    pa.particleGroup = pair.Value.group;
                }
                else
                {
                    attachPoint = Instantiate(p_attachPointChildren, 
                        transform.TransformPoint(pair.Value.position) + (layerVector * layerOffset * y), 
                        transform.localRotation, 
                        firstLayerParent);
                }
            }
        }

        attachPointHolder.Go();
        softbody.LoadBlueprint(solver);
    }

    IEnumerator DelayedPrintout()
    {
        yield return new WaitForSeconds(1f);
        ObiSoftbody softbody = GetComponent<ObiSoftbody>();
        ObiSoftbodyBlueprintBase blueprint = softbody.softbodyBlueprint;
        List<ObiParticleGroup> groups = blueprint.groups;
        Debug.Log(groups.Count);
        foreach(ObiParticleGroup g in groups)
        {
            Debug.Log(g.name + ", " + g.particleIndices[0]);
        }
    }
}
