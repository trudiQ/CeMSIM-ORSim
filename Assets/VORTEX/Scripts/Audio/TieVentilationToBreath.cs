using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TieVentilationToBreath : AudioPlayer
{
    public bool paused = false;
    public AudioClip  inflate;
    public PulseDataNumberRenderer etco2;
    public float lastValue;
    public bool canPlay = false;

    public int offset = 0;

    private WriteToFile writer;
    public float sum = 0.0f;
    public float average = 0.0f;
    public float highestValue = 0.0f;
    public float lowestValue = 100.0f;
    public float currentValue = 0.0f;

    private int delayBuffer = 3;
    private List<float> previousValues;
    public int count;
    public float averageOfApexes = 0.0f;

    private bool audioPlayed = false;


    public void Update()
    {
        if(delayBuffer > 0)
        {
            delayBuffer--;
        }
        else
        {
            // DoStuff();
            DoDifferentStuff();
        }
    }
    public void DoDifferentStuff()
    {
        currentValue = etco2.currentValue;

        if(Mathf.Abs(currentValue - lastValue) > .000001)
        {
            
            // canPlay = true;
            StartCoroutine(DelayedAudioPlay());
        }
        else if (audioPlayed)
        {
            canPlay = false;
        }

        
    }
    public void DoStuff()
    {
        // if(Mathf.Abs(etco2.currentValue - lastValue) > .00001f)
        // {
        //     canPlay = true;
        // }
        // writer = new WriteToFile();
        // writer.WriteStringToFile(etco2.currentValue, Time.time);

        currentValue = etco2.currentValue;
        count++;

        
        // previousValues.Add(currentValue);

        if(currentValue > highestValue)
        {
            highestValue = currentValue;
        }
        if(currentValue < lowestValue && currentValue > 1)
        {
            lowestValue = currentValue;
        }

        sum += currentValue;

        average = sum / count;
        averageOfApexes = (lowestValue + highestValue) / 2;


        // for(int i = etco2.list.Count; i > count; i--)
        // {
        //     if(i < 0)
        //     {
        //         break;
        //     }
        //     if(currentValue > highestValue)
        //     {
        //         highestValue = currentValue;
        //     }
        //     sum += etco2.list.Get(i);
        // }
        // float currentAverage = sum / 300;

        if(currentValue > averageOfApexes)
        {
            canPlay = true;
            // DelayedAudioPlay();
        }
        else
        {
            canPlay = false;
        }
        // if(offset > 75)
        // {
        //     canPlay = true;
        //     offset = 0;
        // }
        // else
        // {
        //     offset++;
        // }
      

        lastValue = etco2.currentValue;
    }

    public IEnumerator DelayedAudioPlay()
    {
        yield return new WaitForSeconds(0.1f);
        canPlay = true;
    }
    public void Play(AudioClip clip)
    {
        if(clip.Equals(inflate) && canPlay)
        {
            audioSource.clip = inflate;
            if(!audioSource.isPlaying)
            {
                audioSource.Play();
                lastValue = currentValue;
                // canPlay = false;
                audioPlayed = true;
            }
        }
        // else if(clip.Equals(deflate))
        // {
        //     audioSource.clip = deflate;
        //     if(!audioSource.isPlaying)
        //     {
        //         audioSource.Play();
        //     }
        // }
    }

    public override void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = isAudio3D ? 1 : 0;
        audioSource.volume = volume;
        audioSource.rolloffMode = AudioRolloffMode.Custom;

        previousValues = new List<float>();
    }

    public override void Pause()
    {
        audioSource.Pause();
        paused = true;
    }

    public override void UnPause()
    {
        audioSource.UnPause();
        paused = false;
    }
}
