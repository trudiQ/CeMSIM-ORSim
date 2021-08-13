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
        public class CeMSIMCommsNetwork : BaseCommsNetwork<CeMSIMWrapServer, CeMSIMWrapClient, int, Unit, Unit>
        {
            #region Fields and Parameters
            public bool isClientSide;   // decide whether the current instance is going to act as a client or dedicated server.
            #endregion


            protected override CeMSIMWrapClient CreateClient([CanBeNull] Unit connectionParameters)
            {
                //throw new System.NotImplementedException();
                ClientInstance.instance.dissonanceClient = new CeMSIMWrapClient(this);
                return ClientInstance.instance.dissonanceClient;
            }

            protected override CeMSIMWrapServer CreateServer([CanBeNull] Unit connectionParameters)
            {
                //throw new System.NotImplementedException();
                ServerNetworkManager.instance.dissonanceServer = new CeMSIMWrapServer(this);
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
                            if(Mode != NetworkMode.Client)
                                RunAsClient(Unit.None);
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
                        if(Mode != NetworkMode.DedicatedServer)
                            RunAsDedicatedServer(Unit.None);
                    }
                }

                base.Update();
            }
        }
    }
}