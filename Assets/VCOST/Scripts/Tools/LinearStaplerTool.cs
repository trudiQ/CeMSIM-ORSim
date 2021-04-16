using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearStaplerTool : Tool //inherits Tool class
{
    public GameObject FiringHandle;
    public Transform firingHandleStartPosition;
    public Transform firingHandleEndPosition;
    public bool handlePushed;
    public GameObject LockingLever;
    public bool leverLocked;

    void Update() //Checks status of knob, lever, and linear stapler in every frame
    {
        CheckFiringKnob();
        CheckLockingLever();
        Enabler();
    }

    void CheckFiringKnob()
    {
        if (Enable && Input.GetKeyUp(KeyCode.T))
        {
            // Handle translates to final position if T is pressed and linear stapler is enabled
            if (!handlePushed)
            {
                FiringHandle.transform.localPosition = firingHandleEndPosition.localPosition;
                print("Firing Handle is engaged.");
            }
            // Handle translates to initial position if T is pressed again and linear stapler is enabled
            else
            {
                FiringHandle.transform.localPosition = firingHandleStartPosition.localPosition;
                print("Firing Handle is not engaged.");
            }

            handlePushed = !handlePushed;
        }
    }

    void CheckLockingLever()
    {
        if (Enable && Input.GetKeyUp(KeyCode.Y))
        {
            //Locking lever closes if Y is pressed and linear stapler is enabled
            if (!leverLocked)
            {
                LockingLever.transform.localEulerAngles = Vector3.zero;
                print("Locking Lever is closed.");
            }
            //Locking lever opens if Y is pressed again and linear stapler is enabled
            else
            {
                LockingLever.transform.localEulerAngles = new Vector3(0, 15, 0);
                print("Locking Lever is open.");
            }

            leverLocked = !leverLocked;
        }
    }
}
