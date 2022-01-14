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
        public class VisorStateManager : ItemStateManager
        {
            public enum VisorStateList
            {
                defaultState = 0,
            }

            private VisorStateList state;
            public static event Action<int, VisorStateList, int> onVisorStateUpdateTrigger;

            public VisorStateManager()
            {
                toolCategory = ToolType.N95Mask;
                UpdateState(VisorStateList.defaultState); // 
                                                            //Debug.Log($"Initialize {toolCategory} - {state}");
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
                if (!Enum.IsDefined(typeof(VisorStateList), _specId))
                {
                    Debug.LogWarning($"{toolCategory} does't have state {_specId}. State ignored");
                    return;
                }

                UpdateState((VisorStateList)_specId);
            }

            /// <summary>
            /// Update state
            /// </summary>
            public void UpdateState(VisorStateList _newState)
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
            public static void ItemStateUpdateTrigger(int _itemId, VisorStateList _state, int _clientId)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onVisorStateUpdateTrigger != null)
                    onVisorStateUpdateTrigger(_itemId, _state, _clientId);
            }
            #endregion
        }
    }
}
