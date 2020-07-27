using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScript.Steps;

public class LinearStaplerTool : Tool //inherits Tool class
{
    public GameObject FiringHandle;
    public GameObject LockingLever;

    void Update() //Checks status of knob, lever, and linear stapler in every frame
    {
        CheckFiringKnob();
        CheckLockingLever();
        Enabler();
    }

    void CheckFiringKnob()
    {
        //Knob translates to final position if T is pressed and linear stapler is enabled
        Vector3 Knob = FiringHandle.transform.localPosition;
        if ((Input.GetKeyUp(KeyCode.T)) && (Knob.x > 0) && (Enable == true))
        {
            Knob.x = -42;
            FiringHandle.transform.localPosition = Knob;
            print("Firing Handle is engaged.");
        }
        //Knob translates to initial position if T is pressed again and linear stapler is enabled
        else if (Input.GetKeyUp(KeyCode.T) && (Enable == true))
        {
            Knob.x = 21;
            FiringHandle.transform.localPosition = Knob;
            print("Firing Handle is not engaged.");

        }
    }

    void CheckLockingLever()
    {
        //Locking lever closes if Y is pressed and linear stapler is enabled
        Quaternion Hinge = LockingLever.transform.localRotation;
        if ((Input.GetKeyUp(KeyCode.Y)) && (Hinge.z < 0.0) && (Enable == true))
        {
            Hinge.z = 0.0f;
            LockingLever.transform.localRotation = Hinge;
            print("Locking Lever is closed.");
        }
        //Locking lever opens if Y is pressed again and linear stapler is enabled
        else if (Input.GetKeyUp(KeyCode.Y) && (Enable == true))
        {
            Hinge.z = -0.2f;
            LockingLever.transform.localRotation = Hinge;
            print("Locking Lever is open.");

        }
    }
}
