using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class RodBuncher : MonoBehaviour
{
    private ObiSolver solver;
    private RodBlueprintParticleIndividualizer rod;

    private List<ObiParticleAttachment> bunches = new List<ObiParticleAttachment>();
    private List<int> initialBunchParticles = new List<int>();
    private List<int> currentCrawlingBuncher = new List<int>();

    private int lastSeedLeadOffAmount = 4;

    public List<ObiParticleAttachment> heldAttachments = new List<ObiParticleAttachment>();

    public void BunchWithParticleAttachments(List<ObiParticleAttachment> seeds, List<GameObject> targets)
    {
        solver = GetComponentInParent<ObiSolver>();
        rod = GetComponent<RodBlueprintParticleIndividualizer>();

        int initialParticle = seeds[0].particleGroup.particleIndices[0];
        int finalParticle = seeds[seeds.Count - 1].particleGroup.particleIndices[0];
        
        for(int i = initialParticle; i < finalParticle; i++)
        {
            int closestTarget = -1;
            float shortestDistance = float.MaxValue;
            for(int t = 0; t < targets.Count; t++)
            {
                float foundDistance = Vector3.Distance(rod.WorldPositionOfParticle(i), targets[t].transform.position);
                if (foundDistance < shortestDistance)
                {
                    shortestDistance = foundDistance;
                    closestTarget = t;
                }
            }

            ObiParticleAttachment particleAttachment = rod.CreateParticleAttachment(i, targets[closestTarget].transform);
            heldAttachments.Add(particleAttachment);
        }
    }

    public void EnableAndDoScanFrom(List<ObiParticleAttachment> bunchSeeds)
    {
        this.enabled = true;

        solver = GetComponentInParent<ObiSolver>();
        rod = GetComponent<RodBlueprintParticleIndividualizer>();
        
        foreach(ObiParticleAttachment p in bunchSeeds)
        {
            bunches.Add(p);
        }
        
        for(int i = 0; i < bunchSeeds.Count-1; i++)
        {
            int currentBunchParticle = bunchSeeds[i].particleGroup.particleIndices[0];
            int nextBunchParticle = bunchSeeds[i+1].particleGroup.particleIndices[0];
            currentCrawlingBuncher.Add(currentBunchParticle);
        }

        for(int i = 0; i < bunchSeeds.Count; i++)
        {
            initialBunchParticles.Add(bunchSeeds[i].particleGroup.particleIndices[0]);
        }
    }

    private void FixedUpdate()
    {
        for(int i = 0; i < bunches.Count - 1; i++)
        {
            int lineFrom = currentCrawlingBuncher[i] + 1;
            int lineTo = initialBunchParticles[i + 1] - 1;
            for(int x = lineFrom; x <= lineTo; x++)
            {
                solver.invMasses[x] = -1;
                solver.positions[x] = 
                    Vector3.Lerp(
                    solver.positions[currentCrawlingBuncher[i]], 
                    solver.positions[initialBunchParticles[i+1]], 
                    (float)(x-lineFrom)/(lineTo-lineFrom));
            }
        }
    }
}
