using Dissonance;
using Dissonance.Networking;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;
using System;

namespace CEMSIM
{
    namespace VoiceChat
    {
        public class CEMSIMCommsNetwork : BaseCommsNetwork<CEMSIMWrapServer, CEMSIMWrapClient, int, Unit, Unit>
        {
            #region Fields and Parameters
            [Tooltip("check if this module is at the client side")]
            public bool isClientSide;   // decide whether the current instance is going to act as a client or dedicated server.
            public bool printNetworkTraffic; //
            public CEMSIMVoicePlayer player; // pointing to a dummy player running at the server side
            #endregion


            protected override CEMSIMWrapClient CreateClient([CanBeNull] Unit connectionParameters)
            {
                //throw new System.NotImplementedException();
                if (isClientSide) // a pure dissonance client.
                {
                    ClientInstance.instance.dissonanceClient = new CEMSIMWrapClient(this);
                    return ClientInstance.instance.dissonanceClient;
                }
                else
                { // a dummy client running at the server side.
                    Debug.Log("Creating a client at the server");
                    ServerNetworkManager.instance.dissonanceDummyClient = new CEMSIMWrapClient(this);
                    return ServerNetworkManager.instance.dissonanceDummyClient;
                }
            }

            protected override CEMSIMWrapServer CreateServer([CanBeNull] Unit connectionParameters)
            {
                //throw new System.NotImplementedException();
                ServerNetworkManager.instance.dissonanceServer = new CEMSIMWrapServer(this);
                return ServerNetworkManager.instance.dissonanceServer;
            }

            public static explicit operator CEMSIMCommsNetwork(DissonanceComms v)
            {
                throw new NotImplementedException();
            }

            protected override void Initialize()
            {
                base.Initialize();
                player = GetComponent<CEMSIMVoicePlayer>();
                if(player == null)
                {
                    Debug.LogWarning("Could not load the dummy dissonance player at the server side.");
                }
                else
                {
                    player.initialization(true, true);
                    Debug.Log("[VoiceChat] loading server side dummy player.");
                }
            }

            protected override void Update()
            {
                // Check if Dissonance is ready
                if (IsInitialized)
                {
                    if (isClientSide)
                    {
                        // This instance is running at the client side.
                        if (ClientInstance.instance.isConnected)
                        {
                            // Running as a dissonance client
                            if (Mode != NetworkMode.Client)
                            {
                                if (printNetworkTraffic)
                                    Debug.Log("[VoiceChat] Running as a client");
                                RunAsClient(Unit.None);

                            }
                        }
                        else
                        {
                            Stop();
                        }
                    }
                    else
                    {
                        // This instance is running at the server side.
                        // Running as a dedicated server
                        /*
                        if (Mode != NetworkMode.DedicatedServer)
                        {
                            if (printNetworkTraffic)
                                Debug.Log("[VoiceChat] Running as a DedicatedServer");
                            RunAsDedicatedServer(Unit.None);
                        }
                        //*/

                        //*
                        if (Mode != NetworkMode.Host)
                        {
                            if (printNetworkTraffic)
                                Debug.Log("[VoiceChat] Running as a Server (Host)");
                            RunAsHost(Unit.None, Unit.None);
                        }
                        //*/
                    }
                }

                base.Update();
            }
        }
    }
}