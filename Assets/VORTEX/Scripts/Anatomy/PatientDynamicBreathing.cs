using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientDynamicBreathing : MonoBehaviour
{
    private SkinnedMeshRenderer meshRenderer;
    public PulseDataNumberRenderer dataSource;

    public float breath = 0f;
    public float inflation = 0;
    private float currentValue = 0f;

    public float holdBreatheTimer = 0;
    public float holdBreatheTime = 1;
    public bool holdBreathe = false;

    public float currentLerpTime;

    private int mod = 1;

    public float totalTime = 0;
    public float numberOfBreathesIn = 0;
    public float numberOfBreathesOut = 0;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        totalTime = Time.timeSinceLevelLoad;
        if(Mathf.Abs(currentValue - dataSource.currentValue) > .1f)
        {
            currentValue = dataSource.currentValue;
        }

        Breathe(60 / currentValue);

    }

    public void Breathe(float breathDuration)
    {
        if(breath > 1 && mod == 1)
        {
            holdBreathe = true;
            numberOfBreathesIn++;
            mod= -1;
        }
        else if(breath < 0 && mod == -1)
        {
            holdBreathe = true;
            numberOfBreathesOut++;
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
            inflation = Mathf.Lerp(0, 100, breath);

            meshRenderer.SetBlendShapeWeight(0, inflation);
        }
        
        

        

    }
}
