using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        
        public class BouffantEvent : ItemBaseEvent
        {
            // state related variables
            private BoufantStateManager.BoufantStateList boufantState;

            public BouffantEvent(int _itemId, ItemEventType _action, BoufantStateManager.BoufantStateList _boufantState, int _clientId) : base(ToolType.boufant, _itemId, _action, _clientId)
            {
                boufantState = _boufantState;
            }

            public override string ToString()
            {
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        return base.ToString();
                    case ItemEventType.StateUpdate:
                        return $"{eventTime}: {toolType}, {itemId}, {action} {boufantState}";
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
                        msg += JsonAddElement("State", boufantState.ToString());
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            public static void GenBoufantStateUpdate(int _itemId, BoufantStateManager.BoufantStateList _boufantState, int _clientId)
            {
                using (ItemBaseEvent e = new BouffantEvent(_itemId, ItemEventType.StateUpdate, _boufantState, _clientId))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

        }
    }
}