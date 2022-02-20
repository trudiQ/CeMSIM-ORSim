using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Obi;


public class RodGuideBehavior : MonoBehaviour
{
    public float minDistanceFromAnotherGuide = 0.5f;
    
    NeedleBehavior focusedNeedle;
    RodBlueprintParticleIndividualizer focusedRod;
    int layerMask = 1 << 13;

    static List<RodGuideBehavior> activeRodGuides = new List<RodGuideBehavior>();
    static UnityEvent e_stoppedTracking = new UnityEvent();

    private ObiCollider[] obiColliders;
    private bool tracking = true;

    private SlidingAttachmentField slidingAttachment;

    public void Init(NeedleBehavior needle, RodBlueprintParticleIndividualizer rod)
    {
        focusedNeedle = needle;
        focusedRod = rod;

        obiColliders = GetComponentsInChildren<ObiCollider>();
        SetRodCollisions(false);

        slidingAttachment = GetComponentInChildren<SlidingAttachmentField>();

        activeRodGuides.Add(this);
    }

    private bool currentColliderStatus = true;
    private void SetRodCollisions(bool enabled)
    {
        if (enabled ^ currentColliderStatus)
        {
            foreach (ObiCollider c in obiColliders)
            {
                if(!c.sourceCollider.isTrigger)
                    c.enabled = enabled;
            }
            currentColliderStatus = enabled;
        }
    }

    Vector3 lastFoundPosition = Vector3.zero;
    Vector3 closestParticlePosition = Vector3.zero;
    Vector3 closestPointOnCollider = Vector3.zero;
    void FixedUpdate()
    {
        if (!tracking) return;

        lastFoundPosition = focusedRod.WorldPositionOfParticle(focusedRod.particleCount - 1);
        closestParticlePosition = focusedNeedle.pointTransform.position;

        Vector3 lookVector = (closestParticlePosition - lastFoundPosition).normalized;

        RaycastHit hit;
        Ray ray = new Ray(lastFoundPosition, lookVector);
        if(Physics.Raycast(ray, out hit, 0.5f, layerMask))
        {
            if (hit.collider.tag == "Needle" || hit.collider.name == "GameObject")
            {
                Debug.Log("??? " + hit.transform.parent.name);
                return;
            }
            if (hit.distance < 0.1f) SetRodCollisions(true);

            //Ensure guide does not interfere with another guide
            bool tooClose = false;
            foreach(RodGuideBehavior guide in activeRodGuides)
            {
                if (guide == this) continue;
                if (Vector3.Distance(hit.point, guide.transform.position) < minDistanceFromAnotherGuide)
                {
                    tooClose = true;
                    break;
                }
            }

            if(!tooClose)
                transform.position = hit.point;
            
            transform.rotation = Quaternion.LookRotation(Vector3.forward, hit.normal);
        }
        else
        {
            /*
            if (Vector3.Distance(transform.position, lastFoundPosition) > 0.1f)
                transform.position = Vector3.up * -100f;
                */
        }
    }

    public void OnDestroy()
    {
        activeRodGuides.Remove(this);
    }

    private void EnableSlidingAttachmentField()
    {
        slidingAttachment.isAttaching = true;
    }
    public void StopTracking()
    {
        tracking = false;
        //e_stoppedTracking.Invoke();
        e_stoppedTracking.AddListener(EnableSlidingAttachmentField); 
    }
    public void DestroySelf() { Destroy(gameObject); }
}
