using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JointController : MonoBehaviour
{
    public Rigidbody[] rbs;
    public List<Rigidbody> ignoreRigidbodies;
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
            Rigidbody[] childJoints = rb.GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody joint in childJoints)
            {
                if (!ignoreRigidbodies.Contains(joint))
                    joint.isKinematic = true;
            }
        }
    }
    public void EnableJoint(Rigidbody rb)
    {
        if(rb.isKinematic)
        {
            rb.isKinematic = false;
            Rigidbody[] childJoints = rb.GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody joint in childJoints)
            {
                if(!ignoreRigidbodies.Contains(joint))
                    joint.isKinematic = false;
            }
        }
    }
    public void ToggleJoints(bool state)
    {
        foreach(Rigidbody rb in rbs)
        {
            if (!ignoreRigidbodies.Contains(rb))
                rb.isKinematic = state;
        }
    }

    IEnumerator DisableJointsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.75f);
        ToggleJoints(true);
    }
}
