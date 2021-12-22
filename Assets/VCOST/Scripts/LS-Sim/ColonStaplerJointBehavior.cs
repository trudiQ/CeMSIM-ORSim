using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonStaplerJointBehavior : MonoBehaviour
{


    public Rigidbody targetSphere;
    public FixedJoint jointToSphere;
    public Transform followedStaplerStart;
    public Transform followedStaplerEnd;
    public float detachDistance;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnchorPosition();
    }

    public void AttachColonSphere()
    {
        jointToSphere.connectedBody = targetSphere;
    }

    public void DetachColonSphere()
    {
        jointToSphere.connectedBody = null;
    }

    public void UpdateAnchorPosition()
    {
        Ray staplerRay = new Ray(followedStaplerEnd.position, followedStaplerStart.position - followedStaplerEnd.position);
        Vector3 newPos = MathUtil.ProjectionPointOnLine(staplerRay, targetSphere.transform.position);
        if (Vector3.Distance(targetSphere.transform.position, newPos) < Vector3.Distance(targetSphere.transform.position, transform.position))
        {
            transform.position = newPos;
        }
        else if (Vector3.Distance(targetSphere.transform.position, newPos) > detachDistance)
        {
            DetachColonSphere();
            gameObject.SetActive(false);
        }
    }
}
