using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class ManualXRControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<InputDevice> deviceList = new List<InputDevice>();
        InputDevices.GetDevices(deviceList);

        Debug.Log(deviceList);
        Debug.Log(deviceList.Capacity);

        if (isHardwarePresent())
        {
            Debug.Log("XR Hardware Present. Attempting XR initialization ...");
            StartCoroutine(StartXR());
        }
        else
        {
            Debug.Log("No XR Hardware Present. Continuining in desktop visualization.");
        }
    }

    public IEnumerator StartXR()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }

    public static bool isHardwarePresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                return true;
            }
        }
        return false;
    }

}

