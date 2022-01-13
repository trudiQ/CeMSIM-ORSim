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
        public class GloveStateManager : ItemStateManager
        {
            // State of the catheter, e.g. empty
            public enum GloveOnHandList
            {
                nonDetermined = 0,
                left,
                right,
            }

            public enum GloveWearStateList
            {
                nonDetermined = 0,
                overcuff,
            }

            private GloveOnHandList hand;
            private GloveWearStateList wearState;
            private int quantity;

            public static event Action<int, GloveOnHandList, GloveWearStateList, int, int> onGloveWearStateTrigger;

            public GloveStateManager()
            {
                toolCategory = ToolType.catheter;
                UpdateState(GloveOnHandList.nonDetermined, GloveWearStateList.nonDetermined, 0); // 

                Debug.Log($"Initialize {toolCategory} - {hand} - {wearState}");

            }

            public override void initializeItem(int _id)
            {
                base.initializeItem(_id);
            }

            public override byte[] GetItemState()
            {
                List<byte> message = new List<byte>();
                message.AddRange(BitConverter.GetBytes((int)hand));
                message.AddRange(BitConverter.GetBytes((int)wearState));
                message.AddRange(BitConverter.GetBytes(quantity));

                return message.ToArray();
            }

            public override void DigestStateMessage(Packet _remainderPacket)
            {
                int _hand = _remainderPacket.ReadInt32();
                int _wearState = _remainderPacket.ReadInt32();
                int _quantity = _remainderPacket.ReadInt32();
                if (!Enum.IsDefined(typeof(GloveOnHandList), _hand))
                {
                    Debug.LogWarning($"{toolCategory} does't have state {_hand}. State ignored");
                    return;
                }
                if (!Enum.IsDefined(typeof(GloveWearStateList), _wearState))
                {
                    Debug.LogWarning($"{toolCategory} does't have state {_wearState}. State ignored");
                    return;
                }

                UpdateState((GloveOnHandList)_hand, (GloveWearStateList)_wearState, _quantity);
            }

            /// <summary>
            /// Update state
            /// </summary>
            public void UpdateState(GloveOnHandList _hand, GloveWearStateList _wearState, int _quantity)
            {
                hand = _hand;
                wearState = _wearState;
                quantity = _quantity;

                if (ClientItemManager.instance != null)
                {
                    ClientItemManager.instance.GainOwnership(itemId);
                    GloveWearStateTrigger(itemId, hand, wearState, quantity, ClientInstance.instance.myId);
                }
                else
                    GloveWearStateTrigger(itemId, hand, wearState, quantity, GameConstants.SINGLE_PLAYER_CLIENTID);

            }

            #region Event System

            public static void GloveWearStateTrigger(int _itemId, GloveOnHandList _hand, GloveWearStateList _wearState, int _quantity, int _clientId)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onGloveWearStateTrigger != null)
                    onGloveWearStateTrigger(_itemId, _hand, _wearState, _quantity, _clientId);
            }
            #endregion
        }
    }
}