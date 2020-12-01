using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pulse.CDM;

public class PulseEventManager : MonoBehaviour
{
    private PulseActionManager actionManager;

    public delegate void TriggerAction();
    public static event TriggerAction triggerAction;

   // private void OnEnable()
   // {
   //     PatientEvents.Instance.PatientPneumothorax += TriggerPulseAction;
   // }
   // private void OnDisable()
   // {
   //     PatientEvents.Instance.PatientPneumothorax -= TriggerPulseAction;
   // }

    private void Start()
    {
        actionManager = this.GetComponent<PulseActionManager>();
    }

    public void TriggerPulseAction(Pulse.CDM.PulseAction action)
    {
        actionManager.action = action;

        triggerAction?.Invoke();
    }

    public void TriggerPulseAction(Pulse.CDM.PulseAction action, float severity)
    {
        actionManager.action = action;
        actionManager.tp_severity = severity;

        triggerAction?.Invoke();
    }
}