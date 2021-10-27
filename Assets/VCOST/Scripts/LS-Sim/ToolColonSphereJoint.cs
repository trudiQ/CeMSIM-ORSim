using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic and data storage for joints created between haptic tools and the colon spheres for grab and push physics interaction
/// </summary>
public class ToolColonSphereJoint : MonoBehaviour
{
    public Joint joint;
    public float breakPushingJointDistance;
    public Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateJointStatus();
    }

    public void UpdateJointStatus()
    {
        if (Vector3.Distance(transform.position, initialPosition) > breakPushingJointDistance)
        {
            Destroy(gameObject);
        }
    }
}
