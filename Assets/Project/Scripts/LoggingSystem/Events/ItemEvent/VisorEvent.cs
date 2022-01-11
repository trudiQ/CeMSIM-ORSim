using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public class VisorEvent : ItemBaseEvent
        {
            VisorStateManager.VisorStateList visorState;

            public VisorEvent(int _itemId, ItemEventType _action, VisorStateManager.VisorStateList _visorState) : base(ToolType.visor, _itemId, _action)
            {
                visorState = _visorState;
            }

            public override string ToString()
            {
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        return base.ToString();
                    case ItemEventType.StateUpdate:
                        return $"{eventTime}: {toolType}, {itemId}, {action} {visorState}";
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
                        msg += JsonAddElement("State", visorState.ToString());
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            public static void GenVisorStateUpdate(int _itemId, VisorStateManager.VisorStateList _visorState)
            {
                using (ItemBaseEvent e = new VisorEvent(_itemId, ItemEventType.StateUpdate, _visorState))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }
        }
    }
}