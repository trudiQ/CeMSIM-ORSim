using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {

        public class ScalpelEvent : ItemBaseEvent
        {
            ScalpelStateManager.ScalpelStateList scalpelState;

            // Currently, the scalpel has no state, so this is a dummy event
            public ScalpelEvent(int _itemId, ItemEventType _action, ScalpelStateManager.ScalpelStateList _scalpelState, int _clientId) : base(ToolType.scalpel, _itemId, _action, _clientId)
            {
                scalpelState = _scalpelState;
            }

            public override string ToString()
            {
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        return base.ToString();
                    case ItemEventType.StateUpdate:
                        return $"{eventTime}: {toolType}, {itemId}, {action} {scalpelState}";
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
                        msg += JsonAddElement("State", scalpelState.ToString());
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            public static void GenScalpelStateUpdate(int _itemId, ScalpelStateManager.ScalpelStateList _scalpelState, int _clientId)
            {
                using (ItemBaseEvent e = new ScalpelEvent(_itemId, ItemEventType.StateUpdate, _scalpelState, _clientId))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

        }
    }
}