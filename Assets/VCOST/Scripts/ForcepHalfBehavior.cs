using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcepHalfBehavior : MonoBehaviour
{
    public Transform grabbedTransform = null;

    public void RotateIfNotGrabbing(Vector3 rot)
    {
        if(grabbedTransform==null)
            transform.Rotate(rot);
    }
}
