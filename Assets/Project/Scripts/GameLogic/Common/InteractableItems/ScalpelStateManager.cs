using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;
using System;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class ScalpelStateManager : ItemStateManager
        {
            // State of scalpel, e.g. no blood
            private enum StateList
            {
                defaultState = 0,
            }

            private StateList state;

            public ScalpelStateManager(int _id)
            {
                id = _id;


                state = StateList.defaultState;
                toolCategory = ToolType.scalpel;

                Debug.Log($"Initialize {toolCategory} - {state}");
            }


            public override byte[] GetItemState()
            {
                List<byte> message = new List<byte>();
                message.AddRange(BitConverter.GetBytes((int)state));

                return message.ToArray();
            }

            public override void DigestStateMessage(Packet _remainderPacket)
            {
                int _specId = _remainderPacket.ReadInt32();
                if (!Enum.IsDefined(typeof(StateList), _specId))
                {
                    Debug.LogWarning($"{toolCategory} does't have state {_specId}. State ignored");
                    return;
                }

                state = (StateList)_specId;
            }
        }
    }
}