﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        namespace Server
        {
            public class NetworkManager : MonoBehaviour
            {
                public static NetworkManager instance;
                public GameObject playerContainer;
                public GameObject playerVRPrefab; // associate to a player
                public GameObject playerDesktopPrefab; // associate to a player


                private void Awake()
                {
                    if (instance == null)
                    {
                        instance = this;
                    }
                    else if (instance != this)
                    {
                        // We only allow one instance of this class to exist.
                        // Destroy current instance if different from the current one.
                        Debug.Log("Another instance already exists. Destroy this one.");
                        Destroy(this);
                    }
                }

                /// <summary>
                /// Start the server from here.
                /// </summary>
                private void Start()
                {
                    QualitySettings.vSyncCount = 0; // disable vSync to limit frame rate.
                    Application.targetFrameRate = Constants.TICKS_PER_SECOND;

                    Server.Start(Constants.CONCURRENT_CLIENTS, Constants.TCP_PORT);
                }

                /// <summary>
                /// Propertly release network ports when quit
                /// </summary>
                private void OnApplicationQuit()
                {
                    Server.Stop();
                }

                /// <summary>
                /// Call the instantiate function to create a player gameObject and return the reference.
                /// </summary>
                /// <returns></returns>
                public PlayerDesktop InstantiatePlayerDesktop()
                {
                    // Initialize a player at the initial location, as a child of the Player container and return the reference
                    return Instantiate(playerDesktopPrefab, new Vector3(3f, 0f, 0f), Quaternion.identity,playerContainer.transform).GetComponent<PlayerDesktop>();
                }

                public PlayerVR InstantiatePlayerVR()
                {
                    // Initialize a player at the initial location, as a child of the Player container and return the reference
                    return Instantiate(playerVRPrefab, new Vector3(3f, 0f, 0f), Quaternion.identity, playerContainer.transform).GetComponent<PlayerVR>();
                }
            }
        }
    }
}