using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Player;
// using HurricaneVR.Framework.Components;

public class GrabHelper : MonoBehaviour
{
    public Rigidbody rb;
    public Collider[] colliders;
    public HVRPlayerController player;

    public GameObject otherHVRGrabbableGO;
    private float initialPlayerAcceleration;
    private float intialPlayerMovementSpeed;
    private float intitalPlayerTurnSpeed;
    private float initialPlayerDeceleration;

    // Start is called before the first frame update
    void Start()
    {
        initialPlayerAcceleration = player.Acceleration;
        intialPlayerMovementSpeed = player.MoveSpeed;
        intitalPlayerTurnSpeed = player.SmoothTurnSpeed;
        initialPlayerDeceleration = player.Deacceleration;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerAcceleration(float a)
    {
        player.Acceleration = a;
    }

    public void SetPlayerAccelerationToInitialValue()
    {
        SetPlayerAcceleration(initialPlayerAcceleration);
    }

    public void SetPlayerSpeed(float s)
    {
        player.MoveSpeed = s;
    }
    public void SetPlayerSpeedToInitialVallue()
    {
        SetPlayerSpeed(intialPlayerMovementSpeed);
    }

    public void SetPlayerTurnSpeed(float s)
    {
        player.SmoothTurnSpeed = s;
    }
    public void SetPlayerTurnSpeedToInitialValue()
    {
        SetPlayerTurnSpeed(intitalPlayerTurnSpeed);
    }

    public void SetPlayerDeceleration(float d)
    {
        player.Deacceleration = d;
    }
    public void SetPlayerDecelerationToInitialValue()
    {
        player.Deacceleration = initialPlayerDeceleration;
    }



    public void IgnorePlayerCollision(bool doit)
    {
        
        if(doit)
        {
            foreach(Collider col in colliders)
            {
                col.gameObject.layer = 22;
            }
        }
        else
        {
            foreach(Collider col in colliders)
            {
                col.gameObject.layer = 12;
            }
        }
        
        
    }

    // public void EnableOther()
    // {
    //     otherHVRGrabbable.GetComponent<HVRGrabbable>().enabled = true;
    // }
    // public void DisableOther()
    // {
    //     otherHVRGrabbable.GetComponent<HVRGrabbable>().enabled = false;
    // }

    public void SetOtherKinematic(bool state)
    {
        otherHVRGrabbableGO.GetComponent<Rigidbody>().isKinematic = state;
        otherHVRGrabbableGO.GetComponent<Rigidbody>().useGravity = !state;
    }

    public void SetKinematic(bool state)
    {
        rb.isKinematic = state;
    }

    public void SetWeight(int weight)
    {
        rb.mass = weight;
    }

    public void LockRB()
    {
        
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }

    public void UnlockRB()
    {
        rb.constraints = RigidbodyConstraints.None;
        
    }
}
