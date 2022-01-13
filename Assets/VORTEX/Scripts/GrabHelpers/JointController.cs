using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointController : MonoBehaviour
{
    public Rigidbody[] rbs;
    // Start is called before the first frame update
    void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        StartCoroutine(DisableJointsAfterDelay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisableJoint(Rigidbody rb)
    {
        if(!rb.isKinematic && rb.GetComponent<ConfigurableJoint>().connectedBody.isKinematic)
        {
            rb.isKinematic = true;
            // rb.GetComponent<ConfigurableJoint>().connectedBody.isKinematic = true;
            Rigidbody[] childJoints = rb.GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody joint in childJoints)
            {
                joint.isKinematic = true;
            }
        }
    }
    public void EnableJoint(Rigidbody rb)
    {
        if(rb.isKinematic)
        {
            rb.isKinematic = false;
            // rb.GetComponent<ConfigurableJoint>().connectedBody.isKinematic = false;
            Rigidbody[] childJoints = rb.GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody joint in childJoints)
            {
                joint.isKinematic = false;
            }
        }
    }
    public void ToggleJoints(bool state)
    {
        foreach(Rigidbody rb in rbs)
        {
            rb.isKinematic = state;
        }
    }

    IEnumerator DisableJointsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.75f);
        ToggleJoints(true);
    }
}
