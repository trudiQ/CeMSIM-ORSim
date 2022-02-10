using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using CEMSIM.Tools;
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
            protected int clientId;
            protected ToolBaseState toolState;

            public ItemBaseEvent(ToolType _toolType, int _itemId, ItemEventType _action, int _clientId, ToolBaseState _toolState = null)
            {
                toolType = _toolType;
                itemId = _itemId;
                action = (ItemEventType)_action;
                clientId = _clientId;

                switch (_action) {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        toolState = null; // no need for these two item event
                        break;
                    case ItemEventType.StateUpdate:
                        toolState = _toolState;
                        Debug.Assert(toolState != null);
                        break;
                }
            }



            public override string ToString()
            {
                string msg = "";
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        msg += $"{eventTime}: {clientId}, {toolType}, {itemId}, {action}";
                        break;
                    case ItemEventType.StateUpdate:
                        msg += $"{eventTime}: {clientId}, {toolType}, {itemId}, {action}";
                        msg += toolState.ToString();
                        break;
                }
                return msg;
            }

            public override string ToJson()
            {
                string msg = JsonPrefix();
                msg += JsonAddElement("Subject", clientId);
                msg += JsonAddElement("ToolType", toolType.ToString());
                msg += JsonAddElement("ItemId", itemId);
                switch (action)
                {
                    case ItemEventType.Pickup:
                    case ItemEventType.Dropdown:
                        msg += JsonAddElement("Action", action.ToString());
                        break;
                    case ItemEventType.StateUpdate:
                        msg += JsonAddElement("Action", action.ToString());
                        msg += JsonAddSubElement("State", toolState.ToJson());
                        break;
                }
                msg += JsonSuffix();
                return msg;
            }

            #region generate events


            public static void GenItemPickup(ToolType _toolType, int _itemId, int _clientId)
            {
                using (ItemBaseEvent e = new ItemBaseEvent(_toolType, _itemId, ItemEventType.Pickup, _clientId))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

            public static void GenItemDropdown(ToolType _toolType, int _itemId, int _clientId)
            {
                using (ItemBaseEvent e = new ItemBaseEvent(_toolType, _itemId, ItemEventType.Dropdown, _clientId))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }

            }

            //public static event Action<ToolType, int, NetworkStateInterface, int> onToolStateUpdateTrigger;

            public static void GenItemStateUpdate(ToolType _toolType, int _itemId, ToolBaseState _state, int _clientId)
            {
                using (ItemBaseEvent e = new ItemBaseEvent(_toolType, _itemId, ItemEventType.StateUpdate, _clientId, _state))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

            #endregion
        }
    }
}