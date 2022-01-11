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
            public static event Action<int, VisorStateList> onVisorStateUpdateTrigger;

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
                ItemStateUpdateTrigger(itemId, state);

            }

            #region Event System
            public static void ItemStateUpdateTrigger(int _itemId, VisorStateList _state)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onVisorStateUpdateTrigger != null)
                    onVisorStateUpdateTrigger(_itemId, _state);
            }
            #endregion
        }
    }
}
