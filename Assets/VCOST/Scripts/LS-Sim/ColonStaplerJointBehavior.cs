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
    public Ray followedRay;
    public List<Transform> anchorForNeighborSpheres;
    public int targetSphereLayer;
    public int targetSphereColon;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
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
        ColonStaplerJointManager.instance.activeAnchors.Remove(this);
        gameObject.SetActive(false);
    }

    public void UpdateAnchorPosition()
    {
        if (followedStaplerStart == null || followedStaplerEnd == null)
        {
            DetachColonSphere();
        }

        followedRay.origin = followedStaplerEnd.position;
        followedRay.direction = followedStaplerStart.position - followedStaplerEnd.position;
        Vector3 newPos = MathUtil.ProjectionPointOnLine(followedRay, targetSphere.transform.position);
        if (Vector3.Distance(targetSphere.transform.position, newPos) < Vector3.Distance(targetSphere.transform.position, transform.position))
        {
            transform.position = newPos;
        }
        else if (Vector3.Distance(targetSphere.transform.position, newPos) > detachDistance)
        {
            DetachColonSphere();
        }

        if (transform.position.z - followedStaplerStart.position.z > ColonStaplerJointManager.instance.insertionDepthDetatch)
        {
            DetachColonSphere();
        }

        if (Mathf.Abs(globalOperators.instance.colonLayerAveragePosition[targetSphereColon][targetSphereLayer].z - transform.position.z) > ColonStaplerJointManager.instance.layerDisplacementDetatch) // If the sphere is "squeezed" too far away from its layer's z-position
        {
            DetachColonSphere();
        }

        //if ()
    }
}
