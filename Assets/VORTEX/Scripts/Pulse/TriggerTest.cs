using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pulse.CDM;

public class TriggerTest : MonoBehaviour
{
    private PulseEventManager eventManager;

    private void Start()
    {
        eventManager = PatientManager.Instance.pulseEventManager;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Tool")
        {
            Debug.Log("Start Tension Pneumothorax");
            eventManager.TriggerPulseAction(Pulse.CDM.PulseAction.TensionPneumothorax);
        }
    }
}