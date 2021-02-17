using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualOverrideVital : MonoBehaviour
{
    private PulseDataNumberRenderer vitalToChange;
    public PulseDataNumberRenderer vitalToMonitor;
    // public float lastValue = 0.0f;
    // public bool over = false;
    public int threshold;

    private int delayBuffer = 3;
    private float lerpTimerMax = 2.0f;
    private float lerpTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        vitalToChange = this.GetComponent<PulseDataNumberRenderer>();
    }

    void Update()
    {
        //buffer is used to avoid setting vitalToChange to zero during intialization processes
        if(delayBuffer > 0)
        {
            delayBuffer--;
        }
        else
        {
            Override();
        }

        
       //else
       //{
           // if(vitalToMonitor.currentValue <= threshold)
           // {
           //     vitalToChange.multiplier = 0;
           // }
        //}
    }
    void Override()
    {
            if(vitalToMonitor.currentValue <= threshold && vitalToChange.currentValue >= threshold)
            {
                ReduceToZero();
                Debug.Log("under thresh");
            }
    }
    void ReduceToZero()
        {
            if(lerpTime <= lerpTimerMax)
            {
                lerpTime += Time.deltaTime * 0.0005f;
            }
            
            float t = lerpTime/lerpTimerMax;
            if(t < 1)
            {
               vitalToChange.multiplier = Mathf.Lerp(vitalToChange.multiplier, 0, t);
            }
        }
}
