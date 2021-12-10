using System;
using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public enum PlayerActionType
        {
            EnterGame=1,
            ExitGame,
            Move,
            PickupItem,
            DropoffItem,
            MoveItem
        }
        public class PlayerEvent : BaseEvent
        {
            /// <summary>
            /// TODO: both controllers and item positions are not stored into files. This also includes the item state.
            /// </summary>
            private int clientId;
            private PlayerActionType eventType;
            private Vector3 player_pos;
            private Vector3 player_rot;
            private Vector3 lft_ctl_pos;
            private Vector3 lft_ctl_rot;
            private Vector3 rgt_ctl_pos;
            private Vector3 rgt_ctl_rot;
            private int itemId;
            private Vector3 item_pos;
            private Vector3 item_rot;

            public PlayerEvent(int _clientId, PlayerActionType _playerAction, Vector3 _position, Quaternion _rotation, int _itemId=-1)
            {
                //eventTime = DateTime.UtcNow - LogManager.instance.SystemStartTime;
                clientId = _clientId;
                eventType = _playerAction;
                player_pos = _position;
                player_rot = _rotation.eulerAngles;
                itemId = _itemId;
            }

            /// <summary>
            /// Generate the title line of the CSV file
            /// </summary>
            /// <returns></returns>
            public static string GetHeader()
            {
                string msg = "EventTime,PlayerId,ActionId,Pos_x,Pos_y,Pos_z,Rot_x,Rot_y,Rot_z,ItemId";
                return msg;
            }

            public override string ToString()
            {
                string msg = $"{eventTime}: player{clientId},{eventType},item{itemId},{player_pos.x},{player_pos.y},{player_pos.z},{player_rot.x},{player_rot.y},{player_rot.z}";
                return msg;
            }

            public override string ToCSV()
            {
                string msg = $"{eventTime},{clientId},{eventType},{player_pos.x},{player_pos.y},{player_pos.z},{player_rot.x},{player_rot.y},{player_rot.z},{itemId}";
                return msg;
            }

            public override string ToJson()
            {
                string msg = JsonPrefix();
                msg += JsonAddElement("EventType", eventType.ToString());
                msg += JsonAddElement("PlayerId", clientId);

                switch (eventType)
                {
                    case PlayerActionType.EnterGame:
                    case PlayerActionType.ExitGame:
                        break;
                    case PlayerActionType.PickupItem:
                    case PlayerActionType.DropoffItem:
                        msg += JsonAddElement("ItemId", itemId);
                        break;
                    case PlayerActionType.Move:
                        string posMsg = JsonAddElement("Body", JsonParsePos(player_pos, player_rot)) + JsonAddElement("LeftController", JsonParsePos(lft_ctl_pos, lft_ctl_rot)) + JsonAddElement("RightController", JsonParsePos(rgt_ctl_pos, rgt_ctl_rot));
                        msg += JsonAddSubElement("Position", posMsg);
                        break;
                    case PlayerActionType.MoveItem:
                        string itemStateMsg = JsonAddElement("ItemPosition", JsonParsePos(item_pos, item_rot));
                        msg += JsonAddElement("ItemId", itemId) + JsonAddSubElement("ItemState", itemStateMsg);
                        break;

                }
                msg += "}}";
                return msg;
            }

            #region generate events

            //public static event Action<int, string> onPlayerEnterTrigger;
            //public static event Action<int, Vector3, Quaternion, Vector3, Quaternion, Vector3, Quaternion> onPlayerMoveTrigger;
            //public static event Action<int, int> onPlayerItemPickupTrigger;
            //public static event Action<int, int> onPlayerItemDropdownTrigger;
            //public static event Action<int, int, Vector3, Quaternion> onPlayerItemMoveTrigger;

            public static void GenPlayerEnterEvent(int _clientId, string _username)
            {
                using (PlayerEvent e = new PlayerEvent(_clientId, PlayerActionType.EnterGame, ServerGameConstants.INIT_SPAWNING_POSITION, ServerGameConstants.INIT_SPAWNING_ROTATION))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToPlayerEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

            public static void GenPlayerExitEvent(int _clientId)
            {
                using (PlayerEvent e = new PlayerEvent(_clientId, PlayerActionType.ExitGame, ServerGameConstants.INIT_SPAWNING_POSITION, ServerGameConstants.INIT_SPAWNING_ROTATION))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToPlayerEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

            public static void GenPlayerMoveEvent(int _clientId, Vector3 _pos, Quaternion _rot, Vector3 _lft_pos, Quaternion _lft_rot, Vector3 _rgt_pos, Quaternion _rgt_rot)
            {
                using (PlayerEvent e = new PlayerEvent(_clientId, PlayerActionType.Move, _pos, _rot))
                {
                    e.AddToPlayerEventQueue();
                }
            }

            public static void GenPlayerItemPickupEvent(int _clientId, int _itemId)
            {
                using (PlayerEvent e = new PlayerEvent(_clientId, PlayerActionType.PickupItem, ServerGameConstants.INIT_SPAWNING_POSITION, ServerGameConstants.INIT_SPAWNING_ROTATION, _itemId))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToPlayerEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

            public static void GenPlayerItemDropoffEvent(int _clientId, int _itemId)
            {
                using (PlayerEvent e = new PlayerEvent(_clientId, PlayerActionType.DropoffItem, ServerGameConstants.INIT_SPAWNING_POSITION, ServerGameConstants.INIT_SPAWNING_ROTATION, _itemId))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToPlayerEventQueue();
                    e.AddToJsonEventQueue();
                }
            }

            public static void GenPlayerItemMoveEvent(int _clientId, int _itemId, Vector3 _item_pos, Quaternion _item_rot)
            {
                using (PlayerEvent e = new PlayerEvent(_clientId, PlayerActionType.MoveItem, ServerGameConstants.INIT_SPAWNING_POSITION, ServerGameConstants.INIT_SPAWNING_ROTATION, _itemId))
                {
                    //e.AddToPlayerEventQueue();

                }
            }
            #endregion
        }
    }
}