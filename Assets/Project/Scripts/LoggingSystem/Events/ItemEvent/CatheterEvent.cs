using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public class CatheterEvent : ItemBaseEvent
        {
            private CatheterStateManager.CatheterStateList catheterState;

            public CatheterEvent(int _itemId, ItemEventType _action, CatheterStateManager.CatheterStateList _catheterState) : base(ToolType.catheter, _itemId, _action)
            {
                catheterState = _catheterState;
            }

            public override string ToString()
            {
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        return base.ToString();
                    case ItemEventType.StateUpdate:
                        return $"{eventTime}: {toolType}, {itemId}, {action} {catheterState}";
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
                        msg += JsonAddElement("State", catheterState.ToString());
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            public static void GenCatheterStateUpdate(int _itemId, CatheterStateManager.CatheterStateList _catheterState)
            {
                using (ItemBaseEvent e = new CatheterEvent(_itemId, ItemEventType.StateUpdate, _catheterState))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }
        }
    }
}