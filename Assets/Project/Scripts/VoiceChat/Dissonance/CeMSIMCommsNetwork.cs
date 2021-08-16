using Dissonance;
using Dissonance.Networking;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;


namespace CEMSIM
{
    namespace VoiceChat
    {
        public class CEMSIMCommsNetwork : BaseCommsNetwork<CEMSIMWrapServer, CEMSIMWrapClient, int, Unit, Unit>
        {
            #region Fields and Parameters
            public bool isClientSide;   // decide whether the current instance is going to act as a client or dedicated server.
            public bool printNetworkTraffic; // 
            #endregion


            protected override CEMSIMWrapClient CreateClient([CanBeNull] Unit connectionParameters)
            {
                //throw new System.NotImplementedException();
                ClientInstance.instance.dissonanceClient = new CEMSIMWrapClient(this);
                return ClientInstance.instance.dissonanceClient;
            }

            protected override CEMSIMWrapServer CreateServer([CanBeNull] Unit connectionParameters)
            {
                //throw new System.NotImplementedException();
                ServerNetworkManager.instance.dissonanceServer = new CEMSIMWrapServer(this);
                return ServerNetworkManager.instance.dissonanceServer;
            }

            protected override void Initialize()
            {
                base.Initialize();
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
                            if (Mode != NetworkMode.DedicatedServer)
                            {
                                if (printNetworkTraffic)
                                    Debug.Log("[VoiceChat] Running as a Server");
                                RunAsDedicatedServer(Unit.None);
                            }
                    }
                }

                base.Update();
            }
        }
    }
}