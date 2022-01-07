using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class ContactEventChecker : MonoBehaviour
{
    private ObiColliderWorld colliderWorld;

    private void Start()
    {
        colliderWorld = ObiColliderWorld.GetInstance();
    }

    public void EnterContactChecker(ObiSolver solver, Oni.Contact contact)
    {
        GameObject ownerB = colliderWorld.colliderHandles[contact.bodyB].owner.gameObject;
        TriggerSoftbody triggerSoftbody = ownerB.GetComponent<TriggerSoftbody>();
        int particleIndex = solver.simplices[contact.bodyA];
        if (triggerSoftbody!= null)triggerSoftbody.TriggerEnterBehavior(contact, particleIndex);
    }

    public void ExitContactChecker(ObiSolver solver, Oni.Contact contact)
    {
        if (contact.bodyB > colliderWorld.colliderHandles.Count - 1) return;
        GameObject ownerB = colliderWorld.colliderHandles[contact.bodyB].owner.gameObject;
        TriggerSoftbody triggerSoftbody = ownerB.GetComponent<TriggerSoftbody>();
        if (triggerSoftbody != null) triggerSoftbody.TriggerExitBehavior();
    }
}
