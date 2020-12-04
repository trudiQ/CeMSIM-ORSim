using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class ServerPlayerVR : ServerPlayer
        {
            public void FixedUpdate()
            {
                SendToServer();
            }

            public void SetPosition(Vector3 _position, Quaternion _rotation)
            {
                transform.position = _position;
                transform.rotation = _rotation;
            }
        }
    }
}
