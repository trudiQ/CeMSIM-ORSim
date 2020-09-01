using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        namespace Server
        {
            public class Player : MonoBehaviour
            {
                public int id;
                public string username;
                public CharacterController controller;
                public float gravity_per_tick = Constants.GRAVITY;
                public float moveSpeed_per_tick = Constants.MOVE_SPEED_PER_SECOND;
                public float jumpSpeed_per_tick = Constants.JUMP_SPEED_PER_SECOND;

                private bool[] inputs;
                private float yVelocity = 0;    // player's vertical velocity


                private void Start()
                {
                    gravity_per_tick *= Time.fixedDeltaTime * Time.fixedDeltaTime;
                    moveSpeed_per_tick *= Time.fixedDeltaTime;
                    jumpSpeed_per_tick *= Time.fixedDeltaTime;
                }

                //public Player(int _id, string _username) // unity monobehavior doesn't allow constructor
                public void Initialize(int _id, string _username)
                {
                    id = _id;
                    username = _username;
                    //position = _spawnPosition;
                    //rotation = Quaternion.identity;


                    inputs = new bool[5];

                }

                public void FixedUpdate()
                {
                    Vector2 _inputDirection = Vector2.zero;
                    if (inputs[0]) // W
                    {
                        _inputDirection.y += 1;
                    }
                    if (inputs[1]) // S
                    {
                        _inputDirection.y -= 1;
                    }
                    if (inputs[2]) // A
                    {
                        _inputDirection.x -= 1;
                    }
                    if (inputs[3]) // D
                    {
                        _inputDirection.x += 1;
                    }

                    Move(_inputDirection);

                }

                private void Move(Vector2 _inputDirection)
                {
                    /*
                     * Here, we assume perfect network status. So we don't consider quick 
                     * movements and quick turning due to the packet loss.
                     * **/

                    // In unity, the following two variables are in "transform"
                    //// Transform user's relative coordinating system to absolute coordinating system
                    //Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
                    //Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

                    Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
                    Vector3 _moveVector = _moveDirection * moveSpeed_per_tick;
                    //position += _moveDirection * moveSpeed;

                    // consider gravity if player is on the ground
                    if (controller.isGrounded)
                    {
                        yVelocity = 0f;
                        if (inputs[4]) // whether jump key (space) is pressed
                        {
                            yVelocity = jumpSpeed_per_tick;
                        }
                    }
                    yVelocity += gravity_per_tick; // gravity * 1 tick 

                    _moveVector.y = yVelocity;

                    controller.Move(_moveVector);


                    // public the position to every client, but public the facing direction to all but the player
                    ServerSend.PlayerPosition(this);
                    ServerSend.PlayerRotation(this);

                }

                public void SetInput(bool[] _inputs, Quaternion _rotation)
                {
                    inputs = _inputs;
                    transform.rotation = _rotation;
                }
            }
        }
    }
}