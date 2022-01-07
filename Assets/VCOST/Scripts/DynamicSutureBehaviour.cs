using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class DynamicSutureBehaviour : MonoBehaviour
{
    private NeedleState currentState;
    private Rigidbody r;
    private ObiParticleAttachment currentAttachedParticle;
    private GameObject currentSuturing;

    public ExitTriggerBehaviour exitTrigger;

    public ObiSolver solver;
    public Material sutureLineMaterial;
    public ObiRopeSection ropeSection;

    public List<ObiRope> sutureRopes;

    public GameObject p_sutureLine;
    public ObiRope currentRope;
    private ObiRopeBlueprint blueprintSutureLine;
    private ObiRopeExtrudedRenderer ropeRenderer;

    private IEnumerator blueprintCreator;

    enum NeedleState
    {
        Free,
        Suturing,
        Exited
    }

    // Start is called before the first frame update
    void Start()
    {
        currentState = NeedleState.Free;
        r = gameObject.GetComponent<Rigidbody>();
        sutureRopes = new List<ObiRope>();

        ObiParticleAttachment[] attachments = currentRope.GetComponentsInChildren<ObiParticleAttachment>();
        foreach(ObiParticleAttachment p in attachments)
        {
            if(p.target == transform)
            {
                currentAttachedParticle = p;
                break;
            }
        }
    }

    private Collider sutureCollider;
    private Transform needleObject;
    private ObiParticleAttachment endAttachment;
    private IEnumerator CreateSutureLine()
    {
        currentRope = Instantiate(p_sutureLine, solver.transform).AddComponent<ObiRope>();
        currentRope.selfCollisions = true;

        // Create both the rope and the solver:	
        ropeRenderer = currentRope.gameObject.AddComponent<ObiRopeExtrudedRenderer>();
        ropeRenderer.section = ropeSection;
        ropeRenderer.uvScale = new Vector2(1, 5);
        ropeRenderer.normalizeV = false;
        ropeRenderer.uvAnchor = 1;
        ropeRenderer.thicknessScale = 0.2f;
        currentRope.GetComponent<MeshRenderer>().material = sutureLineMaterial;

        // Setup a blueprint for the rope:
        blueprintSutureLine = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprintSutureLine.resolution = 5f;

        // Tweak rope parameters:
        currentRope.maxBending = 0.05f;
        currentRope.stretchCompliance = 0f;
        currentRope.stretchingScale = 0f;
        currentRope.maxCompression = 0f;

        yield return 0;
        //Vector3 localHit = currentRope.transform.InverseTransformPoint(sutureCollider.ClosestPoint(needleObject.position));
        Vector3 localHit = (sutureCollider.transform.position - transform.position).normalized;

        RaycastHit hit;
        Physics.Raycast(sutureCollider.transform.position, localHit, out hit);
        localHit = hit.normal;

        // Procedurally generate the rope path (a simple straight line):
        blueprintSutureLine.path.Clear();
        blueprintSutureLine.path.AddControlPoint(sutureCollider.ClosestPoint(needleObject.position), Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, 1, Color.white, "Hook start");
        blueprintSutureLine.path.AddControlPoint(exitTrigger.transform.position, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, 1, Color.white, "Hook end");
        blueprintSutureLine.path.FlushEvents();

        // Generate the particle representation of the rope (wait until it has finished):
        yield return blueprintSutureLine.Generate();

        // Set the blueprint (this adds particles/constraints to the solver and starts simulating them).
        currentRope.ropeBlueprint = blueprintSutureLine;
        currentRope.GetComponent<MeshRenderer>().enabled = true;

        ObiParticleAttachment startAttachment = currentRope.gameObject.AddComponent<ObiParticleAttachment>();
        startAttachment.target = sutureCollider.transform;
        startAttachment.particleGroup = currentRope.blueprint.groups.Find(x => x.name == "Hook start");
        startAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;

        endAttachment = currentRope.gameObject.AddComponent<ObiParticleAttachment>();
        endAttachment.target = exitTrigger.transform;
        endAttachment.particleGroup = currentRope.blueprint.groups.Find(x => x.name == "Hook end");
        endAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;

        ObiRopeCursor cursor = currentRope.gameObject.AddComponent<ObiRopeCursor>();
        cursor.cursorMu = 0.1f;
        cursor.sourceMu = 0.5f;
        cursor.direction = true;
        cursor.ChangeLength(currentRope.restLength * 25);
    }

    public void StartSuturing(Collider c)
    {
        currentSuturing = c.gameObject;
        currentState = NeedleState.Suturing;
        gameObject.layer = 8;
    }

    //Exit trigger enter
    public void StopSuturing(Collider c)
    {
        if (currentState != NeedleState.Suturing) return;
        currentState = NeedleState.Exited;

        if (endAttachment == null)
        {
            ObiParticleAttachment[] pas = currentRope.GetComponents<ObiParticleAttachment>();
            foreach(ObiParticleAttachment p in pas)
            {
                if (p.particleGroup.name == "Hook end") endAttachment = p;
            }
        }
        endAttachment.target = c.transform;
    }

    //Exit trigger exit
    public void CreateNewSutureLine(Collider c, Transform n)
    {
        if (currentState != NeedleState.Exited) return;
        needleObject = n;
        sutureCollider = c;
        blueprintCreator = CreateSutureLine();
        while(blueprintCreator.MoveNext());
        currentState = NeedleState.Free;
        gameObject.layer = 0;
    }
}
