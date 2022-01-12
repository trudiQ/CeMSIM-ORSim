using CEMSIM.Network;
using Dissonance.Networking;
using Dissonance.Networking.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace VoiceChat
    {
        public class CEMSIMWrapServer : BaseServer<CEMSIMWrapServer, CEMSIMWrapClient, int>
        {
            private CEMSIMCommsNetwork comm;
            public bool printNetworkTraffic;
            public CEMSIMWrapServer(CEMSIMCommsNetwork network)
            {
                comm = network;
                printNetworkTraffic = network.printNetworkTraffic;
                // do nothing because CeMSIMWrapServer is merely a wrap up class.
            }

            protected override void ReadMessages()
            {
                // We don't need this functionality
            }


            protected override void SendReliable(int _toClient, ArraySegment<byte> packet)
            {
                if(printNetworkTraffic)
                    Debug.Log($"[VoiceChat] Sending TCP packet to {_toClient} ");
                if (_toClient == 0)
                {
                    ServerNetworkManager.instance.dissonanceDummyClient.PacketDelivered(packet);
                }
                else
                    ServerSend.SendVoiceChatData(_toClient, packet, false);
            }

            protected override void SendUnreliable(int _toClient, ArraySegment<byte> packet)
            {
                if (printNetworkTraffic)
                    Debug.Log($"[VoiceChat] Sending UDP packet to {_toClient} ");
                if (_toClient == 0)
                {
                    ServerNetworkManager.instance.dissonanceDummyClient.PacketDelivered(packet);
                }
                else
                    ServerSend.SendVoiceChatData(_toClient, packet, true);
            }

            public override void Connect()
            {
                base.Connect();
            }

            public override void Disconnect()
            {
                base.Disconnect();
            }

            public void PacketDelivered(int _fromClient, ArraySegment<byte> packet)
            {
                //Skip events we don't care about
                if (printNetworkTraffic)
                    Debug.Log($"[VoiceChat] Receive packet from client {_fromClient}");
                NetworkReceivedPacket(_fromClient, packet);
            }

        }
    }
}