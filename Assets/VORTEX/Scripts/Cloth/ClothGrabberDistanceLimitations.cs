using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Overview
    This script is used to limit the distance between transforms. The intended use case is to set distance limitations on colliders in a ring-like chain.
    Colliders are switched to kinematic when grabbed, which intiates the chain of influences starting from the grabbed object.
    The chain of influence ends when the opposite influences are detected while propagating influence.
    Releasing the grabbed object causes a lack of influence to be propagated through the chain.
*/
public class ClothGrabberDistanceLimitations : MonoBehaviour
{
    public enum State {InfluenceBoth, InfluencedByPrevious, InfluencedByNext, DoNothing};       
    [SerializeField]
    private State currentState = State.DoNothing;                                                

    [Tooltip("Used to reference previous transform in collider chain")]
    public Transform previous;
    
    [Tooltip("Used to reference next transform in collider chain.")]
    public Transform next;

    [Tooltip("Used to determine whether or not the maximum distance between this transform and previous/next is automatically calculated at runtime.")]
    public bool autoCalculateMaxDistance = true;

    [Tooltip("Maximum distance allowed between this transform and the previous transform.")]
    public float maxDistancePrevious;

    [Tooltip("Maximum distance allowed between this transform and the next transform.")]
    public float maxDistanceNext;

    [Tooltip("Maximum angle allowed before this transform should influence the previous or next transform.")]
    public float angleThreshold;
    [Tooltip("Current angle between at this collider. Debugging only.")]
    public float currentAngle = 0f;
    private Rigidbody rb;
    private ClothGrabberDistanceLimitations previousLimiter;
    private ClothGrabberDistanceLimitations nextLimiter;
    private enum InfluenceType {Previous, Next}

    void Start()
    {
        if(autoCalculateMaxDistance)
        {
            Vector3 dP = previous.position - this.transform.position;
            Vector3 dN = next.position - this.transform.position;
            maxDistancePrevious = dP.magnitude;
            maxDistanceNext = dN.magnitude;
        }
        
        rb = this.GetComponent<Rigidbody>();
        previousLimiter = previous.gameObject.GetComponent<ClothGrabberDistanceLimitations>();
        nextLimiter = next.gameObject.GetComponent<ClothGrabberDistanceLimitations>();
    }

    void LateUpdate()
    {
        if(!rb.isKinematic)
        {
            ChangeState(State.InfluenceBoth);
            // PropagateInfluence();
            InfluenceNext();
            InfluencePrevious();
        }

        if(CheckState(State.InfluencedByPrevious))
        {
            InfluenceNext();
        }
        else if(CheckState(State.InfluencedByNext))
        {
            InfluencePrevious();
        }
    }

    ///<summary> Used to check the passed in state against the current state. Returns true if they match. </summary>
    public bool CheckState(State s)
    {
        if(currentState.Equals(s))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    ///<summary> Used to manage distance limitation state of this object. </summary>
    public void ChangeState(State newstate)
    {
        currentState = newstate;
    }

    ///<summary>Propagates influence to preceding colliders until the opposing influence type is encountered.</summary>
    public void PropagatePrevious()
    {
        if(!previousLimiter.CheckState(State.InfluencedByNext))
        {
            previousLimiter.ChangeState(State.InfluencedByPrevious);
            previousLimiter.PropagatePrevious();
        }
    }

    ///<summary>Propagates influence to following colliders until the opposing influence type is encountered.</summary>
    public void PropagateNext()
    {
        if(!nextLimiter.CheckState(State.InfluencedByPrevious))
        {
            nextLimiter.ChangeState(State.InfluencedByPrevious);
            nextLimiter.PropagateNext();
        }
    }

    ///<summary>Stops all objects in chain from influencing each other.</summary>
    public void PropagateRelease()
    {
        if(!CheckState(State.DoNothing))
        {
            ChangeState(State.DoNothing);
            nextLimiter.PropagateRelease();
        }
    }

    /// <summary>Applies distance limiations on next object in chain by calling InfluenceOther() for each axis.</summary> 
    public void InfluencePrevious()
    {
        previous.position = InfluenceOther(previous.position, Vector3.up, maxDistancePrevious, InfluenceType.Previous);
        previous.position = InfluenceOther(previous.position, Vector3.right, maxDistancePrevious, InfluenceType.Previous);
        previous.position = InfluenceOther(previous.position, Vector3.forward, maxDistancePrevious, InfluenceType.Previous);

        if(!previousLimiter.CheckState(State.InfluencedByPrevious))
        {
            previousLimiter.ChangeState(State.InfluencedByNext);
        } 
    }

    /// <summary>Applies distance limiations on previous object in chain by calling InfluenceOther() for each axis.</summary>
    public void InfluenceNext()
    {
        next.position = InfluenceOther(next.position, Vector3.up, maxDistanceNext, InfluenceType.Next);
        next.position = InfluenceOther(next.position, Vector3.right, maxDistanceNext, InfluenceType.Next);
        next.position = InfluenceOther(next.position, Vector3.forward, maxDistanceNext, InfluenceType.Next);

        if(!nextLimiter.CheckState(State.InfluencedByNext))
        {
            nextLimiter.ChangeState(State.InfluencedByPrevious);
        }  
    }

    ///<summary> Used to limit the distance between this object and another object in the chain.</summary>
    private Vector3 InfluenceOther(Vector3 position, Vector3 comparison, float maxDistance, InfluenceType type)
    {
        Vector3 distance = position - this.transform.position;
        float theta = Vector3.Dot(distance, comparison);
        currentAngle = theta;

        if(distance.magnitude > maxDistance)
        {
            distance.Normalize();
            distance *= maxDistance;
            position = distance + this.transform.position;
        }
        else if(distance.magnitude < .1f)
        {
            distance.Normalize();
            distance *= .1f;
            position = distance + this.transform.position;
        }
        

        if(type.Equals(InfluenceType.Previous))
        {
            Debug.DrawRay(transform.position, distance, Color.green);
        }
        else if(type.Equals(InfluenceType.Next))
        {
            Debug.DrawRay(transform.position, distance, Color.red);
        }
        
        return position;
    }
}
