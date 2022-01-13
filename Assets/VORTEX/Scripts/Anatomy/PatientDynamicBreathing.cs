using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientDynamicBreathing : MonoBehaviour
{
    private SkinnedMeshRenderer meshRenderer;
    public PulseDataNumberRenderer dataSource;
    public int shapeKeyIndex;

    private float breath = 0f;
    private float inflation = 0;
    private float currentValue = 0f;

    private float holdBreatheTimer = 0;
    private float holdBreatheTime = 1;
    private bool holdBreathe = false;

    private float currentLerpTime;

    private int mod = 1;        //used to delimit whether lungs should be inflating or deflating

    // public float totalTime = 0;
    // public float numberOfBreathesIn = 0;
    // public float numberOfBreathesOut = 0;
    public TieVentilationToBreath audioPlayer;
    // public bool beginningTriggered = false;
    // public bool endTriggered = false;
    private bool inflating = false;
    // public bool breathIn = false;

    private float lastInflation = 0;
    private float inflationDifference = 0;
    private float breathingRate;
    public bool dynamicBreathing = false;
    // private bool lungsReset = false;
    //public bool middleTiggered = false;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();

        if(!dynamicBreathing)
            breathingRate = 60/16;
    }

    // Update is called once per frame
    void Update()
    {
        // totalTime = Time.timeSinceLevelLoad;
        if(Mathf.Abs(currentValue - dataSource.currentValue) > .1f)
        {
            currentValue = dataSource.currentValue;
            holdBreatheTime = 1.6f - currentValue/16;
        }

        if(dynamicBreathing)
        {
            breathingRate = 60/currentValue;
        }

        Breathe(breathingRate, 100);

        //TODO (MH 5/21/2021): implement dynamic, asynchronous collapsed lung behaviors
        // if(ScenarioManager.Instance.pneumothoraxSeverity <= .1f)
        // {
        //     Breathe(breathingRate, 100, 2);    
        // }
        // else
        // {
        //     if(!lungsReset)
        //     {
        //         meshRenderer.SetBlendShapeWeight(2, 0);
        //         lungsReset = true;
        //     }
        //     Breathe(breathingRate, 100, 3);
        //     Breathe(breathingRate, 100, 4);
        //     // Breathe(breathingRate, 100 * (1.2f - ScenarioManager.Instance.pneumothoraxSeverity), 4);
        // }
        

    }

    public void Breathe(float breathDuration, float breatheMagnitude)
    {
        if(breath > 1 && mod == 1)
        {
            holdBreathe = true;
            // numberOfBreathesIn++;
            mod = -1;
        }
        else if(breath < 0 && mod == -1)
        {
            holdBreathe = true;
            // numberOfBreathesOut++;
            mod = 1;
        }

        if(holdBreathe)
        {
            if (holdBreatheTimer <= holdBreatheTime)
            {
                holdBreatheTimer += Time.deltaTime;
            }
            else
            {
                holdBreatheTimer = 0;
                holdBreathe = false;
            }
        }
        else
        {
            currentLerpTime += Time.deltaTime * mod;

            breath = currentLerpTime / (breathDuration * 0.5f - holdBreatheTime);
            breath = breath * breath * breath * (breath * (6f * breath - 15f) + 10f);   //smootherstep lerp
            inflation = Mathf.Lerp(0, (int)breatheMagnitude, breath);

            meshRenderer.SetBlendShapeWeight(shapeKeyIndex, inflation);
        }
        
        // breathIn = lastInflation - inflation < -.0001f ? true : false;
        inflationDifference = lastInflation - inflation;

        ControlVentilatorSounds();
        lastInflation = inflation;
    }

     private void ControlVentilatorSounds()
     {
         if(inflation > 10 && inflation < 90)
         {
             if(inflationDifference < -0.0001f)
             {
                 if(!inflating)
                 {
                     inflating = true;
                    //  beginningTriggered = false;
                    //  endTriggered = false;
                     audioPlayer.Play(audioPlayer.inflate);
                 }
             }
         }
         else if((int)inflation == 0 && inflating)
         {
             inflating = false;
         }
         else if((int)inflation == 100 && inflating)
         {
             inflating = false;
         }
     }
}
