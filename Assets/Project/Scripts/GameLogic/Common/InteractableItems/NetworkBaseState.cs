﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CEMSIM
{
    namespace Network
    {
        /// <summary>
        /// This is an abstrack class that should be implemebted by classes that need to synchronize via network packet.
        /// </summary>
        public abstract class NetworkBaseState
        {
            /// <summary>
            /// Serialize the tool state to a byte array suitable for socket communication
            /// </summary>
            /// <returns></returns>
            public abstract byte[] ToPacketPayload();


            /// <summary>
            /// Digest the part of the packet that contains state update
            /// </summary>
            /// <param name="_remainderPacket"></param>
            /// <returns>Whether there are any state changes</returns>
            public abstract bool FromPacketPayload(Packet _remainderPacket);

        }
    }
}