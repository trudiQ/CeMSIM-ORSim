using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Obi;

public class NeedleBehavior : MonoBehaviour
{

    private bool currentlySuturing = false;
    public int categoryColon = 15;
    private int collideWithColonFilter = 0;
    private int noCollideWithColonFilter = 0;

    public bool spawnAnimation = true;
    public int animationAttach = 1;
    public int animationRing = 4;
    public float animationBreakDistance = 0.35f;
    public Transform pointTransform;
    public Transform exitTransform;
    public bool attachHoleColliderToParticle = false;
    public StaticHoleColliderManager holeManager;
    public StaticHoleColliderBehavior p_HoleCollider;
    public BlueprintParticleIndividualizer suturable;
    public RodBlueprintParticleIndividualizer rod;
    public bool debug = false;
    public Transform forwardHolder;
    private Transform world;

    private bool currentlyAnimating = false;
    private List<ObiParticleAttachment> currentAnimatedParticleAttachments;
    private Vector3 animationOrigin = Vector3.zero;

    private ObiColliderWorld colliderWorld;
    public Transform needleColliderParent;
    private List<ObiCollider> needleColliders;
    private void SetCollideWithColon(bool collide)
    {
        int decision = collide ? collideWithColonFilter : noCollideWithColonFilter;
        foreach (ObiCollider c in needleColliders)
        {
            //http://obi.virtualmethodstudio.com/forum/thread-3009-post-10369.html#pid10369
            //This should be in the docs
            //Rod is 0
            c.Filter = decision;
        }
    }

    public UnityEvent e_needleGrab = new UnityEvent();
    public UnityEvent e_needleRelease = new UnityEvent();

    private Rigidbody rigidbody;
    private RigidbodyConstraints defaultConstraints;

    private void Awake()
    {
        world = GameObject.Find("OniCollisionWorld").transform;
        rigidbody = gameObject.GetComponent<Rigidbody>();
        defaultConstraints = rigidbody.constraints;

        colliderWorld = ObiColliderWorld.GetInstance();
        needleColliders = needleColliderParent.GetComponentsInChildren<ObiCollider>().ToList();
        collideWithColonFilter = ObiUtils.MakeFilter(0x0000fffe, categoryColon);
        noCollideWithColonFilter = ObiUtils.MakeFilter(0x0000fffe, categoryColon-1);
        SetCollideWithColon(true);
    }

    private void Start()
    {

    }

    private Transform _debugOwner = null;
    private Vector3 _debugPosPart = Vector3.zero;
    private Vector3 _debugPartDir = Vector3.zero;
    private void Update()
    {
        if (currentAnimatedParticleAttachments!=null)
        {
            if (Vector3.Distance(animationOrigin, transform.position) > animationBreakDistance)
            {
                foreach(ObiParticleAttachment p in currentAnimatedParticleAttachments)
                {
                    Destroy(p);
                }
                currentAnimatedParticleAttachments = null;
            }
        }
    }

    public void FreezeRigidBody()
    {
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void UnFreezeRigidBody()
    {
        rigidbody.constraints = defaultConstraints;
    }

    public void OnPointEnter(Oni.Contact contact, int particleIndex)
    {
        if (currentlySuturing) return;
        Transform owner = colliderWorld.colliderHandles[contact.bodyB].owner.transform;
        _debugOwner = owner;
        Vector3 desiredHeading = (suturable.WorldPositionOfParticle(contact.bodyA) - owner.position).normalized;

        float d = Vector3.Dot(-owner.up.normalized, desiredHeading);
        if (d < 0.25f) return;

        if (spawnAnimation)
        {
            currentAnimatedParticleAttachments = suturable.CreateRingAttachment(pointTransform, world, animationAttach, animationRing);
            animationOrigin = exitTransform.position;
        }
        SetCollideWithColon(false);

        Vector3 modifiedNeedleEnterPosition = pointTransform.position + (pointTransform.up * -0.2f);
        StaticHoleColliderBehavior holeCollider = Instantiate(p_HoleCollider, modifiedNeedleEnterPosition, pointTransform.rotation);
        holeCollider.AddToManager(holeManager);

        if (attachHoleColliderToParticle)
        {
            holeCollider.heldAttachment = suturable.CreateNewDynamicParticleAttachmentClosestTo(holeCollider.transform);
        }

        NeedleGuideBehavior needleGuide = holeCollider.GetComponent<NeedleGuideBehavior>();
        needleGuide.needle = this;
        currentlySuturing = true;
    }

    public void OnPointExit()
    {
        
    }

    public void OnExitEnter(Oni.Contact contact, int particleIndex)
    {
        DebugMode("Exit - Enter");
        /*
        Vector3 desiredHeading = (StaticHoleColliderManager.instance.CenterPullTowards.position - exitTransform.position).normalized;
        Vector3 actualHeading = (forwardHolder.position - exitTransform.position).normalized;
        Debug.Log(Vector3.Dot(desiredHeading, actualHeading));
        if (Vector3.Dot(desiredHeading, actualHeading) > 0.75)
        {
            StaticHoleColliderBehavior holeCollider = Instantiate(p_HoleCollider, exitTransform.position, exitTransform.rotation);
            holeCollider.StartMovingToPosition(suturable.GetClosestParticlePosition(exitTransform.position));
        }
         */
    }

    public void OnExitExit()
    {
        DebugMode("Exit - Exit");
    }

    public void GuideExited()
    {
        DebugMode("Guide Exited");
        currentlySuturing = false;
        SetCollideWithColon(true);
    }

    void DebugMode(string message)
    {
        if (debug) Debug.Log(message);
    }
}
