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
                if(body != null)
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
                if(leftHandController != null)
                {
                    leftHandController.transform.position = _leftPosition;
                    leftHandController.transform.rotation = _leftRotation;
                }
                else
                {
                    Debug.LogWarning("ServerPlayerVR.leftHandController is null");
                }
                if(rightHandController != null)
                {
                    rightHandController.transform.position = _rightPosition;
                    rightHandController.transform.rotation = _rightRotation;
                }
                else
                {
                    Debug.LogWarning("ServerPlayerVR.rightHandController is null");
                }

            }
        }
    }
}
