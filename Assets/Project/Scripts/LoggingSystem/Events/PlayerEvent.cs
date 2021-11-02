using System;
using System.Collections;
using System.Collections.Generic;
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
            DropoffItem
        }
        public class PlayerEvent : BaseEvent
        {
            private int playerId;
            private PlayerActionType playerAction;
            private Vector3 position;
            private Vector3 rotation;
            private int itemId;

            public PlayerEvent(int _playerId, PlayerActionType _playerAction, Vector3 _position, Quaternion _rotation, int _itemId=-1)
            {
                eventTime = DateTime.UtcNow.Ticks;
                playerId = _playerId;
                playerAction = _playerAction;
                position = _position;
                rotation = _rotation.eulerAngles;
                itemId = _itemId;
            }

            public static string GetHeader()
            {
                string msg = "EventTime,PlayerId,Pos_x,Pos_y,Pos_z,Rot_x,Rot_y,Rot_z,ActionId,ItemId";
                return msg;
            }

            public override string ToString()
            {
                string msg = string.Format("{0}: {1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    eventTime,
                    playerId,
                    playerAction,
                    position.x,
                    position.y,
                    position.z,
                    rotation.x,
                    rotation.y,
                    rotation.z,
                    itemId
                    );
                return msg;
            }

            public override string ToCSV()
            {
                string msg = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    eventTime,
                    playerId,
                    (int)playerAction,
                    position.x,
                    position.y,
                    position.z,
                    rotation.x,
                    rotation.y,
                    rotation.z,
                    itemId
                    );
                return msg;
            }

        }
    }
}