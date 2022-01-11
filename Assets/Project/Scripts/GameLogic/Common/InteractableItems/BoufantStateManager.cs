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
        public class BoufantStateManager : ItemStateManager
        {
            public enum BoufantStateList
            {
                defaultState = 0,
            }

            private BoufantStateList state;
            public static event Action<int, BoufantStateList> onN95MaskStateUpdateTrigger;

            public BoufantStateManager()
            {
                toolCategory = ToolType.N95Mask;
                UpdateState(BoufantStateList.defaultState); // 
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
                if (!Enum.IsDefined(typeof(BoufantStateList), _specId))
                {
                    Debug.LogWarning($"{toolCategory} does't have state {_specId}. State ignored");
                    return;
                }

                UpdateState((BoufantStateList)_specId);
            }

            /// <summary>
            /// Update state
            /// </summary>
            public void UpdateState(BoufantStateList _newState)
            {
                state = _newState;
                ItemStateUpdateTrigger(itemId, state);

            }

            #region Event System
            public static void ItemStateUpdateTrigger(int _itemId, BoufantStateList _state)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onN95MaskStateUpdateTrigger != null)
                    onN95MaskStateUpdateTrigger(_itemId, _state);
            }
            #endregion
        }
    }
}