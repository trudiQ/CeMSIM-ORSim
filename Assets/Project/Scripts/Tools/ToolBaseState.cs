using System.Collections;
using System.Collections.Generic;
using CEMSIM.Network;
using UnityEngine;

namespace CEMSIM {
    namespace Tools
    {
        public class ToolBaseState : NetworkStateInterface
        {
            public virtual bool FromPacketPayload(Packet _remainderPacket)
            {
                // do nothing
                return true;
            }

            public virtual byte[] ToPacketPayload()
            {
                return new byte[0];
            }
        }
    }
}
