using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pulse.CDM;

public class PatientManager : MonoBehaviour
{
    [HideInInspector]
    public PulseEngineDriver pulseEngineDriver;
    [HideInInspector]
    public PulseEventManager pulseEventManager;

    private static PatientManager _instance;
    public static PatientManager Instance { get { return _instance; } }

    public List<float> intervals = new List<float>();
    private List<bool> intervalActionsTriggered = new List<bool>();

    private ScenarioManager scenarioManager;

    protected virtual void Awake()
    {
        if (_instance = null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        if (scenarioManager == null)
        {
            scenarioManager = ScenarioManager.Instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void Start()
    {
        pulseEngineDriver = this.GetComponent<PulseEngineDriver>();
        pulseEventManager = this.GetComponent<PulseEventManager>();

        PatientEvents.Instance.NeedleDecompression += OnNeedleDecompression;
        PatientEvents.Instance.PatientPneumothorax += OnTriggerPneumothorax;

        DetermineIntervals(scenarioManager.timeNeedleDecompFail, 4);
    }

    private void OnTriggerPneumothorax()
    {
        //Trigger Pneumothorax
        pulseEventManager.TriggerPulseAction(Pulse.CDM.PulseAction.TensionPneumothorax);
    }

    private void OnNeedleDecompression()
    {
        pulseEventManager.TriggerPulseAction(Pulse.CDM.PulseAction.NeedleDecompressions);
    }

    #region Functions | Intervals/Timing
    public void DetermineIntervals(float dur, int noIntervals)
    {
        for (int i = 0; i <= noIntervals; i++)
        {
            intervals.Add((dur / noIntervals) * (i));
            intervalActionsTriggered.Add(false);
        }
        scenarioManager.tensionPneumothorax = true;
    }

    public void CheckTime(int noIntervals)
    {
        //scenarioManager.timeElapsed += Time.deltaTime;

        for (int i = 0; i < noIntervals; i++)
        {
            if (scenarioManager.timeElapsed > intervals[i] && !intervalActionsTriggered[i])
            {
                pulseEventManager.TriggerPulseAction(Pulse.CDM.PulseAction.TensionPneumothorax, (1 / (float)noIntervals) * (i + 1));
                intervalActionsTriggered[i] = true;
            }

            else if (scenarioManager.timeElapsed > intervals[intervals.Count - 1] && !intervalActionsTriggered[intervals.Count - 1])
            {
                Debug.Log("You Lose");
                intervalActionsTriggered[intervals.Count - 1] = true;
            }
        }
    }


    #endregion
}
