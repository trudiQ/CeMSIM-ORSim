using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Obi;

public class CircularStaplerBehavior : MonoBehaviour
{
    public HapticInputEventDispatcher hapticDevice;
    public ObiSoftbody softbody;

    private ObiParticleAttachment worldAttachment;
    private ObiParticleAttachment staplerAttachment;
    private ObiParticleAttachment sutureAttachment;
    private HapticPlugin hapticPlugin;
    private bool moveTool = false;

    private void Start()
    {
        hapticPlugin = hapticDevice.GetComponentInChildren<HapticPlugin>();

        ObiParticleAttachment[] particleAttachments = softbody.GetComponents<ObiParticleAttachment>();
        foreach(ObiParticleAttachment attachment in particleAttachments)
        {
            if(attachment.particleGroup.name == "opening")
            {
                worldAttachment = attachment;
            }
            else if(attachment.particleGroup.name == "attach" || attachment.particleGroup.name == "all")
            {
                staplerAttachment = attachment;
            }else if(attachment.particleGroup.name == "suture")
            {
                sutureAttachment = attachment;
            }
        }

        HapticInputEventDispatcher[] inputEventDispatchers = hapticDevice.GetComponents<HapticInputEventDispatcher>();
        foreach(HapticInputEventDispatcher e in inputEventDispatchers)
        {
            if(e.buttonID == 0)
            {
                e.OnButtonPress.AddListener(PrimaryButtonPress);
                e.OnButtonRelease.AddListener(PrimaryButtonRelease);
            }
            else if(e.buttonID == 1)
            {
                e.OnButtonPress.AddListener(SecondaryButtonPress);
            }
        }
    }

    Vector3 originalHapticPosition;
    Vector3 originalHapticRotation;
    void PrimaryButtonPress()
    {
        moveTool = true;
        originalHapticPosition = hapticPlugin.hapticManipulator.transform.position;
        originalHapticRotation = hapticPlugin.hapticManipulator.transform.forward;
    }

    void PrimaryButtonRelease()
    {
        moveTool = false;
    }

    private bool attachmentActive = false;
    private ObiParticleAttachment particleAttachment;
    void SecondaryButtonPress()
    {
        worldAttachment.enabled = false;
        staplerAttachment.enabled = false;

        if (attachmentActive)
        {
            Destroy(particleAttachment);
            particleAttachment = softbody.gameObject.AddComponent<ObiParticleAttachment>();
            particleAttachment.target = worldAttachment.target;
            particleAttachment.particleGroup = sutureAttachment.particleGroup;
            particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
            particleAttachment.constrainOrientation = false;
            particleAttachment.enabled = true;
        }
        else
        {
            Destroy(particleAttachment);
            particleAttachment = softbody.gameObject.AddComponent<ObiParticleAttachment>();
            particleAttachment.target = transform;
            particleAttachment.particleGroup = staplerAttachment.particleGroup;
            particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
            particleAttachment.constrainOrientation = false;
            particleAttachment.enabled = true;
        }
        ObiCollider[] circularStaplerColliders = GetComponentsInChildren<ObiCollider>();
        foreach (ObiCollider c in circularStaplerColliders)
        {
            c.enabled = attachmentActive;
        }
        attachmentActive = !attachmentActive;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (moveTool)
        {
            transform.Translate(hapticPlugin.hapticManipulator.transform.position - originalHapticPosition);
            originalHapticPosition = hapticPlugin.hapticManipulator.transform.position;
            //transform.rotation = Quaternion.FromToRotation(originalHapticRotation, hapticPlugin.hapticManipulator.transform.forward) * transform.rotation;
            //originalHapticRotation = hapticPlugin.hapticManipulator.transform.forward;
        }
    }
}
