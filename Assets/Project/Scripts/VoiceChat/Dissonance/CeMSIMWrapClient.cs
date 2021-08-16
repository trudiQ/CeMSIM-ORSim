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
        public class CeMSIMWrapClient : BaseClient<CeMSIMWrapServer, CeMSIMWrapClient, int>
        {
            private CeMSIMCommsNetwork comm;

            public CeMSIMWrapClient(CeMSIMCommsNetwork network) : base(network)
            {
                // do nothing, because CeMSIMWrapClient is purely a wrap up class.
                comm = network;
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
                ClientSend.SendVoiceChatData(_voiceChatData, false);
            }

            protected override void SendUnreliable(ArraySegment<byte> _voiceChatData)
            {
                ClientSend.SendVoiceChatData(_voiceChatData, true);
            }
        }
    }
}