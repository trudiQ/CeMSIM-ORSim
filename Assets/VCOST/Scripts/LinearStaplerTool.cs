using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScript.Steps;

public class LinearStaplerTool : Tool
{
    public GameObject FiringHandle;
    public GameObject LockingLever;

    void Update()
    {
        CheckFiringKnob();
        CheckLockingLever();
        Enabler();
    }

    void CheckFiringKnob()
    {
        Vector3 Knob = FiringHandle.transform.localPosition;
        if ((Input.GetKeyUp(KeyCode.T)) && (Knob.x > 0) && (Enable == true))
        {
            Knob.x = -42;
            FiringHandle.transform.localPosition = Knob;
            print("Firing Handle is engaged.");
        }
        else if (Input.GetKeyUp(KeyCode.T) && (Enable == true))
        {
            Knob.x = 21;
            FiringHandle.transform.localPosition = Knob;
            print("Firing Handle is not engaged.");

        }
    }

    void CheckLockingLever()
    {
        Quaternion Hinge = LockingLever.transform.localRotation;
        if ((Input.GetKeyUp(KeyCode.Y)) && (Hinge.z < 0.0) && (Enable == true))
        {
            Hinge.z = 0.0f;
            LockingLever.transform.localRotation = Hinge;
            print("Locking Lever is closed.");
        }
        else if (Input.GetKeyUp(KeyCode.Y) && (Enable == true))
        {
            Hinge.z = -0.2f;
            LockingLever.transform.localRotation = Hinge;
            print("Locking Lever is open.");

        }
    }
}
