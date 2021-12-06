using CEMSIM.Network;
using Dissonance.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace VoiceChat
    {
        public class CEMSIMWrapClient : BaseClient<CEMSIMWrapServer, CEMSIMWrapClient, int>
        {
            private CEMSIMCommsNetwork comm;

            private int counter = 0; // debug use only

            // We allow the server to fake a client for monitoring. so we need to distinguish whether it's a server side fake client or a real client.
            public bool printNetworkTraffic = false;
            public CEMSIMWrapClient(CEMSIMCommsNetwork _network) : base(_network)
            {
                // do nothing, because CeMSIMWrapClient is purely a wrap up class.
                comm = _network;
                printNetworkTraffic = _network.printNetworkTraffic;
            }

            public override void Connect()
            {
                Connected();
                Debug.Log("dissonance client connected");
            }

            public override void Disconnect()
            {
                base.Disconnect();
            }

            protected override void ReadMessages()
            {
                //We don't need this

                //throw new NotImplementedException();
            }

            protected override void SendReliable(ArraySegment<byte> _voiceChatData)
            {
                if (printNetworkTraffic)
                    Debug.Log("[VoiceChat] Sending TCP packet");
                if (comm.isClientSide)
                    ClientSend.SendVoiceChatData(_voiceChatData, false); // real client
                else
                {
                    // for the fake client running at the server side, we directly pass the data to the packet handling function.
                    ServerNetworkManager.instance.dissonanceServer.PacketDelivered(0, _voiceChatData);
                }
            }

            protected override void SendUnreliable(ArraySegment<byte> _voiceChatData)
            {
                if (printNetworkTraffic)
                    Debug.Log("[VoiceChat] Sending UDP packet");
                if (comm.isClientSide)
                    ClientSend.SendVoiceChatData(_voiceChatData, true);
                else
                {
                    // for the fake client running at the server side, we directly pass the data to the packet handling function.
                    ServerNetworkManager.instance.dissonanceServer.PacketDelivered(0, _voiceChatData);
                }

            }

            public ushort? PacketDelivered(ArraySegment<byte> packet)
            {
                //Skip events we don't care about
                if (printNetworkTraffic)
                    Debug.Log($"[VoiceChat] Receive packet from the server");
                return NetworkReceivedPacket(packet);
            }
        }
    }
}