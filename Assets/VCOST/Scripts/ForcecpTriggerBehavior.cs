using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcecpTriggerBehavior : MonoBehaviour
{
    public ForcepHalfBehavior half;

    private void Start()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Needle" && other.isTrigger == false)
        {
            half.grabbedTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.transform == half.grabbedTransform)
            half.grabbedTransform = null;
    }
}
