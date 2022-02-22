using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Obi;
using Utility;

public class OperationManager : MonoBehaviour
{
    public static OperationManager instance = null;

    static Mesh s_anvilColonMesh;

    [Header("Prefabs")]
    public StaticHoleColliderBehavior p_attachObject;
    public StaticHoleColliderManager p_staticHoleColliderManager;
    public GameObject p_sutureRodNeedlePrefab;
    public GameObject p_anvil;
    public ObiCollider p_pseudoColonCollider;
    public ForcepBehavior p_needleHolder;

    [Header("Managers")]
    public HapticColonGrabbersBehavior grabberControl;
    private StaticHoleColliderManager anvilColonHoleColliderManager = null;

    [Header("Scene References")]
    public BlueprintParticleIndividualizer anvilColon;
    public SkinnedMeshRenderer anvilColonSkin;
    public NeedleBehavior anvilNeedle;
    public RodBlueprintParticleIndividualizer anvilRod;
    public GameObject CircularStaplerPair;
    public CircularStaplerBehavior circularStapler;
    public Transform alignReference;

    [Header("Created During Simulation")]
    public GameObject anvil;

    [Header("Demo Objects - To be removed")]
    public HapticGrabber p_grabber;

    private int _currentPhase = 0;

    private GameObject activePseudoColonCollider;

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else instance = this;
    }

    private void Start()
    {
        //Go to phase 1
        AdvancePhase();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Advancing to phase " + (_currentPhase + 1));
            AdvancePhase();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void AdvancePhase()
    {
        switch (++_currentPhase)
        {
            case 1:
                PhaseOneStart();
                break;
            case 2:
                PhaseTwoStart();
                break;
            case 3:
                PhaseThreeStart();
                break;
            case 4:
                PhaseFourStart();
                break;
            case 5:
                PhaseFiveStart();
                break;
            default:
                Debug.Log(this.ToString() + " Is at invalid phase!");
                break;
        }
    }

    //Grab and secure anvil side colon
    //Disable Needle
    public void PhaseOneStart()
    {
        anvilNeedle.enabled = false;
        grabberControl.SetForcepSpawning(true);
    }

    List<int> foundParticles;
    //Suture anvil side colon
    /*
     * Find side of colon held open by curved forceps,
     * spawn StaticHoleColliderManager and enable/create suture needle
     */
    public void PhaseTwoStart()
    {
        //Haptic device gameobject may be disabled when debugging
        if (grabberControl.gameObject.activeSelf)
        {
            grabberControl.SetForcepSpawning(false);
            grabberControl.canGrabObi = false;
            grabberControl.SwitchGrabberObject(p_needleHolder);
        }

        //Disable forcep colliders to prevent explosions AND
        //Get average position of forceps
        Vector3 averagePosition = Vector3.zero;
        foreach(ForcepBehavior f in grabberControl.attachedForceps)
        {
            ObiCollider[] foundColliders = f.GetComponentsInChildren<ObiCollider>();
            foreach(ObiCollider col in foundColliders)
            {
                col.enabled = false;
                col.sourceCollider.enabled = false;
            }

            averagePosition += f.transform.position;
        }
        if(grabberControl.attachedForceps.Count > 0)
            averagePosition /= grabberControl.attachedForceps.Count;

        anvilColonHoleColliderManager = Instantiate(p_staticHoleColliderManager, averagePosition, Quaternion.identity, transform);
        anvilColonHoleColliderManager.softbody = anvilColon;
        anvilColonHoleColliderManager.rod = anvilRod;
        anvilNeedle.holeManager = anvilColonHoleColliderManager;

        //Set up the anvil suture to advance the phase of the operation once
        //the end of the line is reached
        anvilColonHoleColliderManager._E_EndOfRodReached.AddListener(AdvancePhase);

        anvilNeedle.enabled = true;
        anvilNeedle.suturable = anvilColon;
        anvilNeedle.rod = anvilRod;

        //Create the pseudo colon collider for rod collisions
        activePseudoColonCollider = anvilColon.CreateColliderSnapshot(p_pseudoColonCollider);
    }

    //Place anvil
    public void PhaseThreeStart()
    {
        anvil = Instantiate(
            p_anvil,
            Vector3.zero,
            Quaternion.identity);

        //Set pull towards from reference in anvil
        GameObject pullTowards = anvil.GetComponent<GameObjectReference>().reference;
        anvilColonHoleColliderManager.CenterPullTowards = pullTowards.transform;

        grabberControl.SwitchToObject(anvil);
        grabberControl.SetForcepSpawning(true);
    }

    //Begin Closing of the colon
    public void PhaseFourStart()
    {
        //Destroy pseudo colon collider
        Destroy(activePseudoColonCollider);

        //Remove rod attachment to end-of-line gameobject
        Destroy(anvilRod.GetComponent<ParticleAttachmentReference>().reference);

        //Remove forceps holding colon in place
        foreach(ForcepBehavior f in grabberControl.attachedForceps)
        {
            f.DestroyAttachment();
            Destroy(f.gameObject);
        }

        //Remove attachment between holecolliders and colon
        foreach(StaticHoleColliderBehavior staticHoleCollider in anvilColonHoleColliderManager.inner)
        {
            Destroy(staticHoleCollider.heldAttachment);
        }


        foundParticles =
            anvilColon.GetClosestRestingSliceOfParticles(
                Vector3.back * 999f,
                0.2f,
                true);

        List<GameObject> createdOuterRing = new List<GameObject>();
        foreach (int i in foundParticles)
        {
            StaticHoleColliderBehavior outerPart = Instantiate(
                p_attachObject, 
                anvilColon.WorldPositionOfParticle(i), 
                Quaternion.identity, 
                anvilColonHoleColliderManager.transform);
            anvilColonHoleColliderManager.AddOuter(outerPart);
            createdOuterRing.Add(outerPart.gameObject);
        }

        //Done, tell collider manager to start closing
        anvilColonHoleColliderManager.StartPullingBehaviour();

        //Enable rod normalizer so stretching artifacts are minimized
        /*
        ObiParticleAttachment[] rodAttachments = anvilRod.GetComponents<ObiParticleAttachment>();
        ObiParticleAttachment needleAttachment = null;
        foreach (ObiParticleAttachment p in rodAttachments)
        {
            if (p.particleGroup.name == "needleEnd")
            {
                needleAttachment = p;
            }
        }
        ObiParticleAttachment lastHoleCollider = null;
        lastHoleCollider = anvilColonHoleColliderManager.rodAttachments[anvilColonHoleColliderManager.rodAttachments.Count - 1];

        if (needleAttachment == null || lastHoleCollider == null) Debug.LogError("OperationManager - Getting rod strech range failed!");
        
        anvilRod.GetComponent<RodStretchNormalizer>().EnableAndLimitBetweenExclusively(
            lastHoleCollider.particleGroup.particleIndices[0],
            needleAttachment.particleGroup.particleIndices[0]);
            */

        //Disable rod smoothing if it is present as it can cause weird behavior when combined with the buncher
        ObiPathSmoother pathSmoother = anvilRod.GetComponent<ObiPathSmoother>();
        if (pathSmoother)
        {
            pathSmoother.decimation = 0;
            pathSmoother.smoothing = 0;
            pathSmoother.twist = 0;
        }

        //Enable rod bunching to eliminate extra rod slack between suture points
        anvilRod.GetComponent<RodBuncher>().EnableAndDoScanFrom(anvilColonHoleColliderManager.rodAttachments);
    }

    //Use circular stapler
    private void PhaseFiveStart()
    {
        //Switch colon attachment to anvil
        foreach(StaticHoleColliderBehavior staticHoleCollider in anvilColonHoleColliderManager.outerHoles)
        {
            if (staticHoleCollider == null) continue;
            staticHoleCollider.heldAttachment.enabled = false;
            Destroy(staticHoleCollider.heldAttachment.target.gameObject);
            staticHoleCollider.heldAttachment.target = anvil.transform;
            staticHoleCollider.heldAttachment.enabled = true;
        }

        CircularStaplerPair.SetActive(true);

        //Align anvil
        //As the anvil is going to be moving, there is a chance the edges might pop through the colon.
        //Switch the material the largest part of the anvil uses for a transparent one.
        MaterialSwitcher switcher = anvil.GetComponentInChildren<MaterialSwitcher>();
        if (switcher)
            switcher.SwitchMaterials();
        else
            Debug.LogWarning("Switcher not found!");

        //Reassign parent of the rodguides to the anvil as it will be moving
        foreach(StaticHoleColliderBehavior hole in anvilColonHoleColliderManager.inner)
        {
            hole.transform.SetParent(anvil.transform);
        }

        //Find nearest point on circular stapler colon's normal to place the anvil on
        Vector3 anvilNewPosition = MathHelper.NearestPointOnLine(
            alignReference.transform.position,
            alignReference.transform.forward,
            anvil.transform.position);

        //Orient it to be aligned 
        Quaternion anvilNewOrientation = Quaternion.LookRotation(
            alignReference.transform.forward * -1,
            alignReference.transform.up);

        //Finally, align the anvil over two seconds.
        GameObjectManipulator manipulator = anvil.GetComponent<GameObjectManipulator>();
        if (manipulator)
            manipulator.AlignOverTime(anvilNewPosition, anvilNewOrientation, 2f);

        //Circular stapler needs to lock on to the anvil once it is done aligning
        manipulator.e_OnCoroutineFinish.AddListener(circularStapler.StartLockOn);
        circularStapler.lockTargetPoint = anvil.transform;

        //This will need to be removed at some point
        MeshRenderer[] meshes = grabberControl.activeForceps.GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer m in meshes)
        {
            m.enabled = false;
        }

        //This, too.
        ObiCollider[] colliders = grabberControl.activeForceps.GetComponentsInChildren<ObiCollider>();
        foreach (ObiCollider c in colliders)
        {
            c.enabled = false;
        }

        grabberControl.enabled = false;
    }
}
