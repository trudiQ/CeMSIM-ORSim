using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TieVentilationToBreath : AudioPlayer
{
    VitalsAudioPlayer vitalsAudioPlayer;
    //public PatientDynamicBreathing breathManager;
    public bool paused = false;
    public AudioClip beginning, middle, end, inflate, deflate;
    // Start is called before the first frame update
    void Start()
    {
        vitalsAudioPlayer = GetComponent<VitalsAudioPlayer>();
        audioSource.clip = middle;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Play(AudioClip clip)
    {
        if(clip.Equals(beginning))
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(beginning);
        }
        else if(clip.Equals(inflate))
        {
            if(!audioSource.clip.Equals(inflate))
            {
                audioSource.clip = inflate;
            }
            if(!audioSource.isPlaying)
            {
                //audioSource.loop = true;
                audioSource.Play();
            }
            
        }
        else if(clip.Equals(deflate))
        {
            if(!audioSource.clip.Equals(deflate))
            {
                audioSource.clip = deflate;
            }
            if(!audioSource.isPlaying)
            {
                //audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if(clip.Equals(end))
        {
            //audioSource.loop = true;
            audioSource.PlayOneShot(end);
        }
    }

    public override void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = isAudio3D ? 1 : 0;
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
