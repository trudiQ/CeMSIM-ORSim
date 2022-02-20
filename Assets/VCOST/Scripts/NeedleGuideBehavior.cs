using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using Obi;

public class NeedleGuideBehavior : MonoBehaviour
{
    public bool _debug = true;
    public float rodDistance = 0.15f;

    public NeedleBehavior needle;
    public float shrinkMult = 1.75f;
    public UnityEvent e_onSuccessfulPassthrough = new UnityEvent();
    public UnityEvent e_onSelfDestruct = new UnityEvent();
    private bool needleContained = true;
    private bool holeShrunk = false;

    private bool needleGrabbed = false;
    private Vector3 positionOffset;
    private Quaternion rotationOffset;
    
    private List<Collider> colliders;
    private List<GameObject> currentNeedleParts = new List<GameObject>();
    private Collider trigger;

    private void GrabNeedle()
    {
        if (needleContained)
        {
            needleGrabbed = true;
            needle.FreezeRigidBody();

            positionOffset = needle.transform.position - transform.position;
        }
    }

    private void ReleaseNeedle()
    {
        if (needleContained)
        {
            needleGrabbed = false;
            needle.UnFreezeRigidBody();
        }
    }

    private void Start()
    {
        colliders = new List<Collider>();
        needle.e_needleRelease.AddListener(GrabNeedle);
        needle.e_needleGrab.AddListener(ReleaseNeedle);

        Collider[] selfColliders = gameObject.GetComponents<Collider>();
        foreach(Collider c in selfColliders)
        {
            if(c.isTrigger == true)
            {
                trigger = c;
                break;
            }
        }

        Collider[] unfilteredColliders = gameObject.GetComponentsInChildren<Collider>();
        foreach(Collider c in unfilteredColliders)
        {
            if (!c.isTrigger) colliders.Add(c);
        }
    }

    private void FixedUpdate()
    {
        if (!needleContained) return;

        if (_debug)
        {
            Debug.DrawRay(transform.position, transform.right * -1 * rodDistance, Color.cyan);
        }

        if (needleGrabbed)
        {
            needle.transform.position = transform.position + positionOffset;
        }

        bool anyCollision = false;
        foreach(ObiCollider c in needle.needleColliders)
        {
            if (c.sourceCollider.bounds.Intersects(trigger.bounds))
            {
                anyCollision = true;
            }
        }

        if (!anyCollision)
        {
            needle.GuideExited();
            needleContained = false;

            Vector3 needleDir = transform.position - needle.transform.position;

            if (Vector3.Dot(needleDir, transform.up) < 0)
            {
                e_onSelfDestruct.Invoke();
                Destroy(gameObject);
            }
            else
            {
                e_onSuccessfulPassthrough.Invoke();
            }
        }
    }
}
