using System;
using System.Collections;
using System.Collections.Generic;
using CEMSIM.Network;
using CEMSIM.GameLogic;
using UnityEngine;
using CEMSIM.Logger;

namespace CEMSIM
{
    namespace Tools
    {
        public class ScalpelState : ToolBaseState
        {
            // State of scalpel, e.g. no blood
            public enum ScalpelStateList
            {
                defaultState = 0,
            }

            private ScalpelStateList state;

            public ScalpelState(ScalpelStateList _initState = ScalpelStateList.defaultState)
            {
                state = _initState;
            }

            public override bool FromPacketPayload(Packet _remainderPacket)
            {
                bool isChange;
                ScalpelStateList newState = (ScalpelStateList)_remainderPacket.ReadInt32();
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

            public override string ToJson()
            {
                string msg = "";
                msg += BaseEvent.JsonAddElement("ScalpelState", state.ToString());
                return msg;
            }
        }


        public class ScalpelInteraction : ToolBaseInteraction
        {
            public ScalpelInteraction(): base(ToolType.scalpel)
            {
                base.toolState = new ScalpelState(ScalpelState.ScalpelStateList.defaultState);
            }

            public override void UpdateState()
            {
                // since scalpel has no animation, no update
            }
        }
    }
}