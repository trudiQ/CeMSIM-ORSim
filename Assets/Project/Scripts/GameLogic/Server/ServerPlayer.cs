using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.Network;

namespace CEMSIM
{
    namespace GameLogic
    {
        public abstract class ServerPlayer : MonoBehaviour
        {
            public int id;
            public string username;
            public CharacterController controller;

            public void Initialize(int _id, string _username)
            {
                id = _id;
                username = _username;
            }

            public void SendToServer()
            {
                // public the position to every client, but public the facing direction to all but the player
                ServerSend.PlayerPosition(this);
                ServerSend.PlayerRotation(this);
            }
        }
    }
}