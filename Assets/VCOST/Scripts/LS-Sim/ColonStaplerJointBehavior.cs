using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColonStaplerJointBehavior : MonoBehaviour
{

    public Rigidbody targetSphere;
    public FixedJoint jointToSphere;
    public Transform followedStaplerStart;
    public Transform followedStaplerEnd;
    public float detachDistance;
    public Ray followedRay;
    public List<ColonStaplerJointBehavior> anchorForNeighborSpheres;
    public List<Transform> neighborSpheres;
    public int targetSphereLayer;
    public int targetSphereColon;
    public bool isStaticCollisionJoint; // Is this a staticCollisionJoint
    public Vector3 staticAnchorLocalStaplerPosition;
    public Transform followedStapler;
    public ColonStaplerJointBehavior createdStaticJoint;

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
        if (!isStaticCollisionJoint) // Normal update for regular joint object
        {
            UpdateAnchorPosition();
            CheckSphereStatusForStaticJoint();
        }
        else
        {
            UpdateAnchorPositionForStaticJoint();
        }
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

        if (transform.position.z - followedStaplerStart.position.z > ColonStaplerJointManager.instance.insertionDepthDetatch &&
            Vector3.Distance(transform.position, followedStaplerStart.position) > ColonStaplerJointManager.instance.insertionDepthDetatch)
        {
            DetachColonSphere();
        }

        if (Mathf.Abs(globalOperators.instance.colonLayerAveragePosition[targetSphereColon][targetSphereLayer].z - transform.position.z) > ColonStaplerJointManager.instance.layerDisplacementDetatch) // If the sphere is "squeezed" too far away from its layer's z-position
        {
            DetachColonSphere();
        }

        //if ()
    }

    /// <summary>
    /// Check if this anchor's target sphere need to have static joint created connecting stapler
    /// </summary>
    public void CheckSphereStatusForStaticJoint()
    {
        // If this anchor's sphere is too far away from any of its neighbor
        List<ColonStaplerJointBehavior> distantNeighbors = anchorForNeighborSpheres.FindAll(a =>
        Vector3.Distance(a.targetSphere.transform.position, targetSphere.transform.position) > ColonStaplerJointManager.instance.staticAnchorEnableDistance).ToList();

        if (distantNeighbors.Count > 0 && distantNeighbors.Exists(a => !globalOperators.instance.IsSphereGrabbedByForceps(a.targetSphere.gameObject)))
        {
            isStaticCollisionJoint = true;
            ColonStaplerJointManager.instance.staticCollisionJoints.Add(this);
        }
    }

    public void UpdateAnchorPositionForStaticJoint()
    {
        transform.position = followedStapler.TransformPoint(staticAnchorLocalStaplerPosition);

        // If this anchor's sphere is not too far away from any of its neighbor
        if (!anchorForNeighborSpheres.Exists(a =>
        Vector3.Distance(a.GetComponent<ColonStaplerJointBehavior>().targetSphere.transform.position, targetSphere.transform.position) > ColonStaplerJointManager.instance.staticAnchorDisableDistance))
        {
            isStaticCollisionJoint = false;
            ColonStaplerJointManager.instance.staticCollisionJoints.Remove(this);
        }
    }
}
