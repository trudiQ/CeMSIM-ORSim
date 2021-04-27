using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabHelper : MonoBehaviour
{
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
