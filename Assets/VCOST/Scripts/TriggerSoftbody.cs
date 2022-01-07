using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Obi;
using Custom.Events;

namespace Custom.Events
{
    [System.Serializable]
    public class UnityEventObiContact : UnityEvent<Oni.Contact, int> { }
}

public class TriggerSoftbody : MonoBehaviour
{
    public UnityEventObiContact triggerEnterEvent = new UnityEventObiContact();
    public UnityEvent triggerExitEvent = new UnityEvent();

    public void TriggerEnterBehavior(Oni.Contact contact, int particleIndex)
    {
        triggerEnterEvent.Invoke(contact, particleIndex);
    }

    public void TriggerExitBehavior()
    {
        triggerExitEvent.Invoke();
    }
}
