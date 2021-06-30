using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class ServerPlayerDesktop : ServerPlayer
        {
            private float gravity_per_tick = ServerGameConstants.GRAVITY;
            private float moveSpeed_per_tick = ServerGameConstants.MOVE_SPEED_PER_SECOND;
            private float jumpSpeed_per_tick = ServerGameConstants.JUMP_SPEED_PER_SECOND;

            private bool[] keyboardInputs = new bool[5];
            private float yVelocity = 0;    // player's vertical velocity (jump)

            // Start is called before the first frame update
            void Start()
            {
                gravity_per_tick *= Time.fixedDeltaTime * Time.fixedDeltaTime;
                moveSpeed_per_tick *= Time.fixedDeltaTime;
                jumpSpeed_per_tick *= Time.fixedDeltaTime;
            }

            public void FixedUpdate()
            {
                
                UpdateKeyboardUserPosition();
                SendToClients();

            }

            private void UpdateKeyboardUserPosition()
            {
                /*
                    * Here, we assume perfect network status. So we don't consider quick 
                    * movements and quick turning due to the packet loss.
                    * **/

                Vector2 _inputDirection = Vector2.zero;
                if (keyboardInputs[0]) // W
                {
                    _inputDirection.y += 1;
                }
                if (keyboardInputs[1]) // S
                {
                    _inputDirection.y -= 1;
                }
                if (keyboardInputs[2]) // A
                {
                    _inputDirection.x -= 1;
                }
                if (keyboardInputs[3]) // D
                {
                    _inputDirection.x += 1;
                }

                Vector3 _moveDirection = body.transform.right * _inputDirection.x + body.transform.forward * _inputDirection.y;
                Vector3 _moveVector = _moveDirection * moveSpeed_per_tick;
                //position += _moveDirection * moveSpeed;

                // consider gravity if player is on the ground
                if (controller != null && controller.isGrounded)
                {
                    yVelocity = 0f;
                    if (keyboardInputs[4]) // whether jump key (space) is pressed
                    {
                        yVelocity = jumpSpeed_per_tick;
                    }
                }
                yVelocity += gravity_per_tick; // gravity * 1 tick 

                _moveVector.y = yVelocity;

                // player position of the keyboard user is managed by the CharacterController
                if (controller != null)
                    controller.Move(_moveVector); 
            }

            public void SetInput(bool[] _inputs, Quaternion _rotation)
            {
                keyboardInputs = _inputs;
                if (body != null)
                    body.transform.rotation = _rotation;
                else
                    Debug.LogWarning("avatar body is not specified");
            }
        }
    }
}
