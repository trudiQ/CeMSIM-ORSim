using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public enum ItemEventType
        {
            Pickup = 1,      // item pickup
            Dropdown,        // item dropdown
            StateUpdate,     // item state update
        }

        /// <summary>
        /// This is the base clase for each item event, which is the same as the ItemController.
        /// </summary>
        public class ItemBaseEvent : BaseEvent
        {
            protected ItemEventType action;
            protected ToolType toolType;
            protected int itemId;

            public ItemBaseEvent(ToolType _toolType, int _itemId, ItemEventType _action)
            {
                toolType = _toolType;
                itemId = _itemId;
                action = (ItemEventType)_action;
            }
            public override string ToString()
            {
                string msg = "";
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        msg += $"{eventTime}: {toolType}, {itemId}, {action}";
                        break;
                    case ItemEventType.StateUpdate:
                        msg += "";
                        break;
                }
                return msg;
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
                        //msg += JsonAddSubElement("State")
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            #region generate events


            public static void GenItemPickup(ToolType _toolType, int _itemId)
            {
                using (ItemBaseEvent e = new ItemBaseEvent(_toolType, _itemId, ItemEventType.Pickup))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

            public static void GenItemDropdown(ToolType _toolType, int _itemId)
            {
                using (ItemBaseEvent e = new ItemBaseEvent(_toolType, _itemId, ItemEventType.Dropdown))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }

            }

            #endregion
        }
    }
}