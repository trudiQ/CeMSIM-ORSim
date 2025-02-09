using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.Network;
using System;
using CEMSIM.VoiceChat;

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
            //public GameObject playerPrefab;
            //public List<GameObject> playerPrefabs = new List<GameObject>();
            public List<NetRoleAvatarList> remotePlayerPrefabs;


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
            public void SpawnPlayer(int _id, string _username, int _role_i, int _avatar_i, Vector3 _position, Quaternion _rotation)
            {
                GameObject _player;
                bool _isVR = true;

                // TODO: Currently, no difference when the client knows the role of any other client.
                Roles _role = Roles.surgeon;

                if (Enum.IsDefined(typeof(Roles), _role_i))
                    _role = (Roles)_role_i;

                Debug.Log($"Spawning player {_id} - {_username} - {_role}");


                if (_id == ClientInstance.instance.myId)
                {
                    //if (localPlayerVR.activeInHierarchy)
                    if (localPlayerVR != null)
                    {
                        _isVR = true;
                        _player = localPlayerVR;
                        //_player.GetComponent<PlayerVRController>().enabled = true;
                    }
                    else
                    {
                        // create the desktop player for client
                        _isVR = false;
                        _player = Instantiate(localPlayerPrefab, _position, _rotation);
                    }
                    ClientInstance.instance.dissonanceClient.Connect();
                    _player.GetComponent<CEMSIMVoicePlayer>().initialization(true);
                    Debug.Log("Connect to dissonance server");
                }
                else
                {
                    // create the player avatar of one existing client
                    int _role_id = (int)_role;
                    _player = Instantiate(remotePlayerPrefabs[_role_id].avatars[_avatar_i].avatarPrefab_NetRig, new Vector3(_position.x, 0f, _position.z), Quaternion.identity);
                    // Since the new rig model treats the initial y-axis as the floor, we should first spawn it to a coordinate with 0 as y-axis
                    // then pull it to the correct position.
                    _player.GetComponent<PlayerManager>().enabled = true;
                    //_player.GetComponent<PlayerManager>().SetPosition(_position, _rotation);

                    // initialize the Dissonance player as a remote player (false)
                    _player.GetComponent<CEMSIMVoicePlayer>().initialization(false);


                }
                _player.GetComponent<PlayerManager>().InitializePlayerManager(_id, _username, _role, _avatar_i, true, _isVR);

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
