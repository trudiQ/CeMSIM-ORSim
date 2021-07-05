using CEMSIM.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace GameLogic
    {
        /// <summary>
        /// This class is the base class that manage the state of each interactable items.
        /// </summary>
        public class ItemStateManager
        {

            private enum StateList
            {
                defaultState=0,   // item has no other working state
            }

            protected ToolType toolCategory;
            protected int id;
            private StateList state;

            public ItemStateManager()
            {
                state = StateList.defaultState;
                toolCategory = ToolType.simpleTool;
            }

            public virtual void initializeItem(int _id)
            {
                id = _id;
            }


            /// <summary>
            /// Return a byte array that abstract the current state of the attached item
            /// </summary>
            /// <returns></returns>
            public virtual byte[] GetItemState()
            {
                List<byte> message = new List<byte>();
                message.AddRange(BitConverter.GetBytes((int)state));

                return message.ToArray();
            }

            /// <summary>
            /// Digest the payload of the _packet to extract additional information of the attached item
            /// </summary>
            /// <param name="_packet"></param>
            public virtual void DigestStateMessage(Packet _remainderPacket)
            {
                int _specId = _remainderPacket.ReadInt32();
                if(!Enum.IsDefined(typeof(StateList), _specId))
                {
                    Debug.LogWarning($"{toolCategory} does't have state {_specId}. State ignored");
                    return;
                }

                state = (StateList)_specId;
            }

        }
    }
}