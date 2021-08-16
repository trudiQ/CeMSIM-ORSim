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
            public bool printNetworkTraffic = false;
            public CEMSIMWrapClient(CEMSIMCommsNetwork network) : base(network)
            {
                // do nothing, because CeMSIMWrapClient is purely a wrap up class.
                comm = network;
                printNetworkTraffic = network.printNetworkTraffic;
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
                ClientSend.SendVoiceChatData(_voiceChatData, false);
            }

            protected override void SendUnreliable(ArraySegment<byte> _voiceChatData)
            {
                if (printNetworkTraffic)
                    Debug.Log("[VoiceChat] Sending UDP packet");
                ClientSend.SendVoiceChatData(_voiceChatData, true);
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