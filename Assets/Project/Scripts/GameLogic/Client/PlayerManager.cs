using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class PlayerManager : MonoBehaviour
        {
            public int id;
            public string username;
            public Roles role;

            [Header("Required")]
            public GameObject body;
            public GameObject leftHandController;
            public GameObject rightHandController;
            public GameObject displayName;
            

            void Start(){
	        	
        	}

        	void FixedUpdate(){

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
        }

        
    }
}