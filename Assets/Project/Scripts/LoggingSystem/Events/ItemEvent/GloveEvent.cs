using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {

        public class GloveEvent : ItemBaseEvent
        {
            // state
            private GloveStateManager.GloveOnHandList hand;          // left or right hand
            private GloveStateManager.GloveWearStateList wearState;
            private int quantity;               // number of layers of gloves

            public GloveEvent(int _itemId, ItemEventType _action, GloveStateManager.GloveOnHandList _hand, GloveStateManager.GloveWearStateList _gloveWearState, int _quantity, int _clientId) : base(ToolType.glove, _itemId, _action, _clientId)
            {
                hand = _hand;
                wearState = _gloveWearState;
                quantity = _quantity;
            }

            public override string ToString()
            {
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        return base.ToString();
                    case ItemEventType.StateUpdate:
                        return $"{eventTime}: {toolType}, {itemId}, {action} {hand} {wearState} {quantity}-layers";
                }
                return "";
            }

            public override string ToJson()
            {
                string msg = JsonPrefix();
                msg += JsonAddElement("ToolType", toolType.ToString());
                msg += JsonAddElement("ItemId", itemId);
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        msg += JsonAddElement("Action", action.ToString());
                        break;
                    case ItemEventType.StateUpdate:
                        msg += JsonAddSubElement("State",
                            JsonAddElement("TargetHand", hand.ToString()) + JsonAddElement("WearState", wearState.ToString()) + JsonAddElement("Layers", quantity)
                            );
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            public static void GenGloveWearStateUpdate(int _itemId, GloveStateManager.GloveOnHandList _hand, GloveStateManager.GloveWearStateList _gloveWearState, int _quantity, int _clientId)
            {
                using (ItemBaseEvent e = new GloveEvent(_itemId, ItemEventType.StateUpdate, _hand, _gloveWearState, _quantity, _clientId))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

        }
    }
}