using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class RodBuncher : MonoBehaviour
{
    private ObiSolver solver;
    private ObiRod rod;

    private List<ObiParticleAttachment> bunches = new List<ObiParticleAttachment>();
    private List<int> initialBunchParticles = new List<int>();
    private List<int> currentCrawlingBuncher = new List<int>();

    public float particlesPerDistanceUnit = 10f;

    public void EnableAndDoScanFrom(List<ObiParticleAttachment> bunchSeeds)
    {
        this.enabled = true;

        solver = GetComponentInParent<ObiSolver>();
        rod = GetComponent<ObiRod>();
        
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
            /*
            float currentDistance = Vector3.Distance(bunches[i].target.position, bunches[i + 1].target.position);
            float currentSlack = (initialBunchParticles[i + 1] - currentCrawlingBuncher[i]) / currentDistance;
            float currentExcess = currentSlack / particlesPerDistanceUnit;

            //Debug.Log(currentSlack);

            if (currentExcess > 1)
            {
                if (currentCrawlingBuncher[i] + 1 > initialBunchParticles[i + 1]) continue;

                currentCrawlingBuncher[i] += 1;
                int particleToAdd = currentCrawlingBuncher[i];

                solver.positions[particleToAdd] = solver.positions[initialBunchParticles[i]];
                solver.invMasses[particleToAdd] = -1;

                bunches[i].particleGroup.particleIndices.Add(particleToAdd);

                currentDistance = Vector3.Distance(bunches[i].transform.position, bunches[i + 1].transform.position);
                currentSlack = (initialBunchParticles[i + 1] - currentCrawlingBuncher[i]) / particlesPerDistanceUnit;
            }
            */
            
            //Set non-bunched particles to be in a line
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
