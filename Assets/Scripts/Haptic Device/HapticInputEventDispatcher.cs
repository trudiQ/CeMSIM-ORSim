using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//HapticGrabber.cs but simply abstracted to work like OnKeyDown, OnKeyHeld, OnKeyUp
public class HapticInputEventDispatcher : MonoBehaviour
{
    public int buttonID = 0;        //!< index of the button assigned to grabbing.  Defaults to the first button

    private GameObject hapticDevice = null;   //!< Reference to the GameObject representing the Haptic Device
    private bool buttonStatus = false;          //!< Is the button currently pressed?
    private GameObject touching = null;         //!< Reference to the object currently touched
    private GameObject grabbing = null;         //!< Reference to the object currently grabbed
    private FixedJoint joint = null;			//!< The Unity physics joint created between the stylus and the object being grabbed.

    public UnityEvent OnButtonPress = new UnityEvent();
    public UnityEvent OnButtonHeld = new UnityEvent();
    public UnityEvent OnButtonRelease = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        hapticDevice = GetComponentInChildren<HapticPlugin>().gameObject;
    }

    void FixedUpdate()
    {
        bool newButtonStatus = hapticDevice.GetComponent<HapticPlugin>().Buttons[buttonID] == 1;
        bool oldButtonStatus = buttonStatus;
        buttonStatus = newButtonStatus;


        if (oldButtonStatus == false && newButtonStatus == true)
        {
            OnButtonPress.Invoke();
        }
        else if (oldButtonStatus == true && newButtonStatus == false)
        {
            OnButtonRelease.Invoke();
        }
        else if(oldButtonStatus == true && newButtonStatus == true)
        {
            OnButtonHeld.Invoke();
        }
    }
}
