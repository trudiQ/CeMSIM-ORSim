using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISoundHelper : AudioPlayer
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = isAudio3D ? 1 : 0;
        audioSource.volume = volume;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
    }

    public void PlayUIBeep()
    {
        audioSource.PlayOneShot(audioClip);
    }

    public override void Pause()
    {
        audioSource.Pause();
    }

    public override void UnPause()
    {
        audioSource.UnPause();
    }
}
