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
    public Transform belongedColumnStart; // The first sphere of the column which this anchor's sphere belongs to
    public Transform belongedColumnEnd; // The last sphere of the column which this anchor's sphere belongs to
    public Transform belongedColumnTiltingEnd; // The sphere of the column that is at the end position of the tilting animation
    public float detachDistance;
    public Ray followedRay;
    public List<ColonStaplerJointBehavior> anchorForNeighborSpheres;
    public List<Transform> neighborSpheres;
    public int targetSphereInLayerIndex;
    public int targetSphereLayer;
    public int targetSphereColon;
    public bool isStaticCollisionJoint; // Is this a staticCollisionJoint
    public Vector3 staticAnchorLocalStaplerPosition;
    public Transform followedStapler;
    public StaplerColonSphereTrigger followedStaplerCollisionTrigger;
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
        CheckConditionsForStaticJoint();

        if (!isStaticCollisionJoint) // Normal update for non-static anchor
        {
            UpdateAnchorPosition();
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

    public void SwitchStatic(bool isStatic)
    {
        isStaticCollisionJoint = isStatic;

        if (isStatic)
        {
            staticAnchorLocalStaplerPosition = followedStapler.InverseTransformPoint(transform.position);
        }
        else
        {

        }
    }

    /// <summary>
    /// Check if this anchor's target sphere need to have static joint created connecting stapler
    /// </summary>
    public void CheckConditionsForStaticJoint()
    {
        //// If this anchor's sphere is too far away from any of its neighbor
        //List<ColonStaplerJointBehavior> distantNeighbors = anchorForNeighborSpheres.FindAll(a =>
        //Vector3.Distance(a.targetSphere.transform.position, targetSphere.transform.position) > ColonStaplerJointManager.instance.staticAnchorEnableDistance).ToList();

        //if (distantNeighbors.Count > 0 && distantNeighbors.Exists(a => !globalOperators.instance.IsSphereGrabbedByForceps(a.targetSphere.gameObject)))
        //{
        //    isStaticCollisionJoint = true;
        //    staticAnchorLocalStaplerPosition = followedStapler.InverseTransformPoint(transform.position);
        //    ColonStaplerJointManager.instance.staticCollisionJoints.Add(this);
        //}

        if (!followedStaplerCollisionTrigger.isConnectingColon)
        {
            bool connect = false;
            bool byTilt = false;
            Vector3 staplerTriggerLookTarget = Vector3.zero;
            // Check stapler tilt
            if (followedStaplerStart.position.y - followedStaplerEnd.position.y >)
            {
                connect = true;
                byTilt = true;
                staplerTriggerLookTarget = followedStaplerCollisionTrigger.transform.position + Vector3.up;
            }

            // Check sphere position
            Transform columnEndSphere;
            if (ColonMovementController.instance.updateMode[targetSphereColon] == 1)
            {
                columnEndSphere = belongedColumnTiltingEnd;
            }
            else
            {
                columnEndSphere = belongedColumnEnd;
            }
            Ray column = new Ray(belongedColumnStart.position, columnEndSphere.position - belongedColumnStart.position);
            float sphereToColumnDistance = MathUtil.DistancePointToLine(column, targetSphere.position);

            if (sphereToColumnDistance >)
            {
                connect = true;
                byTilt = false;
                staplerTriggerLookTarget = targetSphere.position;
            }

            if (connect)
            {
                followedStaplerCollisionTrigger.connectedByTilt = byTilt;
                followedStaplerCollisionTrigger.RotateToward(staplerTriggerLookTarget); // Rotate the trigger to capture target spheres to turn related anchor to static
            }
        }
        else
        {
            if (followedStaplerCollisionTrigger.connectedByTilt)
            {
                // Check stapler tilt
                if (followedStaplerStart.position.y - followedStaplerEnd.position.y <)
                {
                    ColonStaplerJointManager.instance.SwitchJointAnchorStatic(this, followedStaplerCollisionTrigger, false);
                }
            }
            else
            {
                // Check sphere position
                Transform columnEndSphere;
                if (ColonMovementController.instance.updateMode[targetSphereColon] == 1)
                {
                    columnEndSphere = belongedColumnTiltingEnd;
                }
                else
                {
                    columnEndSphere = belongedColumnEnd;
                }
                Ray column = new Ray(belongedColumnStart.position, columnEndSphere.position - belongedColumnStart.position);
                float sphereToColumnDistance = MathUtil.DistancePointToLine(column, targetSphere.position);

                if (sphereToColumnDistance <)
                {
                    ColonStaplerJointManager.instance.SwitchJointAnchorStatic(this, followedStaplerCollisionTrigger, false);
                }
            }
        }
    }

    /// <summary>
    /// Enable the collision behavior to prevent stapler penetration
    /// </summary>
    /// <returns></returns>
    public IEnumerator ConnectStapler()
    {
        yield return null;

        ColonStaplerJointManager.instance.SwitchJointAnchorStatic(this, followedStaplerCollisionTrigger, true);
    }

    public void UpdateAnchorPositionForStaticJoint()
    {
        transform.position = followedStapler.TransformPoint(staticAnchorLocalStaplerPosition);

        //// If this anchor's sphere is not too far away from any of its neighbor
        //if (!anchorForNeighborSpheres.Exists(a =>
        //Vector3.Distance(a.GetComponent<ColonStaplerJointBehavior>().targetSphere.transform.position, targetSphere.transform.position) > ColonStaplerJointManager.instance.staticAnchorDisableDistance))
        //{
        //    isStaticCollisionJoint = false;
        //    ColonStaplerJointManager.instance.staticCollisionJoints.Remove(this);
        //}
    }

    public void InitializeAnchor()
    {
        // Get target sphere layer
        targetSphereLayer = globalOperators.GetSphereLayer(targetSphere.name);

        // Get target sphere colon
        targetSphereColon = int.Parse(targetSphere.name[7].ToString());

        // Get belonged column info
        if (targetSphereColon == 0)
        {
            // Get target sphere's index in its layer
            targetSphereInLayerIndex = globalOperators.instance.colon0Spheres[targetSphereLayer].IndexOf(targetSphere.transform);

            belongedColumnStart = globalOperators.instance.colon0Spheres[0][targetSphereInLayerIndex];
            belongedColumnEnd = globalOperators.instance.colon0Spheres[19][targetSphereInLayerIndex];
            belongedColumnTiltingEnd = globalOperators.instance.colon0Spheres[9][targetSphereInLayerIndex];
        }
        else
        {
            // Get target sphere's index in its layer
            targetSphereInLayerIndex = globalOperators.instance.colon1Spheres[targetSphereLayer].IndexOf(targetSphere.transform);

            belongedColumnStart = globalOperators.instance.colon1Spheres[0][targetSphereInLayerIndex];
            belongedColumnEnd = globalOperators.instance.colon1Spheres[19][targetSphereInLayerIndex];
            belongedColumnTiltingEnd = globalOperators.instance.colon1Spheres[9][targetSphereInLayerIndex];
        }
    }
}
