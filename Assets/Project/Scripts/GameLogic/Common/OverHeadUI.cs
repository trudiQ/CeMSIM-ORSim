using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace CEMSIM
{
    namespace GameLogic
    {
        public class OverHeadUI : MonoBehaviour
        {

            public GameObject displayName;
            public GameObject displayRole;
            // Start is called before the first frame update


            // Update is called once per frame
            public void SetDisplayName(string _name)
            {
                if (displayName != null)
                {
                    displayName.GetComponentInChildren<Text>().text = String.Format($"{_name}");
                }
                else
                {
                    Debug.LogWarning("Username is null. Maybe the player controlled avatar");
                }
            }

            public void SetRoleName(Roles _role)
            {
                if (displayName != null)
                {
                    displayRole.GetComponentInChildren<Text>().text = String.Format($"{_role}");
                }
                else
                {
                    Debug.LogWarning("Role is null. Please check.");
                }
            }
        }
    }
}