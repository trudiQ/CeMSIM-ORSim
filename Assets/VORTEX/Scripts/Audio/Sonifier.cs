using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sonifier : MonoBehaviour
{
    public PulseDataNumberRenderer dataSource;
    public float sonifcationRate = 0.02f;       //most monitors appear to change their frequency by roughly 2% for each pulse ox percentage change.
    // public float thresholdValue;
    public float currentValue = 0;
    private float lastValue = 0;
    private AudioPlayer audioPlayer;
    private int delayBuffer = 3;        //used to ensure sonification system does not kick in before all pulse data is loaded

    // Start is called before the first frame update
    void Start()
    {
        audioPlayer = GetComponent<AudioPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(delayBuffer > 0)
        {
            delayBuffer--;
        }
        else
        {
            currentValue = dataSource.currentValue;

            if(audioPlayer.audioSource.pitch < 0.4f)
            {
                audioPlayer.audioSource.pitch = 0.4f;
            }
            if((int)(currentValue + .5f) != (int)lastValue)
            {
                audioPlayer.Pause();
                Sonifiy();
                audioPlayer.UnPause();
                lastValue = currentValue;
            }
        }
        
        

        
    }


    public void AdjustPitch(float value)
    {
        audioPlayer.audioSource.pitch = value;
    }

    void Sonifiy()
    {
        float normalizedValue = currentValue / 100;
        normalizedValue = normalizedValue - sonifcationRate;

        AdjustPitch(normalizedValue);
    }
}
