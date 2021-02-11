using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace CEMSIM
{
    public class XRManager : MonoBehaviour
    {
        [Header("Player XR Rig")]
        public GameObject XRPlayerRig;

        [Header("XR Components")]
        public GameObject LeftHandController;
        public GameObject RightHandController;
        public GameObject LeftTeleportRay;
        public GameObject RightTeleportRay;

        [Header("Steam Components")]
        public GameObject VRObjects;
        public GameObject FallbackObjects;
        public GameObject HeadCollider;
        public GameObject Teleporting;

        [Header("Teleport Areas")]
        public GameObject XRTeleportationObject;
        public GameObject SteamTeleportationObject;

        //Private Variables
        private List<InputDevice> deviceList;
        private bool isPlayerControllerEnabled = false;

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(ActivatePlayerCamera());
        }
        
        IEnumerator ActivatePlayerCamera()
        {
            yield return new WaitForSeconds(1.0f);
            if(isHeadsetAvailable())
            {
                foreach (var device in deviceList)
                {
                    if (device.name == "OpenVR Headset(vive_cosmos)")
                    {
                        Debug.Log($"Headset Detected: {device.name}. Activating SteamVR Character and Camera");

                        VRObjects.SetActive(true);
                        Teleporting.SetActive(true);

                        SteamTeleportationObject.SetActive(true);
                        isPlayerControllerEnabled = true;
                        break;
                    }
                    else if (device.name == "OpenVR Headset(Vive MV)")
                    {
                        Debug.Log($"Headset Detected: {device.name}. Activating SteamVR Character and Camera");

                        VRObjects.SetActive(true);
                        Teleporting.SetActive(true);

                        SteamTeleportationObject.SetActive(true);
                        isPlayerControllerEnabled = true;
                        break;
                    }
                    else if (device.name == "Oculus Rift")
                    {
                        Debug.Log($"Headset Detected: {device.name}. Activating Oculus Character and Camera");

                        XRPlayerRig.GetComponent<Valve.VR.InteractionSystem.Player>().enabled = false;
                        LeftHandController.SetActive(true);
                        RightHandController.SetActive(true);
                        LeftTeleportRay.SetActive(true);
                        RightTeleportRay.SetActive(true);

                        XRTeleportationObject.SetActive(true);
                        isPlayerControllerEnabled = true;
                        break;
                    }
                    else if (device.name == "Oculus Rift S")
                    {
                        Debug.Log($"Headset Detected: {device.name}. Activating Oculus Character and Camera");

                        XRPlayerRig.GetComponent<Valve.VR.InteractionSystem.Player>().enabled = false;
                        LeftHandController.SetActive(true);
                        RightHandController.SetActive(true);
                        LeftTeleportRay.SetActive(true);
                        RightTeleportRay.SetActive(true);

                        XRTeleportationObject.SetActive(true);
                        isPlayerControllerEnabled = true;
                        break;
                    }

                }
                
                if(!isPlayerControllerEnabled)
                {
                    Debug.Log("Cannot Activate Character/Camera. Do not recognize any headsets");
                }
            }
        }

        bool isHeadsetAvailable()
        {
            deviceList = new List<InputDevice>();
            InputDevices.GetDevices(deviceList);

            if(deviceList.Capacity!=0)
            {
                return true;
            }
            else return false;
        }
    }
}
