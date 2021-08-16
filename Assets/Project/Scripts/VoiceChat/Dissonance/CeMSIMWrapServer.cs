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
        public class CeMSIMWrapServer : BaseServer<CeMSIMWrapServer, CeMSIMWrapClient, int>
        {
            private CeMSIMCommsNetwork comm;
            public CeMSIMWrapServer(CeMSIMCommsNetwork network)
            {
                comm = network;
                // do nothing because CeMSIMWrapServer is merely a wrap up class.
            }

            protected override void ReadMessages()
            {
                // We don't need this functionality
            }


            protected override void SendReliable(int connection, ArraySegment<byte> packet)
            {
                //throw new NotImplementedException();
                int _toClient = connection; // Not sure. Need double check
                Debug.Log($"[VoiceChat] Sending TCP packet to {_toClient} ");

                ServerSend.SendVoiceChatData(_toClient, packet, false);
            }

            protected override void SendUnreliable(int connection, ArraySegment<byte> packet)
            {
                //throw new NotImplementedException();
                int _toClient = connection; // Not sure. Placeholder
                Debug.Log($"[VoiceChat] Sending TCP packet to {_toClient} ");

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
                Debug.Log($"[VoiceChat] Receive packet from client {_fromClient}");
                NetworkReceivedPacket(_fromClient, packet);
            }

        }
    }
}