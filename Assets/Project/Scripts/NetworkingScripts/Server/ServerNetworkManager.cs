using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.GameLogic;

namespace CEMSIM
{
    namespace Network
    {
        public class ServerNetworkManager : MonoBehaviour
        {
            public static ServerNetworkManager instance;

            [Header("Player Prefabs")]
            public GameObject playerDesktopPrefab;  // associate to a player
            public GameObject playerVRPrefab;       // associate to a player'

            [Header("Player Spawning")]
            public Vector3 spawnLocation;
            public Vector3 spawnRotation;

            [Header("References")]
            public GameObject playersContainer;


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
                Application.targetFrameRate = ServerGameConstants.TICKS_PER_SECOND;

                ServerInstance.Start(ServerNetworkConstants.CONCURRENT_CLIENTS, ServerNetworkConstants.TCP_PORT);
            }

            /// <summary>
            /// Propertly release network ports when quit
            /// </summary>
            private void OnApplicationQuit()
            {
                ServerInstance.Stop();
            }

            /// <summary>
            /// Call the instantiate function to create a player gameObject and return the reference.
            /// </summary>
            /// <returns></returns>
            public ServerPlayer InstantiatePlayerDesktop()
            {
                // initialize a player at the initial location and return the reference
                return Instantiate(playerDesktopPrefab, spawnLocation, Quaternion.Euler(spawnRotation), playersContainer.transform).GetComponent<ServerPlayer>();
            }

            /// <summary>
            /// Call the instantiate function to create a player gameObject and return the reference.
            /// </summary>
            /// <returns></returns>
            public ServerPlayer InstantiatePlayerVR()
            {
                // initialize a player at the initial location and return the reference
                return Instantiate(playerVRPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity, playersContainer.transform).GetComponent<ServerPlayer>();
            }
        }
    }
}