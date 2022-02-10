using System.Collections;
using System.Collections.Generic;
using CEMSIM.Network;
using UnityEngine;
using CEMSIM.Logger;

namespace CEMSIM {
    namespace Tools
    {
        public abstract class ToolBaseState : NetworkStateInterface, LoggingEventInterface
        {
            public virtual bool FromPacketPayload(Packet _remainderPacket)
            {
                // do nothing
                return true;
            }

            public virtual string ToCSV()
            {
                return "";
            }

            public abstract string ToJson(); // this implementation is required

            

            public virtual byte[] ToPacketPayload()
            {
                return new byte[0];
            }
        }
    }
}
