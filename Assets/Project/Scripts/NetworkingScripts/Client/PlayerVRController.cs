using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        namespace Client
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
}