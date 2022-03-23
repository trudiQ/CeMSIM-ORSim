using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableWhenInFrontOfHaptic : MonoBehaviour
{
    public Collider colliderToDisable;

    GameObject hapticDevice;

    // Start is called before the first frame update
    void Start()
    {
        hapticDevice = GameObject.FindObjectOfType<HapticPlugin>().hapticManipulator;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(hapticDevice.transform.position.y < transform.position.y)
        {
            colliderToDisable.enabled = false;
        }else if(hapticDevice.transform.position.y > transform.position.y)
        {
            colliderToDisable.enabled = true;
        }
    }
}
