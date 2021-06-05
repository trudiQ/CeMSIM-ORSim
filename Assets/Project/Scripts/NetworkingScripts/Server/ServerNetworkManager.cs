using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.GameLogic;
using System;

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

            [Header("Traffic Visualization")]
            public bool printNetworkTraffic=false;        // True: print out the inbound and outbound traffic in console.


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

                // start the HeartBeatDetection mechanism
                Debug.Log("Start HeartBeat Detection");
                StartCoroutine(HeartBeatDetection());
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


            /// <summary>
            /// Periodically sending HeartBeatDetection packets to
            /// 1. measure the round-trip-time for tcp and udp link
            /// 2. check whether a client is alive
            /// </summary>
            /// <returns></returns>
            IEnumerator HeartBeatDetection()
            {
                while (true)
                {
                    // prepare for the next detection after HEARTBEAT_DETECTION_PERIOD seconds
                    yield return new WaitForSeconds(ServerNetworkConstants.HEARTBEAT_DETECTION_PERIOD);

                    // check whether a client is offline
                    long utcNow = DateTime.UtcNow.Ticks;
                    //for (int clientId = 0; clientId < ServerNetworkConstants.CONCURRENT_CLIENTS; clientId++)
                    //{
                    //    if (ServerInstance.clients[clientId].udp.lastHeartBeat == 0 && ServerInstance.clients[clientId].tcp.lastHeartBeat == 0)
                    //    {
                    //        // lastHeartBeat = 0 => this client has not been utilized yet
                    //        continue;
                    //    }
                    //    // check UDP instance
                    //    if (utcNow - ServerInstance.clients[clientId].udp.lastHeartBeat > ServerNetworkConstants.HEARTBEAT_TIMEOUT)
                    //    {
                    //        // the udp instance of the client is offline

                    //    }

                    //    if (utcNow - ServerInstance.clients[clientId].tcp.lastHeartBeat > ServerNetworkConstants.HEARTBEAT_TIMEOUT)
                    //    {
                    //        // the tcp instance of the client is offline

                    //    }

                    //}

                    // send UDP and TCP Heart Beat Detection packets
                    ServerSend.HeartBeatDetection();
                    if (ServerNetworkManager.instance.printNetworkTraffic)
                    {
                        Debug.Log("Send HeartBeat Detection packets.");
                    }



                }
            }

        }
    }
}