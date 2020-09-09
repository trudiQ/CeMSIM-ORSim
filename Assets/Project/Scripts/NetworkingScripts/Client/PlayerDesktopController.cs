using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        namespace Client
        {
            public class PlayerDesktopController : MonoBehaviour
            {
                private void FixedUpdate()
                {
                    SendInputToServer();
                }

                /// <summary>
                /// Collect user's input control onto the player, and send the input to the server
                /// </summary>
                private void SendInputToServer()
                {
                    bool[] _inputs = new bool[]
                    {
                        Input.GetKey(KeyCode.W),
                        Input.GetKey(KeyCode.S),
                        Input.GetKey(KeyCode.A),
                        Input.GetKey(KeyCode.D),
                        Input.GetKey(KeyCode.Space), // for jump
                    };

                    ClientSend.PlayerDesktopMovement(_inputs);
                }
            }
        }
    }
}