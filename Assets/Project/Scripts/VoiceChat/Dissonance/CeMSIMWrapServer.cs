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
        public class CeMSIMWrapServer : BaseServer<CeMSIMWrapServer, CeMSIMWrapClient, int>
        {
            public CeMSIMWrapServer(CeMSIMCommsNetwork network)
            {
                // do nothing because CeMSIMWrapServer is merely a wrap up class.
            }

            protected override void ReadMessages()
            {
                // We don't need this functionality
            }


            protected override void SendReliable(int connection, ArraySegment<byte> packet)
            {
                //throw new NotImplementedException();
                int _fromClient = connection; // Not sure. Placeholder
                ServerSend.SendVoiceChatData(_fromClient, packet.Array, false);
            }

            protected override void SendUnreliable(int connection, ArraySegment<byte> packet)
            {
                //throw new NotImplementedException();
                int _fromClient = connection; // Not sure. Placeholder
                ServerSend.SendVoiceChatData(_fromClient, packet.Array, true);
            }

            public override void Connect()
            {
                base.Connect();
            }

            public override void Disconnect()
            {
                base.Disconnect();
            }

           
        }
    }
}