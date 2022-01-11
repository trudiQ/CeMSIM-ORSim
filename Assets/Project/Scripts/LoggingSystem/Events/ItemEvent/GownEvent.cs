using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {

        public class GownEvent : ItemBaseEvent
        {
            GownStateManager.GownStateList gownState;

            public GownEvent(int _itemId, ItemEventType _action, GownStateManager.GownStateList _gownState) : base(ToolType.gown, _itemId, _action)
            {
                gownState = _gownState;
            }

            public override string ToString()
            {
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        return base.ToString();
                    case ItemEventType.StateUpdate:
                        return $"{eventTime}: {toolType}, {itemId}, {action} {gownState}";
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
                        msg += JsonAddElement("State", gownState.ToString());
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            public static void GenGownStateUpdate(int _itemId, GownStateManager.GownStateList _gownState)
            {
                using (ItemBaseEvent e = new GownEvent(_itemId, ItemEventType.StateUpdate, _gownState))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }
        }
    }
}