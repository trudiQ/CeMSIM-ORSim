using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pulse.CDM
{
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
                eventManager.TriggerPulseAction(PulseAction.TensionPneumothorax);
            }
        }
    }
}