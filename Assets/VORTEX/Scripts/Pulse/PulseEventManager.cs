using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pulse.CDM
{
    public class PulseEventManager : MonoBehaviour
    {
        public PulseActionManager actionManager;

        public delegate void TriggerAction();
        public static event TriggerAction triggerAction;

        public void TriggerPulseAction(PulseAction action)
        {
            actionManager.action = action;

            triggerAction?.Invoke();
        }
    }
}