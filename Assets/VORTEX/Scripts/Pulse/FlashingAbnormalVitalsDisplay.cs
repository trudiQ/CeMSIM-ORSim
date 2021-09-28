using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashingAbnormalVitalsDisplay : MonoBehaviour
{
    public int abnormalUpperThreshold = 0;
    public int abnormalLowerThreshold = 0;
    public List<FlashingAbnormalVitalsDisplay> otherVitalsAffected;
    private float baseline = 1.0f;
    private float frequency = 6.0f;
    private float offset = 0.0f;
    private float magnitude = 1.4f;


    private Text vitalText;
    private PulseDataNumberRenderer pulseData;
    private bool evaluate = true;
    // Start is called before the first frame update
    void Start()
    {
        vitalText = this.GetComponent<Text>();
        pulseData = this.GetComponent<PulseDataNumberRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(evaluate)
        {
            Evaluate();
        }
    }

    public void Evaluate()
    {
        float currentValue = pulseData.currentValue;
        if(currentValue <= abnormalLowerThreshold)
        {
            FlashValue();
            CheckForOtherVitalsToFlash();
            
        }
        else if(currentValue >= abnormalUpperThreshold)
        {
            FlashValue();
            CheckForOtherVitalsToFlash();
        }
        else
        {
            ReturnToNormal();
            CheckForOtherVitalsToReturnToNormal();
        }
    }

    public void StartEvaluating()
    {
        evaluate = true;
    }

    public void StopEvaluating()
    {
        evaluate = false;
    }

    public void CheckForOtherVitalsToFlash()
    {
        if(otherVitalsAffected.Count != 0)
        {
            foreach(var vital in otherVitalsAffected)
            {
                vital.StopEvaluating();
                vital.FlashValue();
            }
        }
    }

    public void CheckForOtherVitalsToReturnToNormal()
    {
        if(otherVitalsAffected.Count != 0)
        {
            foreach(var vital in otherVitalsAffected)
            {
                vital.StartEvaluating();
                vital.ReturnToNormal();
            }
        }
    }

    public void FlashValue()
    {
        float opacity = Mathf.Clamp(baseline * Mathf.Sin(Time.time * frequency + offset) * magnitude, 0.01f, 1);
        vitalText.color = new Color(vitalText.color.r, vitalText.color.g, vitalText.color.b, opacity);
    }

    public void ReturnToNormal()
    {
        vitalText.color = new Color(vitalText.color.r, vitalText.color.g, vitalText.color.b, 1);
    }
    
}
