using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using HurricaneVR.Framework.Components;

public class GrabHelper : MonoBehaviour
{
    public Rigidbody rb;
    public Collider[] colliders;

    public GameObject otherHVRGrabbableGO;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IgnorePlayerCollision(bool doit)
    {
        
        if(doit)
        {
            foreach(Collider col in colliders)
            {
                col.gameObject.layer = 22;
            }
        }
        else
        {
            foreach(Collider col in colliders)
            {
                col.gameObject.layer = 12;
            }
        }
        
        
    }

    // public void EnableOther()
    // {
    //     otherHVRGrabbable.GetComponent<HVRGrabbable>().enabled = true;
    // }
    // public void DisableOther()
    // {
    //     otherHVRGrabbable.GetComponent<HVRGrabbable>().enabled = false;
    // }

    public void SetOtherKinematic(bool state)
    {
        otherHVRGrabbableGO.GetComponent<Rigidbody>().isKinematic = state;
        otherHVRGrabbableGO.GetComponent<Rigidbody>().useGravity = !state;
    }

    public void SetKinematic(bool state)
    {
        rb.isKinematic = state;
    }

    public void SetWeight(int weight)
    {
        rb.mass = weight;
    }

    public void LockRB()
    {
        
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }

    public void UnlockRB()
    {
        rb.constraints = RigidbodyConstraints.None;
        
    }
}
