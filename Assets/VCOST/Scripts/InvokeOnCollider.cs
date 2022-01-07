using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Custom.Events;

namespace Custom.Events
{
    public class UnityEventCollision : UnityEvent<Collision> { }
    public class UnityEventCollider: UnityEvent<Collider> { }
}

public class InvokeOnCollider : MonoBehaviour
{
    public UnityEventCollision collisionEnter = new UnityEventCollision();
    public UnityEventCollision collisionExit = new UnityEventCollision();

    public UnityEventCollider triggerEnter = new UnityEventCollider();
    public UnityEventCollider triggerExit = new UnityEventCollider();

    private void OnCollisionEnter(Collision collision)
    {
        collisionEnter.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionExit.Invoke(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerEnter.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerExit.Invoke(other);
    }
}
