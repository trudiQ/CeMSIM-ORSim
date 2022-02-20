using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Obi;

public class StaticHoleColliderManager : MonoBehaviour
{
    public Transform pseudoColonCollider;

    public GameObject p_Anvil;
    public bool createAnvil = true;
    public Transform createAnvilAt;
    public Transform CenterPullTowards;
    public float innerSpeedMultiplier = 0.75f;
    public float outerSpeedMultiplier = 0.75f;
    public float slowDownMultiplier = 3;
    public float outerHaltDistance = 0.25f;
    public float innerHaltDistance = 0.75f;
    public bool useParticleAttachments = false;
    public bool adjustOuterPositions = true;

    public RodBlueprintParticleIndividualizer rod;
    public List<ObiParticleAttachment> rodAttachments = new List<ObiParticleAttachment>();
    public BlueprintParticleIndividualizer softbody;

    public List<StaticHoleColliderBehavior> outerHoles = new List<StaticHoleColliderBehavior>();
    public List<StaticHoleColliderBehavior> inner = new List<StaticHoleColliderBehavior>();
    public void AddOuter(StaticHoleColliderBehavior hole){ outerHoles.Add(hole);}
    public void AddInner(StaticHoleColliderBehavior hole) { inner.Add(hole); }

    public void RemoveFromManager(StaticHoleColliderBehavior staticHoleCollider, bool isInner)
    {
        if (isInner)
        {
            if (inner.Contains(staticHoleCollider))
            {
                inner.Remove(staticHoleCollider);
            }
            else
            {
                Debug.LogError("Tried to remove a hole that was not managed.");
            }

        }
        else
        {
            if (outerHoles.Contains(staticHoleCollider))
            {
                outerHoles.Remove(staticHoleCollider);
            }
            else
            {
                Debug.LogError("Tried to remove a hole that was not managed.");
            }
        }
    }

    public UnityEvent _E_EndOfRodReached;
    public UnityEvent _E_OnPullStart;
    public UnityEvent _E_OnPullFinish;

    private bool isPulling = false;
    
    private void Update()
    {
        //cheat
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartPullingIgnoreTension();
        }
    }

    public void AddRodAttachments()
    {
        for(int i =0; i< inner.Count; i++)
        {
            if (inner[i] == null) inner.RemoveAt(i);
                
        }
        for (int i = 0; i < inner.Count; i++)
        {
            rodAttachments.Add(rod.CreateNewParticleAttachmentClosestTo(inner[i].transform));
        }
    }

    public void StartPullingBehaviour()
    {
        if (isPulling) return;

        //Attach rod particles to colliders
        AddRodAttachments();
        foreach (StaticHoleColliderBehavior s in inner)
        {
            rod.CreateNewParticleAttachmentClosestTo(s.transform);
            s.DisableColliders();
        }

        if (useParticleAttachments)
        {
            foreach (StaticHoleColliderBehavior outerHole in outerHoles)
            {
                softbody.MoveAndCreateParticleAttachmentTo(outerHole.transform);
            }
        }
        StartCoroutine(DoPull(outerHoles, outerHaltDistance, outerSpeedMultiplier));

        //Disable colon colliders
        if (pseudoColonCollider != null)
        {
            pseudoColonCollider.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No pseudo collider assigned to " + gameObject.name);
        }

        if (createAnvil)
        {
            Instantiate(p_Anvil, createAnvilAt.position, createAnvilAt.rotation, softbody.transform.parent);
        }
        
        foreach(StaticHoleColliderBehavior i in inner)
        {
            i.heldAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
            i.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        StartCoroutine(DoPull(inner, innerHaltDistance, innerSpeedMultiplier));

        softbody.instance.deformationResistance = 0.2f;
        isPulling = true;
        _E_OnPullStart.Invoke();
    }
    
    public void StartPullingIgnoreTension()
    {
        _tensionOverride = true;
        StartPullingBehaviour();
    }

    public void StopPulling()
    {
        _override = true;
    }

    private bool _tensionOverride = false;
    private bool _override = false;
    IEnumerator DoPull(List<StaticHoleColliderBehavior> holes, float haltDistance, float speedMultiplier)
    {
        int startParticle = rodAttachments[rodAttachments.Count - 1].particleGroup.particleIndices[0] + 1;
        int endParticle = rod.solver.positions.count - 1;

        float vel = 0;
        float rodTension = 0;
        float rodLength = 0;
        float rodRestLength = 0;
        for (int i = startParticle; i < endParticle; i++)
        {
            rodRestLength += Vector4.Distance(rod.solver.restPositions[i], rod.solver.restPositions[i + 1]);
        }

        float startPullRodTension = 0;
        for (int i = startParticle; i < endParticle; i++)
        {
            startPullRodTension += Vector4.Distance(rod.solver.positions[i], rod.solver.positions[i + 1]);
        }
        startPullRodTension = startPullRodTension / rodRestLength;

        while (true)
        {
            bool allHalting = true;
            Vector3 path = Vector3.zero;
            foreach (StaticHoleColliderBehavior s in holes)
            {
                float dist = Vector3.Distance(CenterPullTowards.position, s.transform.position);
                if (dist < haltDistance)
                    continue;

                path = (CenterPullTowards.position - s.transform.position).normalized;
                
                if (_tensionOverride)
                    rodTension = 1.1f;
                else
                {
                    //Calculate tension only in the manipulatable part of the rod
                    rodLength = 0;
                    for (int i = startParticle; i < endParticle; i++)
                    {
                        rodLength += Vector4.Distance(rod.solver.positions[i], rod.solver.positions[i + 1]);
                    }
                    rodTension = rodLength / rodRestLength;
                }

                if (rodTension > startPullRodTension)
                {
                    vel = (rodTension - startPullRodTension) * speedMultiplier;
                }
                else
                {
                    vel = 0;
                }

                path *= Time.deltaTime * vel * dist;

                s.transform.Translate(path, Space.World);
                allHalting = false;
            }
            if (allHalting || _override)
            {
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        _E_OnPullFinish.Invoke();
        //Destroy(this);
    }

}
