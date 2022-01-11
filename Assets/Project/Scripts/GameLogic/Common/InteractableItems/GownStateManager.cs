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

            public static event Action<int, GownStateList> onGownStateUpdateTrigger;

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
                ItemStateUpdateTrigger(itemId, state);

            }

            #region Event System
            public static void ItemStateUpdateTrigger(int _itemId, GownStateList _state)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onGownStateUpdateTrigger != null)
                    onGownStateUpdateTrigger(_itemId, _state);
            }
            #endregion
        }
    }
}