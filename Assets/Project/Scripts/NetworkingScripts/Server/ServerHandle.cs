using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.GameLogic;
using CEMSIM.VoiceChat;
using System;

namespace CEMSIM
{
    namespace Network
    {
        public class ServerHandle : MonoBehaviour
        {
            public static event Action<int, string> onPlayerEnterTrigger;
            public static event Action<int, Vector3, Quaternion, Vector3, Quaternion, Vector3, Quaternion> onPlayerMoveTrigger;
            public static event Action<int, int> onPlayerItemPickupTrigger;
            public static event Action<int, int> onPlayerItemDropoffTrigger;
            public static event Action<int, int, Vector3, Quaternion> onPlayerItemMoveTrigger;

            public static void InvalidPacketResponse(int _fromClient, Packet _packet)
            {
                Debug.LogWarning($"Client {_fromClient} sends an invalid packet");
                NetworkOverlayMenu.Instance.Log($"Client {_fromClient} sends an invalid packet");
                return;
            }

            
            public static void Welcome(int _fromClient, Packet _packet)
            {
                // Do nothing, because the "Welcome" packet is the first packet sent by the client through UDP
                // It is used to verify the establishment of UDP connection
                Debug.Log($"Welcome packet from {_fromClient}");
            }

            public static void WelcomeReceived(int _fromClient, Packet _packet)
            {
                int _clientIdCheck = _packet.ReadInt32();
                string _username = _packet.ReadString();

                Debug.Log($"Client {ServerInstance.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connects successfully and whose username is {_username}");
                NetworkOverlayMenu.Instance.Log($"Client {ServerInstance.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connects successfully and whose username is {_username}");

                // check whether the packet is from the client
                if (_clientIdCheck != _fromClient)
                {
                    Debug.LogWarning($"Client {_fromClient} has assumed with client id {_clientIdCheck} with username {_username}");
                    NetworkOverlayMenu.Instance.Log($"Warning: Client {_fromClient} has assumed with client id {_clientIdCheck} with username {_username}");
                    return;
                }

            }

            public static void WelcomeUDP(int _fromClient, Packet packet)
            {
                ServerSend.WelcomeUDP(_fromClient);
            }

            public static void PingUDP(int _fromClient, Packet _packet)
            {
                // Digest the packet
                int _clientIdCheck = _packet.ReadInt32();
                string _msg = _packet.ReadString();

                Debug.Log($"Client {ServerInstance.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} sends a UDP ping with msg {_msg}");
                NetworkOverlayMenu.Instance.Log($"Client {ServerInstance.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} sends a UDP ping with msg {_msg}");

                // check whether the packet is from the client
                if (_clientIdCheck != _fromClient)
                {
                    Debug.Log($"Client {_fromClient} has assumed with client id {_clientIdCheck} ");
                    NetworkOverlayMenu.Instance.Log($"Client {_fromClient} has assumed with client id {_clientIdCheck} ");
                    return;
                }

                // Create response
                // we reply the client with the same mesage appended with a check message
                string _replyMsg = _msg + " - server read";
                ServerSend.UDPPingReply(_fromClient, _msg);
            }


            public static void PingTCP(int _fromClient, Packet _packet)
            {
                // Digest the packet
                string _msg = _packet.ReadString();

                Debug.Log($"Client {ServerInstance.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} sends a TCP ping with msg {_msg}");
                NetworkOverlayMenu.Instance.Log($"Client {ServerInstance.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} sends a TCP ping with msg {_msg}");


                // Create response
                // we reply the client with the same mesage appended with a check message
                string _replyMsg = _msg + " - server read";
                ServerSend.TCPPingReply(_fromClient, _msg);
            }

            /// <summary>
            /// In response to client's SpawnRequest packet.
            /// Send the player into the game (simulation) and reply with the spawn detail
            /// </summary>
            /// <param name="_fromClient"></param>
            /// <param name="_packet"></param>
            public static void SpawnRequest(int _fromClient, Packet _packet)
            {
                string _username = _packet.ReadString();
                bool _vr = _packet.ReadBool();
                int _role_i = _packet.ReadInt32();
                int _avatar_i = _packet.ReadInt32();

                Debug.Log($"client{_fromClient}: Spawn player.");
                NetworkOverlayMenu.Instance.Log($"client{_fromClient}: Spawn player.");
                
                PlayerEnterTrigger(_fromClient, _username);

                // send back the packet with necessary inforamation about player locations
                ServerInstance.clients[_fromClient].SendIntoGame(_username, _vr, _role_i, _avatar_i);
            }

            /// <summary>
            /// Handle the user control on the player and respond with the updated player status
            /// </summary>
            /// <param name="_fromClient"></param>
            /// <param name="_packet"></param>
            public static void PlayerDesktopMovement(int _fromClient, Packet _packet)
            {
                bool[] _inputs = new bool[_packet.ReadInt32()];
                for (int i = 0; i < _inputs.Length; i++)
                {
                    _inputs[i] = _packet.ReadBool();
                }

                Quaternion _rotation = _packet.ReadQuaternion();

                //Debug.Log($"client{_fromClient}: move packet received.");
                PlayerManager fromPlayer = (PlayerManager)ServerInstance.clients[_fromClient].player;
                fromPlayer.SetInput(_inputs, _rotation);
            }

            /// <summary>
            /// Handle the VR position and orientation
            /// </summary>
            /// <param name="_fromClient"></param>
            /// <param name="_packet"></param>
            public static void PlayerVRMovement(int _fromClient, Packet _packet)
            {
                // avatar position
                Vector3 _position = _packet.ReadVector3();
                Quaternion _rotation = _packet.ReadQuaternion();

                // left and right controller positions
                Vector3 _leftPosition = _packet.ReadVector3();
                Quaternion _leftRotation = _packet.ReadQuaternion();
                Vector3 _rightPosition = _packet.ReadVector3();
                Quaternion _rightRotation = _packet.ReadQuaternion();

                //Debug.Log($"client{_fromClient}: move packet received.");
                PlayerManager fromPlayer = (PlayerManager)ServerInstance.clients[_fromClient].player;
                fromPlayer.SetPosition(_position, _rotation);
                fromPlayer.SetControllerPositions(_leftPosition, _leftRotation, _rightPosition, _rightRotation);

                PlayerMoveTrigger(_fromClient, _position, _rotation, _leftPosition, _leftRotation, _rightPosition, _rightRotation);
            }

            // update the TCP round-trip-time based on the response packet
            public static void HeartBeatDetectionTCP(int _fromClient, Packet _packet)
            {
                long utcnow = System.DateTime.UtcNow.Ticks;
                long sendticks = _packet.ReadInt64();
                ServerInstance.clients[_fromClient].tcp.lastHeartBeat = utcnow;
                ServerInstance.clients[_fromClient].tcp.rtt = utcnow - sendticks;
            }

            // update the UDP round-trip-time based on the response packet
            public static void HeartBeatDetectionUDP(int _fromClient, Packet _packet)
            {
                long utcnow = System.DateTime.UtcNow.Ticks;
                long sendticks = _packet.ReadInt64();
                ServerInstance.clients[_fromClient].udp.lastHeartBeat = utcnow;
                ServerInstance.clients[_fromClient].udp.rtt = utcnow - sendticks;
            }


            /// <summary>
            /// Update an item's position and state as instructed in packet
            /// </summary>
            /// <param name="_packet"></param>
            public static void ItemState(int _fromClient, Packet _packet)
            {
                // interpret the packet
                int _item_id = _packet.ReadInt32();
                Vector3 _position = _packet.ReadVector3();
                Quaternion _rotation = _packet.ReadQuaternion();


                // Update item position
                //Ignore if the client is not the owner of the item
                if (ServerItemManager.instance.itemList[_item_id].GetComponent<ItemController>().ownerId != _fromClient){   
                    Debug.Log(string.Format("client {0} attempted to update pos on item {1} but ignored by server",_fromClient,_item_id));
                    return;
                }
                ServerItemManager.instance.UpdateItemState(_item_id, _position, _rotation, _packet);
                PlayerItemMoveTrigger(_fromClient, _item_id, _position, _rotation);
            }

            /// <summary>
            /// Update an item's ownership as instructed in packet
            /// </summary>
            /// <param name="_packet"></param>
            public static void ItemOwnershipChange(int _fromClient, Packet _packet)
            {
                int _itemId = _packet.ReadInt32();
                bool _toGrab = _packet.ReadBool();

                GameObject item = ServerItemManager.instance.itemList[_itemId];
                ItemController itemCon = item.GetComponent<ItemController>();
                int currentOwner = itemCon.ownerId;
                Rigidbody rb = item.GetComponent<Rigidbody>();
                //This item is currently not owned by anyone\ or owned by the incoming client

                if(_toGrab)
                {
                    // user _fromClient wants the item
                    if (currentOwner == 0)
                    {
                        // server is the current owner
                        itemCon.ownerId = _fromClient;
                        //if the item is no longer controlled by server then set item to kinematic and no gravity
                        rb.isKinematic = true;                  //Prevent server physics system from changing the item's position & rotation
                        rb.useGravity = false;

                        PlayerItemPickupTrigger(_fromClient, _itemId);
                    }
                    else
                    {
                        // this item is controlled by another user
                        if (currentOwner != _fromClient)
                        {
                            ServerSend.ownershipDeprivation(currentOwner, _itemId);
                            itemCon.ownerId = _fromClient;

                            PlayerItemDropoffTrigger(currentOwner, _itemId);
                            PlayerItemPickupTrigger(_fromClient, _itemId);
                        }
                        else
                        {
                            // This shouldn't happen, unless some lost packets or lagging network
                            // Do nothing
                        }
                    }

                }
                else
                {
                    // the _fromClient user release the item. 
                    if (currentOwner == _fromClient)
                    {
                        // no other user wants this item. The server gets it by default
                        itemCon.ownerId = 0;

                        //if server regains control of an item then turn on gravity and set kinematic off
                        rb.isKinematic = false;
                        rb.useGravity = true;

                        PlayerItemDropoffTrigger(_fromClient, _itemId);
                    }
                    else
                    {
                        // some one is controlling this item already
                        // do nothing
                    }
                }

                Debug.Log($"The ownership of item {_itemId} - {itemCon.toolType} transfers from {currentOwner} to {itemCon.ownerId}");

            }


            public static void EnvironmentState(int _fromClient, Packet _packet)
            {
                int _eventId = _packet.ReadInt32();
                ServerNetworkManager.handleEventPacket(_fromClient, _eventId, _packet);
            }

            public static void VoiceChatData(int _fromClient, Packet _packet)
            {
                ArraySegment<byte> _voiceData = _packet.ReadByteArraySegment();
                if (ServerNetworkManager.instance.dissonanceServer != null)
                    ServerNetworkManager.instance.dissonanceServer.PacketDelivered(_fromClient, _voiceData); // any dissonance data, TCP/UDP, voice/message
                else
                    Debug.LogWarning("DissonanceServer has not been configured");
            }

            public static void VoiceChatPlayerId(int _fromClient, Packet _packet)
            {
                string _clientuuid = _packet.ReadString();

                // set playerId
                ServerInstance.clients[_fromClient].player.gameObject.GetComponent<CEMSIMVoicePlayer>().ChangePlayerName(_clientuuid);

                // update two dicionaries that maps client id and dissonance uuid
                ServerInstance.SetClientuuid(_fromClient, _clientuuid);

                // inform other clients
                ServerSend.SendVoiceChatPlayerId(_fromClient, _clientuuid, true);
            }

            #region event system
            public static void PlayerEnterTrigger(int _clientId, string _username)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onPlayerEnterTrigger != null)
                    onPlayerEnterTrigger(_clientId, _username);
            }
            public static void PlayerMoveTrigger(int _clientId, Vector3 _pos, Quaternion _rot, Vector3 _lft_pos, Quaternion _lft_rot, Vector3 _rgt_pos, Quaternion _rgt_rot)
            {
                if (onPlayerMoveTrigger != null)
                    onPlayerMoveTrigger(_clientId, _pos, _rot, _lft_pos, _lft_rot, _rgt_pos, _rgt_rot);
            }
            public static void PlayerItemPickupTrigger(int _clientId, int _itemId)
            {
                if (onPlayerItemPickupTrigger != null)
                    onPlayerItemPickupTrigger(_clientId, _itemId);
            }
            public static void PlayerItemDropoffTrigger(int _clientId, int _itemId)
            {
                if (onPlayerItemDropoffTrigger != null)
                    onPlayerItemDropoffTrigger(_clientId, _itemId);
            }
            public static void PlayerItemMoveTrigger(int _clientId, int _itemId, Vector3 _pos, Quaternion _rot)
            {
                if (onPlayerItemMoveTrigger != null)
                    onPlayerItemMoveTrigger(_clientId, _itemId, _pos, _rot);
            }
            #endregion

        }
    }
}
