using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodPressureMonitorAudioPlayer : AudioPlayer
{
    public float timer = 0.0f;
    public int randomTime = 0;
    private bool paused = false;
    private bool randomSet = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!paused)
        {
            if(!randomSet)
            {
                randomTime = Random.Range(30, 80);
                randomSet = true;
            }
            else if(!audioSource.isPlaying)
            {
                timer += Time.deltaTime;
            }

            if(timer >= randomTime)
            {
                Play();
                randomSet = false;
                timer = 0.0f;
            }
           
        }
    }

    void Play()
    {
        audioSource.PlayOneShot(audioClip);
    }

    public override void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = isAudio3D ? 1 : 0;
        audioSource.volume = volume;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
    }

    public override void Pause()
    {
        audioSource.Pause();
        paused = true;
    }

    public override void UnPause()
    {
        audioSource.UnPause();
        paused = true;
    }
}
