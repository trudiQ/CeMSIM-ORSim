using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(BlueprintParticleIndividualizer))]
public class HapticColonGrabber : MonoBehaviour
{
    public GameObject hapticGrabberEnd;
    public float maxGrabberDistance = 0.1f;

    private BlueprintParticleIndividualizer colon;
    private ObiParticleAttachment attachmentToGrabber = null;

    // Start is called before the first frame update
    void Start()
    {
        colon = gameObject.GetComponent<BlueprintParticleIndividualizer>();
    }

    public void AttachClosestParticleToGrabber()
    {
        if(colon.GetDistanceToClosestParticle(hapticGrabberEnd.transform.position) < maxGrabberDistance)
            attachmentToGrabber = colon.CreateNewParticleAttachmentClosestTo(hapticGrabberEnd.transform);
    }

    public void ReleaseAttachmentToGrabber()
    {
        if(attachmentToGrabber!=null)
            Destroy(attachmentToGrabber);
    }
}
