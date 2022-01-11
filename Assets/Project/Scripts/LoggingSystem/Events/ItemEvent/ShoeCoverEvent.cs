using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public class ShoeCoverEvent : ItemBaseEvent
        {
            ShoeCoverStateManager.ShoeCoverOnFootList showCoverState;

            public ShoeCoverEvent(int _itemId, ItemEventType _action, ShoeCoverStateManager.ShoeCoverOnFootList _showCoverState) : base(ToolType.shoeCover, _itemId, _action)
            {
                showCoverState = _showCoverState;
            }

            public override string ToString()
            {
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        return base.ToString();
                    case ItemEventType.StateUpdate:
                        return $"{eventTime}: {toolType}, {itemId}, {action} {showCoverState}";
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
                        msg += JsonAddElement("State", showCoverState.ToString());
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            public static void GenShoeCoverStateUpdate(int _itemId, ShoeCoverStateManager.ShoeCoverOnFootList _showCoverState)
            {
                using (ItemBaseEvent e = new ShoeCoverEvent(_itemId, ItemEventType.StateUpdate, _showCoverState))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }
        }
    }
}
