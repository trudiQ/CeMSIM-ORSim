using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Obi;

public class HumerusRodBehavior : MonoBehaviour
{
    private ObiRod rod;
    private Dictionary<string, ObiParticleAttachment> particleAttachments = new Dictionary<string, ObiParticleAttachment>();

    private void Awake()
    {
        rod = GetComponentInChildren<ObiRod>();
        List<ObiParticleAttachment> attachments = rod.GetComponents<ObiParticleAttachment>().ToList();
        foreach(ObiParticleAttachment a in attachments)
        {
            particleAttachments.Add(a.particleGroup.name, a);
        }
    }

    public void AttachControlPointTo(string point, Transform t)
    {
        if (!particleAttachments.ContainsKey(point))
        {
            Debug.LogError("Can not find control point " + point + " in particle attachments!");
            return;
        }
        particleAttachments[point].target = t;
    }

    public void DisableParticleAttachment(string name)
    {
        if (!particleAttachments.ContainsKey(name))
        {
            Debug.LogError("Can not find name " + name + " in particle attachments!");
            return;
        }
        particleAttachments[name].enabled = false;
    }
}
