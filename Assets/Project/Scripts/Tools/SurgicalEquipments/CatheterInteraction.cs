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
        public class CatheterState : ToolBaseState
        {
            // State of scalpel, e.g. no blood
            public enum CatheterStateList
            {
                defaultState = 0,
            }

            private CatheterStateList state;

            public CatheterState(CatheterStateList _initState = CatheterStateList.defaultState)
            {
                state = _initState;
            }

            public override bool FromPacketPayload(Packet _remainderPacket)
            {
                bool isChange;
                CatheterStateList newState = (CatheterStateList)_remainderPacket.ReadInt32();
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
                msg += BaseEvent.JsonAddElement("CatheterState", state.ToString());
                return msg;
;            }
        }


        public class CatheterInteraction : ToolBaseInteraction
        {
            public CatheterInteraction() : base(ToolType.catheter)
            {
                base.toolState = new CatheterState(CatheterState.CatheterStateList.defaultState);
            }

            public override void UpdateState()
            {
                // since scalpel has no animation, no update
            }
        }
    }
}