using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pulse.CDM;

public class TriggerTest : MonoBehaviour
{
    private PatientManager patient;

    private void Start()
    {
        patient = PatientManager.Instance;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Tool")
        {
            Debug.Log("Start Tension Pneumothorax");
            patient.pulseEventManager.TriggerPulseAction(Pulse.CDM.PulseAction.TensionPneumothorax);
        }
    }
}