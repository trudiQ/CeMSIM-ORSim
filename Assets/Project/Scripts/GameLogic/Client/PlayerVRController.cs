using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.Network;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class PlayerVRController : MonoBehaviour
        {
            public Transform VRCamera;

            private void FixedUpdate()
            {
                ClientSend.PlayerVRMovement();
            }
        }
    }
}