using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiSolver))]
public class SuturePhaseZone : MonoBehaviour
{
    private ObiSolver solver;

    private int defaultPhase = 0;
    private int excludePseudoColonPhase = 0;

    private List<int> previousContacts = new List<int>();

    private string PrintListPretty(List<int> l)
    {
        if (l.Count == 0) return "None";
        string m = "";
        foreach (int i in l)
        {
            m += i + ", ";
        }
        return m.Substring(0, m.Length - 2);
    }

    // Start is called before the first frame update
    void Start()
    {
        solver = gameObject.GetComponent<ObiSolver>();

        defaultPhase = ObiUtils.MakeFilter(0x0000fffe, 0);
        excludePseudoColonPhase = ObiUtils.MakeFilter(0x0000fffa, 0);

        solver.OnCollision += PhaseZoneBehaviour;
    }

    private void PhaseZoneBehaviour(ObiSolver solver, ObiSolver.ObiCollisionEventArgs e)
    {
        var world = ObiColliderWorld.GetInstance();

        List<int> currentContacts = new List<int>();

        // just iterate over all contacts in the current frame:
        foreach (Oni.Contact contact in e.contacts)
        {
            // if this one is an actual collision:
            if (contact.distance < 0.01)
            {
                ObiColliderBase col = world.colliderHandles[contact.bodyB].owner;
                if (col != null && col.gameObject.CompareTag("phaseZone"))
                {
                    int particleIndex = solver.simplices[contact.bodyA];

                    // do something with each particle, for instance get its position:
                    //Debug.Log(particleIndex + " Entered");
                    solver.filters[particleIndex] = excludePseudoColonPhase;

                    currentContacts.Add(particleIndex);
                }
            }
        }

        //Reenable collisions on the particles that just left the phase zone
        foreach(int i in previousContacts)
        {
            if (!currentContacts.Contains(i))
            {
                solver.filters[i] = defaultPhase;
            }
        }

        previousContacts = currentContacts;
    }

    public void PhaseZoneEnter(ObiSolver solver, Oni.Contact contact)
    {
        var world = ObiColliderWorld.GetInstance();

        ObiColliderBase col = world.colliderHandles[contact.bodyB].owner;

        Debug.Log(solver.simplices[contact.bodyA] + "..." + solver.simplices[contact.bodyB]);

        if (col != null && col.gameObject.CompareTag("phaseZone"))
        {

            // retrieve the offset and size of the simplex in the solver.simplices array:
            int simplexStart = solver.simplexCounts.GetSimplexStartAndSize(contact.bodyA, out int simplexSize);

            // starting at simplexStart, iterate over all particles in the simplex:
            for (int i = 0; i < simplexSize; ++i)
            {
                int particleIndex = solver.simplices[simplexStart + i];

                // do something with each particle, for instance get its position:
                Debug.Log(particleIndex + " Entered");
                solver.filters[particleIndex] = excludePseudoColonPhase;
            }
        }
    }

    public void PhaseZoneExit(ObiSolver solver, Oni.Contact contact)
    {
        var world = ObiColliderWorld.GetInstance();

        ObiColliderBase col = world.colliderHandles[contact.bodyB].owner;

        int particleIndex = solver.simplices[contact.bodyA];

        if (col != null && col.gameObject.CompareTag("phaseZone"))
        {
            // retrieve the offset and size of the simplex in the solver.simplices array:
            int simplexStart = solver.simplexCounts.GetSimplexStartAndSize(contact.bodyA, out int simplexSize);

            // starting at simplexStart, iterate over all particles in the simplex:
            for (int i = 0; i < simplexSize; ++i)
            {
                //int particleIndex = solver.simplices[simplexStart + i];

                // do something with each particle, for instance get its position:
                Debug.Log(particleIndex + " Exited");
                solver.filters[particleIndex] = defaultPhase;
            }
        }
    }
}
