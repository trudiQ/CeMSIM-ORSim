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
                public GameObject VRCamera;

                private void FixedUpdate()
                {
                    SendInputToServer();
                }

                /// <summary>
                /// Collect user's input control onto the player, and send the input to the server
                /// </summary>
                private void SendInputToServer()
                {
                    //Send user position to the server
                    ClientSend.PlayerVRMovement(VRCamera.transform.position, VRCamera.transform.rotation);
                }
            }
        }
    }
}
