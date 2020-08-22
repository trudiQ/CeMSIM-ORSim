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
        [Header("Player Camera and Control Prefab")]
        public GameObject XRPlayerPrefab;
        public GameObject XRInteractionPrefab;

        public GameObject steamPlayerPrefab;
        public GameObject steamInteractionPrefab;

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
                        Instantiate(steamInteractionPrefab);
                        Instantiate(steamPlayerPrefab);
                        isPlayerControllerEnabled = true;
                        break;
                    }
                    else if (device.name == "Oculus Rift")
                    {
                        Debug.Log($"Headset Detected: {device.name}. Activating Oculus Character and Camera");
                        Instantiate(XRInteractionPrefab);
                        Instantiate(XRPlayerPrefab);
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
