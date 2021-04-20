using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detect if the two parts of the linear stapler are close enough to be attached together
/// </summary>
public class StaplerAttachDetection : MonoBehaviour
{
    public Collider matcher; // The collider that match with this trigger

    public bool isTogether;

    private void OnTriggerEnter(Collider other)
    {
        if (other == matcher)
        {
            isTogether = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == matcher)
        {
            isTogether = false;
        }
    }
}
