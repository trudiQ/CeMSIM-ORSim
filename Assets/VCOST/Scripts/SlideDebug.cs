using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideDebug : MonoBehaviour
{
    public Text invalidDebugText;
    public Text attachedParticlesDebugText;
    public Text gdir;
    public Text v;

    void Update()
    {
        //SetupInvalid();
        //SetUpAttached();
        //gdir.text = SlidingRodAttachment.g_dir.ToString();
    }

    void SetupInvalid()
    {
        if (SlidingRodAttachment.invalidParticles == null) return;
        string debug = "";
        foreach(int i in SlidingRodAttachment.invalidParticles)
        {
            debug += i + ", ";
        }
        invalidDebugText.text = debug;
    }

    void SetUpAttached()
    {
        if (SlidingRodAttachment.totalAttachedParticles == null) return;
        string debug = "";
        foreach(int i in SlidingRodAttachment.totalAttachedParticles)
        {
            debug += i + ", ";
        }
        attachedParticlesDebugText.text = debug;
    }

}
