using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

public class NeedleGuideBehavior : MonoBehaviour
{
    int needleInside = 0;

    public NeedleBehavior needle;
    public float shrinkMult = 1.75f;
    public UnityEvent onNeedleExit = new UnityEvent();
    private bool needleContained = true;
    private bool holeShrunk = false;

    private bool needleGrabbed = false;
    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    private List<Collider> colliders;

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

        Collider[] unfilteredColliders = gameObject.GetComponentsInChildren<Collider>();
        foreach(Collider c in unfilteredColliders)
        {
            if (!c.isTrigger) colliders.Add(c);
        }
    }

    private void FixedUpdate()
    {
        if (needleGrabbed)
        {
            needle.transform.position = transform.position + positionOffset;
        }

        if(!needleContained && !holeShrunk)
        {
            StartCoroutine(ShrinkHole());
            holeShrunk = true;
        }
    }

    private IEnumerator ShrinkHole()
    {
        float currentDist = Vector3.Distance(colliders[0].transform.position, transform.position);
        float desiredDist = currentDist / shrinkMult;
        float moveIncrement = desiredDist / 20f;
        while (true)
        {
            bool allStopped = true;
            foreach (Collider c in colliders)
            {
                if (Vector3.Distance(c.transform.position, transform.position) >= desiredDist)
                {
                    Vector3 translateVector = (transform.position - c.transform.position).normalized;
                    c.transform.Translate(translateVector * moveIncrement, Space.World);
                    allStopped = false;
                }
            }
            if (allStopped)
            {
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!needleContained) return;

        if(other.tag == "Needle" || other.tag == "NeedleExit")
        {
            needleInside++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!needleContained) return;

        if(other.tag == "Needle" || other.tag == "NeedleExit")
        {
            needleInside--;
            if (needleInside == 0)
            {
                needle.GuideExited();
                onNeedleExit.Invoke();
                needleContained = false;
            }
        }
    }
}
