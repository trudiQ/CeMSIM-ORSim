using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class ForcepBehavior : MonoBehaviour
{
    public Transform rigidbodyHolder;
    public ForcepHalfBehavior rightHalf;
    public ForcepHalfBehavior leftHalf;
    
    //public Transform follow;

    //Local-space rotations are done in local space (0-1), who knew?
    public float maxOpenAngle = 0.35f;
    public float articulationSpeed = 2f;

    public bool doObi = false;
    public Transform obiAttachPoint;
    public BlueprintParticleIndividualizer obiObject;
    public float attachObiMaxAngle = 0.05f;
    public float finalObiMaxAngle = 0.01f;
    public float maxGrabberDistance = 0.25f;

    private ObiParticleAttachment currentGrabberAttachment = null;
    public void DestroyAttachment() { Destroy(currentGrabberAttachment); }

    private bool open = false;
    private bool close = false;

    public void SignalOpen() { open = true; }
    public void SignalStopOpen() { open = false; }

    public void SignalClose() { close = true; }
    public void SignalStopClose() { close = false; }

    private GameObject grabbing;
    private FixedJoint joint;

    private void Start()
    {
        grabbing = null;
        joint = null;
    }

    private void FixedUpdate()
    {
        if (doObi)
        {
            ObiPhysicsGrab();
        }
        else
        {
            UnityPhysicsGrab();
        }
    }

    void ObiPhysicsGrab()
    {
        if (open)
        {
            if (currentGrabberAttachment != null)
                Destroy(currentGrabberAttachment);

            if (leftHalf.transform.localRotation.y <= maxOpenAngle)
            {
                leftHalf.transform.Rotate(
                    Vector3.up * articulationSpeed * Time.fixedDeltaTime
                    );
            }

            if (rightHalf.transform.localRotation.y >= maxOpenAngle * -1)
            {
                rightHalf.transform.Rotate(
                    Vector3.up * articulationSpeed * Time.fixedDeltaTime * -1
                    );
            }
        }
        else if (close)
        {
            if (leftHalf.transform.localRotation.y >= finalObiMaxAngle &&
                rightHalf.transform.localRotation.y <= finalObiMaxAngle * -1 &&
                currentGrabberAttachment != null)
            {
                return;
            }

            if (leftHalf.transform.localRotation.y <= attachObiMaxAngle &&
                rightHalf.transform.localRotation.y >= attachObiMaxAngle * -1 &&
                currentGrabberAttachment == null)
            {
                if (obiObject.GetDistanceToClosestParticle(obiAttachPoint.transform.position) < maxGrabberDistance)
                    currentGrabberAttachment = obiObject.CreateNewParticleAttachmentClosestTo(obiAttachPoint.transform);
            }

            if (leftHalf.transform.localRotation.y >= 0)
            {
                leftHalf.RotateIfNotGrabbing(
                    Vector3.up * articulationSpeed * Time.fixedDeltaTime * -1
                    );
            }

            if (rightHalf.transform.localRotation.y <= 0)
            {
                rightHalf.RotateIfNotGrabbing(
                    Vector3.up * articulationSpeed * Time.fixedDeltaTime
                    );
            }
        }
    }

    void UnityPhysicsGrab()
    {
        if (open)
        {
            release();

            if (leftHalf.transform.localRotation.y <= maxOpenAngle)
            {
                leftHalf.transform.Rotate(
                    Vector3.up * articulationSpeed * Time.fixedDeltaTime
                    );
            }

            if (rightHalf.transform.localRotation.y >= maxOpenAngle * -1)
            {
                rightHalf.transform.Rotate(
                    Vector3.up * articulationSpeed * Time.fixedDeltaTime * -1
                    );
            }
        }
        else if (close)
        {
            //Debug.Log(joint);
            if (leftHalf.grabbedTransform != null && rightHalf.grabbedTransform != null && grabbing == null)
            {
                grab(leftHalf.grabbedTransform);
            }

            if (leftHalf.transform.localRotation.y >= 0)
            {
                leftHalf.RotateIfNotGrabbing(
                    Vector3.up * articulationSpeed * Time.fixedDeltaTime * -1
                    );
            }

            if (rightHalf.transform.localRotation.y <= 0)
            {
                rightHalf.RotateIfNotGrabbing(
                    Vector3.up * articulationSpeed * Time.fixedDeltaTime
                    );
            }
        }
    }

    //Just copy pasta'd from haptic grabber with a few edits
    void grab(Transform t)
    {
        //Debug.Log(" Object : " + t.name + "  Tag : " + t.tag);
        grabbing = t.gameObject;

        //Debug.logger.Log("Grabbing Object : " + grabbing.name);
        Rigidbody body = grabbing.GetComponent<Rigidbody>();

        // If this doesn't have a rigidbody, walk up the tree. 
        // It may be PART of a larger physics object.
        while (body == null)
        {
            //Debug.logger.Log("Grabbing : " + grabbing.name + " Has no body. Finding Parent. ");
            if (grabbing.transform.parent == null)
            {
                grabbing = null;
                return;
            }
            GameObject parent = grabbing.transform.parent.gameObject;
            if (parent == null)
            {
                grabbing = null;
                return;
            }
            grabbing = parent;
            body = grabbing.GetComponent<Rigidbody>();
        }

        NeedleBehavior needle = grabbing.GetComponent<NeedleBehavior>();
        if (needle != null) needle.e_needleGrab.Invoke();

        joint = (FixedJoint)gameObject.AddComponent(typeof(FixedJoint));
        joint.connectedBody = body;
    }

    //Good. Let the pasta flow through you
    void release()
    {
        if (grabbing == null) //Nothing to release
            return;

        joint.connectedBody = null;
        Destroy(joint);

        NeedleBehavior needle = grabbing.GetComponent<NeedleBehavior>();
        if (needle != null) needle.e_needleRelease.Invoke();

        grabbing = null;

    }
}
