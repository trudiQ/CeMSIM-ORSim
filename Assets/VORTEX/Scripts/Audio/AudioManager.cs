using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private List<Component> audioPlayers;
    public bool paused = false;
    private bool playing = true;
    public bool startScenario = false;
    // Start is called before the first frame update
    void Start()
    {
        audioPlayers = new List<Component>();

        foreach(var component in GetComponents<Component>())
        {
            if(component != this && component != GetComponent<Transform>())
            {
                audioPlayers.Add(component);
            }
        }
    }

    public void Update()
    {
        #region Pause - Test Functionality
        //This section is only used for testing.
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (startScenario)
            {
                if (ScenarioManager.Instance.isRunning)
                    ScenarioManager.Instance.PauseScenario();
                else
                    ScenarioManager.Instance.UnPauseScenario();
            }
            else
            {
                if (!paused && playing)
                {
                    playing = false;
                    PauseAudio();
                }
                else if (!playing)
                {
                    playing = true;
                    UnPauseAudio();
                }

                paused = !paused;
            }
        }
        //paused = ScenarioManager.Instance.isRunning;
        #endregion  
    }

    public void PauseAudio()
    {
        foreach(AudioPlayer audioPlayer in audioPlayers)
        {
            audioPlayer.Pause();
        }
    }

    public void UnPauseAudio()
    {
        foreach (AudioPlayer audioPlayer in audioPlayers)
        {
            audioPlayer.UnPause();
        }
    }
}
