using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAttachmentTypeChanger : MonoBehaviour
{
    public List<ObiParticleAttachment> attachments;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakeStatic() {
        foreach(ObiParticleAttachment attachment in attachments) {
            attachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        }
    }

    public void MakeDynamic() {
        foreach(ObiParticleAttachment attachment in attachments) {
            attachment.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
        }
    }
}
