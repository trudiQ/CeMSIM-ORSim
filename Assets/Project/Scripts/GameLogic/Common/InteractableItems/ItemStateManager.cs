using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;

namespace CEMSIM
{
    namespace GameLogic
    {
        /// <summary>
        /// This class is the base class that manage the state of each interactable items.
        ///
        /// Will be deprecated and replaced by ItemBaseStateManager
        /// </summary>
        public class ItemStateManager
        {

            public enum StateList
            {
                defaultState=0,   // item has no other working state
            }

            protected ToolType toolCategory;
            protected int itemId;
            private StateList state;

            public static event Action<int, StateList, int> onItemStateUpdateTrigger;



            public ItemStateManager()
            {
                toolCategory = ToolType.simpleTool;
                UpdateState(StateList.defaultState);
            }

            public virtual void initializeItem(int _id)
            {
                itemId = _id;
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

                UpdateState((StateList)_specId);
            }

            /// <summary>
            /// Update state
            /// </summary>
            public void UpdateState(StateList _newState)
            {
                state = _newState;
                if (ClientItemManager.instance != null)
                {
                    ItemStateUpdateTrigger(itemId, state, ClientInstance.instance.myId);
                }
                else
                    ItemStateUpdateTrigger(itemId, state, GameConstants.SINGLE_PLAYER_CLIENTID);
            }

            #region Event System
            public static void ItemStateUpdateTrigger(int _itemId, StateList _state, int _clientId)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onItemStateUpdateTrigger != null)
                    onItemStateUpdateTrigger(_itemId, _state, _clientId);
            }
            #endregion

        }
    }
}