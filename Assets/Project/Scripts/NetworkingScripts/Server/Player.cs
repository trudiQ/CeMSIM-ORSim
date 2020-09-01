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

                //public Player(int _id, string _username) // unity monobehavior doesn't allow constructor
                public void Initialize(int _id, string _username)
                {
                    id = _id;
                    username = _username;
                }
            }
        }
    }
}