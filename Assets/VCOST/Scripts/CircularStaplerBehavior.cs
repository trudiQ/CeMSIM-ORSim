using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Obi;
using Utility;

public class CircularStaplerBehavior : MonoBehaviour
{
    public HapticInputEventDispatcher hapticDevice;
    public ObiSoftbody softbody;

    public Transform lockSourcePoint;
    public Transform lockTargetPoint;
    public bool lockingOn = false;
    public float lockonDistance;
    public float lockonMaxAngle = 2f;
    public AnimationCurve lockonDistanceFalloff;

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

    public void StartLockOn()
    {
        lockingOn = true;
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
        //worldAttachment.enabled = false;
        //staplerAttachment.enabled = false;

        if (attachmentActive)
        {
            Destroy(particleAttachment);
        }
        else
        {
            Destroy(particleAttachment);
            particleAttachment = softbody.gameObject.AddComponent<ObiParticleAttachment>();
            particleAttachment.target = transform;
            particleAttachment.particleGroup = sutureAttachment.particleGroup;
            particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
            particleAttachment.constrainOrientation = false;
            particleAttachment.enabled = true;
        }
        attachmentActive = !attachmentActive;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (moveTool)
        {
            Vector3 rawMoveVector = hapticPlugin.hapticManipulator.transform.position - originalHapticPosition;

            Vector3 moveVector = rawMoveVector;

            if (lockingOn)
            {
                float lockonInfluence = lockonDistanceFalloff.Evaluate(
                    MathHelper.Map01(
                        Vector3.Distance(lockSourcePoint.position, lockTargetPoint.position), 0, lockonDistance));

                float offset = Vector3.Angle((lockTargetPoint.position - lockSourcePoint.position), lockTargetPoint.forward);

                float angleInfluence = lockonDistanceFalloff.Evaluate(
                    MathHelper.Map01(
                        offset, 0, lockonMaxAngle));

                Vector3 angleCorrectionVector = (lockTargetPoint.position - lockSourcePoint.position).normalized * rawMoveVector.magnitude;

                moveVector = Vector3.Lerp(angleCorrectionVector, rawMoveVector, lockonInfluence);
            }

            transform.Translate(moveVector);
            originalHapticPosition = hapticPlugin.hapticManipulator.transform.position;
        }
    }
}
