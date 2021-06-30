using CEMSIM.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class PlayerManager : MonoBehaviour
        {
            public int id;                  //< id of the avatar (specified by the server)
            public string username;         //< username specified by the player
            public Roles role;              //< surgeon, nurse, etc?
            public bool isClientSide;       //< specify whether the current avatar is spawned at the client or the server side
            public bool isVR;               //< a VR-controlled or keyboard-controlled avatar

            [Header("Required by Client and Server Avatar")]
            public GameObject body;
            public GameObject leftHandController;
            public GameObject rightHandController;
            public GameObject displayName;


            // server side player management
            [Header("Required by Server Avatar")]
            public CharacterController controller;  //< a easy controller used to implement collision detection

            // server side keyboard player management
            private float gravity_per_tick = ServerGameConstants.GRAVITY;
            private float moveSpeed_per_tick = ServerGameConstants.MOVE_SPEED_PER_SECOND;
            private float jumpSpeed_per_tick = ServerGameConstants.JUMP_SPEED_PER_SECOND;

            private bool[] keyboardInputs = new bool[5];
            private float yVelocity = 0;    // player's vertical velocity (jump)

            void Start(){
                gravity_per_tick *= Time.fixedDeltaTime * Time.fixedDeltaTime;
                moveSpeed_per_tick *= Time.fixedDeltaTime;
                jumpSpeed_per_tick *= Time.fixedDeltaTime;
            }

        	void FixedUpdate(){
                if(isClientSide && id == ClientInstance.instance.myId)
                {
                    if (isVR)
                    {
                        ClientSend.PlayerVRMovement();
                    }
                    else
                    {
                        // Desktop user
                        SendInputToServer();
                    }
                }
                else
                {
                    // Server user
                    if (!isVR)
                    {
                        UpdateKeyboardUserPosition();
                    }

                    ServerSend.PlayerPosition(this);
                }
        	}


            #region Desktop User Keyboard Inputs Collection
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

            /// <summary>
            /// Update keyboard user position via the attached CharacterController based on the received keyboard inputs
            /// </summary>
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

            #endregion

            #region Set rig mode attributes
            public void InitializePlayerManager(int _id, string _username, Roles _role, bool _isClientSide, bool _isVR)
            {
                id = _id;
                username = _username;
                role = _role;
                isClientSide = _isClientSide;
                isVR = _isVR;

                SetDisplayName(_username);

                if (!isClientSide)
                {
                    if(controller == null)
                    {
                        Debug.LogWarning("Character Controller is null. This may affect the avatar collision detection and/or movement (especially for keyboard)");
                    }
                }
                if (controller != null)
                {
                    controller.enabled = !isClientSide; // Only the server side avatar needs the controller 
                }
            }

            public void SetDisplayName(string _name)
            {
                if (displayName != null)
                {
                    displayName.GetComponent<TextMesh>().text = _name;
                }
                else
                {
                    Debug.LogWarning("username is null");
                }
            }

            public void SetPosition(Vector3 _position, Quaternion _rotation)
            {
                if (body != null)
                {
                    body.transform.position = _position;
                    body.transform.rotation = _rotation;
                }
                else
                {
                    Debug.LogWarning("ServerPlayerVR.body is null");
                }
            }

            public void SetControllerPositions(Vector3 _leftPosition, Quaternion _leftRotation, Vector3 _rightPosition, Quaternion _rightRotation)
            {
                if (leftHandController != null)
                {
                    leftHandController.transform.position = _leftPosition;
                    leftHandController.transform.rotation = _leftRotation;
                }
                else
                {
                    Debug.LogWarning("ServerPlayerVR.leftHandController is null");
                }
                if (rightHandController != null)
                {
                    rightHandController.transform.position = _rightPosition;
                    rightHandController.transform.rotation = _rightRotation;
                }
                else
                {
                    Debug.LogWarning("ServerPlayerVR.rightHandController is null");
                }

            }

            public void SetInput(bool[] _inputs, Quaternion _rotation)
            {
                if (isVR)
                {
                    Debug.LogError("VR user should not use keyboard to update the position");
                    return;
                }

                keyboardInputs = _inputs;
                if (body != null)
                    body.transform.rotation = _rotation;
                else
                    Debug.LogWarning("avatar body is not specified");
            }
            #endregion
        }


    }
}