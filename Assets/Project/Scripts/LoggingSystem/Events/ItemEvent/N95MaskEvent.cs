using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {

        public class N95MaskEvent : ItemBaseEvent
        {
            N95MaskStateManager.N95MaskStateList n95MaskState;

            public N95MaskEvent(int _itemId, ItemEventType _action, N95MaskStateManager.N95MaskStateList _n95MaskState, int _clientId) : base(ToolType.N95Mask, _itemId, _action, _clientId)
            {
                n95MaskState = _n95MaskState;
            }

            public override string ToString()
            {
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        return base.ToString();
                    case ItemEventType.StateUpdate:
                        return $"{eventTime}: {toolType}, {itemId}, {action} {n95MaskState}";
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
                        msg += JsonAddElement("State", n95MaskState.ToString());
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            public static void GenN95MaskStateUpdate(int _itemId, N95MaskStateManager.N95MaskStateList _n95MaskState, int _clientId)
            {
                using (ItemBaseEvent e = new N95MaskEvent(_itemId, ItemEventType.StateUpdate, _n95MaskState, _clientId))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }
        }
    }
}