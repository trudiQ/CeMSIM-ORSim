using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Intended to prevent the gloves from being grabbed by the hand it is equipped to
public class IgnoreHandGrab : MonoBehaviour
{
    public Collider[] grabColliders;

    private Collider[] colliders;

    void Start()
    {
        colliders = GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            foreach (Collider other in grabColliders)
            {
                Physics.IgnoreCollision(collider, other);
            }
        }
    }
}
