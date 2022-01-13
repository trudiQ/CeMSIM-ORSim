using CEMSIM.GameLogic;
using CEMSIM.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CEMSIM
{
    namespace GameLogic
    {
        public class GownStateManager : ItemStateManager
        {
            public enum GownStateList
            {
                defaultState = 0,
            }

            private GownStateList state;

            public static event Action<int, GownStateList, int> onGownStateUpdateTrigger;

            public GownStateManager()
            {
                toolCategory = ToolType.catheter;
                UpdateState(GownStateList.defaultState); // 

                Debug.Log($"Initialize {toolCategory} - {state}");

            }

            public override void initializeItem(int _id)
            {
                base.initializeItem(_id);
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
                if (!Enum.IsDefined(typeof(GownStateList), _specId))
                {
                    Debug.LogWarning($"{toolCategory} does't have state {_specId}. State ignored");
                    return;
                }

                UpdateState((GownStateList)_specId);
            }

            /// <summary>
            /// Update state
            /// </summary>
            public void UpdateState(GownStateList _newState)
            {
                state = _newState;
                if (ClientItemManager.instance != null)
                {
                    ClientItemManager.instance.GainOwnership(itemId);
                    ItemStateUpdateTrigger(itemId, state, ClientInstance.instance.myId);
                }
                else
                    ItemStateUpdateTrigger(itemId, state, GameConstants.SINGLE_PLAYER_CLIENTID);

            }

            #region Event System
            public static void ItemStateUpdateTrigger(int _itemId, GownStateList _state, int _clientId)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onGownStateUpdateTrigger != null)
                    onGownStateUpdateTrigger(_itemId, _state, _clientId);
            }
            #endregion
        }
    }
}