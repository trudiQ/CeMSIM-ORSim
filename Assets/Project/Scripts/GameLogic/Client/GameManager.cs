using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.Network;
using System;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class GameManager : MonoBehaviour
        {
            public static GameManager instance;

            // store all information about all players in game
            public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

            [Header("Player Prefabs")]
            public GameObject localPlayerVR;
            public GameObject localPlayerPrefab;
            public GameObject playerPrefab;

            [Header("Events")]
            public GameObject roomLightButton;

            // event management
            private delegate void eventHandler(Packet _packet);
            private static Dictionary<int, eventHandler> eventHandlers;


            private void Awake()
            {
                if (instance == null)
                {
                    instance = this;
                }
                else if (instance != this)
                {
                    // We only allow one instance of this class to exist.
                    // Destroy current instance if different from the current one.
                    Debug.Log("Another instance already exists. Destroy this one.");
                    Destroy(this);
                }
            }

            private void Start()
            {
                GameManager.InitializeEventId();
            }

            /// <summary>Spawns a player, not necessarily the player controlled by the current user.</summary>
            /// <param name="_id">The player's ID.</param>
            /// <param name="_name">The player's name.</param>
            /// <param name="_position">The player's starting position.</param>
            /// <param name="_rotation">The player's starting rotation.</param>
            public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
            {
                GameObject _player;


                if (_id == ClientInstance.instance.myId)
                {
                    if (localPlayerVR.activeInHierarchy)
                    {
                        _player = localPlayerVR;
                        _player.GetComponent<PlayerVRController>().enabled = true;
                    }
                    else
                    {
                        // create player for client
                        _player = Instantiate(localPlayerPrefab, _position, _rotation);
                    }
                }
                else
                {
                    // create player for another client
                    _player = Instantiate(playerPrefab, _position, _rotation);
                    // Child 1 is the username gameobject
                    // P.S. Only the prefab of other players has "username"
                    _player.transform.GetChild(1).gameObject.GetComponent<TextMesh>().text = _username; 

                }

                _player.GetComponent<PlayerManager>().id = _id;
                _player.GetComponent<PlayerManager>().username = _username;


                // record the player instance in the players dictionary
                players.Add(_id, _player.GetComponent<PlayerManager>());
            }


            #region Environment State Updating Handling
            public static void handleEventPacket(int eventId, Packet _packet)
            {
                if (eventHandlers.ContainsKey(eventId))
                {
                    eventHandlers[eventId](_packet);
                }
                else
                {
                    Debug.LogWarning($"event id {eventId} is not supported");
                }
            }

            public static void SetRoomLightState(Packet _packet)
            {
                bool onOff = _packet.ReadBool();
                GameManager.instance.roomLightButton.GetComponent<RoomLightsOnOff>().SetSwitchState(onOff);

            }


            public static void SendRoomLightState(bool switchState)
            {
                List<byte> message = new List<byte>();
                message.AddRange(BitConverter.GetBytes(switchState));

                ClientSend.SendEnvironmentState((int)EnvironmentId.roomLight, message.ToArray());

            }


            private static void InitializeEventId()
            {
                eventHandlers = new Dictionary<int, eventHandler>
                {
                    {(int)EnvironmentId.roomLight, SetRoomLightState}
                };
            }

            #endregion
        }
    }
}
