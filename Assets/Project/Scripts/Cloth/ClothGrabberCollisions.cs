using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is used to allow cloth grab colliders to fall into place on scene load.

public class ClothGrabberCollisions : MonoBehaviour
{
    private Rigidbody rb;
    private bool initialCollisionMade = false;      //used to ensure this only runs once
    private ClothGrabberDistanceLimitations limiter;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        limiter = this.GetComponent<ClothGrabberDistanceLimitations>();
    }
    void OnCollisionEnter(Collision other)
    {
        if(/*other.gameObject.tag == "PatientColliders" &&*/ !initialCollisionMade)
        {   
            rb.isKinematic = true;
            rb.useGravity = false;
            initialCollisionMade = true;
            limiter.PropagateRelease();
        }
    }
}
