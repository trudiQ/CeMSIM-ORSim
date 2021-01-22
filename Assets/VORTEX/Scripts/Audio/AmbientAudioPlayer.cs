using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioPlayer : AudioPlayer
{
    public override void Initialize()
    {
        if (source == null)
        {
            source = gameObject;
        }
        audioSource = source.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.spatialBlend = source == gameObject ? 0 : 1;
        audioSource.Play();
    }
    public override void  Pause()
    {
        audioSource.Pause();
    }

    public override void UnPause()
    {
        audioSource.Play();
    }
}
