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

            public void SetControllerPositions(Vector3 _leftPosition, Quaternion _leftRotation, Vector3 _rightPosition, Quaternion _rightRotation)
            {
                leftController.position = _leftPosition;
                leftController.rotation = _leftRotation;
                rightController.position = _rightPosition;
                rightController.rotation = _rightRotation;
            }
        }
    }
}
