﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmPlayer : AudioPlayer
{
    public enum AlarmType{DESAT, O2WARNING};
    public AlarmType alarmType;
    public PulseDataNumberRenderer valueToMonitor;
    public float valueToTriggerAlarmAt = 0.0f;
    public bool higher;
    public float delayBetweenDings = 0.5f;
    public bool limitNumberOfDings = false;
    public int numberOfDings = 10;
    public float muteDuration;
    // public GameObject[] alarmLights;
    public AlarmLightController lightController;
    public AlarmPlayer[] otherAlarms;
    private float timeSinceDing = 0.0f;
    private int currentDingNumber = 0;
    private bool paused = false;
    public bool thresholdMet = false;


    // Update is called once per frame
    void Update()
    {
        if(!paused)
        {
            HandleAlarm();
        }
    }

    public override void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = isAudio3D ? 1 : 0;
        audioSource.volume = volume;
    }


    private void CheckForDingLimitation()
    {
        if(limitNumberOfDings)
        {
            if(currentDingNumber >= numberOfDings - 1)
            {
                Pause();
                currentDingNumber = 0;
                
                CheckForOtherAlarmsToUnPause();
            }
            else
            {
                currentDingNumber++;
            }
        }
    }

    private void CheckForOtherAlarmsToPause()
    {   
        if(otherAlarms.Length != 0)
        {
            foreach(var alarm in otherAlarms)
            {
                alarm.Pause(); 
            }
        }
    }
    private void CheckForOtherAlarmsToUnPause()
    {
        if(otherAlarms.Length != 0)
        {
            foreach(var alarm in otherAlarms)
            {
                alarm.UnPause(); 
            }
        }
    }

    private bool IsThresholdMet()
    {
        bool above = valueToMonitor.currentValue > valueToTriggerAlarmAt ? true : false;

        if((higher && above) || (!higher && !above))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool OtherThresholdMet()
    {
        if(otherAlarms.Length != 0)
        {
            foreach(var other in otherAlarms)
            {
                if(other.IsThresholdMet())
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            return false;
        }
    }

    private void HandleAlarm()
    {
        thresholdMet = IsThresholdMet();

        if(thresholdMet)
        {
            if(timeSinceDing >= delayBetweenDings)
            {
                Play();
            }
            else if(timeSinceDing > delayBetweenDings / 2 && thresholdMet)
            {
                SetAlarmLightColor();
                lightController.AlteranateLitLight(0);
            }

            timeSinceDing+=Time.deltaTime;
        }
        else if(!OtherThresholdMet())
        {
            lightController.TurnOffLight();
        }

    }

    private void SetAlarmLightColor()
    {
        switch(alarmType)
        {
            case AlarmType.DESAT:
                lightController.TurnOnRedLight();
                break;
            case AlarmType.O2WARNING:
                lightController.TurnOnYellowLight();
                break;
            default:
                break;
        }
    }

    public void Play()
    {
        if(!audioSource.isPlaying)
        {
            CheckForOtherAlarmsToPause();
            timeSinceDing = 0;
            CheckForDingLimitation();
            audioSource.PlayOneShot(audioClip);
            SetAlarmLightColor();
            lightController.AlteranateLitLight(1);
        }
    }
    public override void Pause()
    {
        audioSource.Pause();
        lightController.TurnOffLight();
        paused = true;
    }

    public override void UnPause()
    {
        audioSource.Pause();
        SetAlarmLightColor();
        paused = false;
    }

    public void Mute()
    {
        Pause();
        StartCoroutine(UnMuteAfterDelay());
    }

    IEnumerator UnMuteAfterDelay()
    {
        yield return new WaitForSeconds(muteDuration);
        UnPause();
    }
}
