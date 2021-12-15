using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class StaticHoleColliderBehavior : MonoBehaviour
{
    public bool inner = false;
    public bool disableCollisionsOnStart = false;

    public ObiParticleAttachment heldAttachment;
    private StaticHoleColliderManager parentManager;

    public List<ObiCollider> heldColliders = new List<ObiCollider>();

    public void DisableColliders()
    {
        foreach(ObiCollider o in heldColliders)
        {
            o.enabled = false;
        }
    }

    // Start is called before the first frame update
    /* No.
    void Start()
    {
        if (inner)
        {
            StaticHoleColliderManager.instance.AddInner(this);
            transform.parent = StaticHoleColliderManager.instance.transform;
        }
        else
        {
            StaticHoleColliderManager.instance.AddOuter(this, lagMult);
        }

        if (disableCollisionsOnStart)
            DisableColliders();
    }
    */

    public void AddToManager(StaticHoleColliderManager manager)
    {
        parentManager = manager;
        transform.parent = parentManager.transform;

        if (inner)
        {
            parentManager.AddInner(this);
        }
        else
        {
            parentManager.AddOuter(this);
        }

        if (disableCollisionsOnStart)
            DisableColliders();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Obi")
        {
            //parentManager._E_EndOfRodReached.Invoke();
        }
    }

    public void DisableUnityCollisions()
    {
        foreach (ObiCollider o in heldColliders)
        {
            o.gameObject.layer = 9;
        }
    }
}
