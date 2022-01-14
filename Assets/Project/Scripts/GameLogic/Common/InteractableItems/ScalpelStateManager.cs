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
            public enum ScalpelStateList
            {
                defaultState = 0,
            }

            private ScalpelStateList state;
            public static event Action<int, ScalpelStateList, int> onScalpelStateUpdateTrigger;

            public ScalpelStateManager()
            {
                toolCategory = ToolType.scalpel;
                UpdateState(ScalpelStateList.defaultState); // 
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
                if (!Enum.IsDefined(typeof(ScalpelStateList), _specId))
                {
                    Debug.LogWarning($"{toolCategory} does't have state {_specId}. State ignored");
                    return;
                }

                UpdateState((ScalpelStateList)_specId);
            }

            /// <summary>
            /// Update state
            /// </summary>
            public void UpdateState(ScalpelStateList _newState)
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
            public static void ItemStateUpdateTrigger(int _itemId, ScalpelStateList _state, int _clientId)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onScalpelStateUpdateTrigger != null)
                    onScalpelStateUpdateTrigger(_itemId, _state, _clientId);
            }
            #endregion
        }
    }
}