using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pulse.CDM;

//namespace Pulse.CDM
//{
    public class PulseEventManager : MonoBehaviour
    {
        private PulseActionManager actionManager;

        public delegate void TriggerAction();
        public static event TriggerAction triggerAction;

        private void Start()
        {
            actionManager = this.GetComponent<PulseActionManager>();
        }

        public void TriggerPulseAction(Pulse.CDM.PulseAction action)
        {
            actionManager.action = action;

            triggerAction?.Invoke();
        }
    }
//}