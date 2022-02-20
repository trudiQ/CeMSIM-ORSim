using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiSolver))]
public class ParticleTriggerFieldDispatcher : MonoBehaviour
{
    private ObiSolver solver;

    // Start is called before the first frame update
    void Start()
    {
        solver = gameObject.GetComponent<ObiSolver>();

        solver.OnCollision += PhaseZoneBehaviour;
    }

    private void PhaseZoneBehaviour(ObiSolver solver, ObiSolver.ObiCollisionEventArgs e)
    {
        var world = ObiColliderWorld.GetInstance();

        Dictionary<ParticleTriggerField, List<int>> triggerFields = new Dictionary<ParticleTriggerField, List<int>>();

        // just iterate over all contacts in the current frame:
        foreach (Oni.Contact contact in e.contacts)
        {
            // if this one is an actual collision:
            if (contact.distance < 0.01)
            {
                ObiColliderBase col = world.colliderHandles[contact.bodyB].owner;
                ParticleTriggerField particleTriggerField = col.gameObject.GetComponent<ParticleTriggerField>();

                if (particleTriggerField != null)
                {
                    int simplexStart = solver.simplexCounts.GetSimplexStartAndSize(contact.bodyA, out int simplexSize);
                    for (int i = 0; i < simplexSize; ++i)
                    {
                        int particleIndex = solver.simplices[simplexStart + i];
                        if (triggerFields.ContainsKey(particleTriggerField))
                        {
                            triggerFields[particleTriggerField].Add(particleIndex);
                        }
                        else
                        {
                            triggerFields.Add(particleTriggerField, new List<int>() { particleIndex });
                        }
                    }
                }
            }
        }

        foreach (KeyValuePair<ParticleTriggerField, List<int>> p in triggerFields)
        {
            p.Key.TriggerField(p.Value);
        }
    }
}
