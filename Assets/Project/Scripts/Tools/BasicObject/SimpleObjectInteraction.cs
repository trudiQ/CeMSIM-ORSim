using System;
using System.Collections;
using System.Collections.Generic;
using CEMSIM.Network;
using CEMSIM.GameLogic;
using UnityEngine;


namespace CEMSIM
{
    namespace Tools
    {
        public class SimpleObjectState : ToolBaseState
        {
            // State of scalpel, e.g. no blood
            public enum SimpleObjectStateList
            {
                defaultState = 0,
            }

            private SimpleObjectStateList state;

            public SimpleObjectState(SimpleObjectStateList _initState = SimpleObjectStateList.defaultState)
            {
                state = _initState;
            }

            public override bool FromPacketPayload(Packet _remainderPacket)
            {
                bool isChange;
                SimpleObjectStateList newState = (SimpleObjectStateList)_remainderPacket.ReadInt32();
                isChange = state != newState;

                state = newState;
                return isChange;
            }

            public override byte[] ToPacketPayload()
            {
                List<byte> message = new List<byte>();
                message.AddRange(BitConverter.GetBytes((int)state));

                return message.ToArray();
            }
        }


        public class SimpleObjectInteraction : ToolBaseInteraction
        {
            public SimpleObjectInteraction() : base(ToolType.simpleTool)
            {
                base.toolState = new SimpleObjectState(SimpleObjectState.SimpleObjectStateList.defaultState);
            }

            public override void UpdateState()
            {
                // since scalpel has no animation, no update
            }
        }
    }
}
