﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

using CEMSIM.VoiceChat;
using CEMSIM.GameLogic;

namespace CEMSIM
{
    namespace Network
    {
        /// <summary>
        /// This class process packets received from the server
        /// </summary>
        public class ClientHandle : MonoBehaviour
        {

            public static void InvalidPacketResponse(Packet _packet)
            {
                Debug.Log($"Received an invalid packet from Server");
            }

            public static void Welcome(Packet _packet)
            {
                string _msg = _packet.ReadString();
                int _myid = _packet.ReadInt32();

                Debug.Log($"Message from Server:{_msg} My Id:{_myid}");

                // set the client id based on received packet
                ClientInstance.instance.myId = _myid;

                // send response
                ClientSend.WelcomeReceived();

                // connect udp 
                ClientInstance.instance.udp.Connect(((IPEndPoint)ClientInstance.instance.tcp.socket.Client.LocalEndPoint).Port);

                // Mark TCP ready-to-use
                ClientInstance.instance.tcp.isTCPConnected = true;
                ClientInstance.instance.CheckConnection();
            }

            public static void WelcomeUDP(Packet _packet)
            {
                Debug.Log("UDP connection success");
                ClientInstance.instance.udp.isUDPConnected = true;
                ClientInstance.instance.CheckConnection();
            }

            /// <summary>
            /// Handle the TCP ping response from the server.
            /// </summary>
            /// <param name="_packet"></param>
            public static void TCPPingResponse(Packet _packet)
            {
                string _msg = _packet.ReadString();

                ClientPCConnetMenu.Instance.UpdateServerMessage("TCP:" + _msg);

            }

            /// <summary>
            /// Handle the UDP ping response from the server
            /// </summary>
            /// <param name="_packet"></param>
            public static void UDPPingResponse(Packet _packet)
            {
                string _msg = _packet.ReadString();

                ClientPCConnetMenu.Instance.UpdateServerMessage("UDP:" + _msg);
            }

            /// <summary>
            /// Handle the server's instruction of spawning a player
            /// </summary>
            /// <param name="_packet"></param>
            public static void SpawnPlayer(Packet _packet)
            {
                int _id = _packet.ReadInt32();
                string _username = _packet.ReadString();
                int _role_i = _packet.ReadInt32();
                int _avatar_i = _packet.ReadInt32();
                Vector3 _position = _packet.ReadVector3();
                Quaternion _rotation = _packet.ReadQuaternion();

                Debug.Log($"Spawn Player {_id} at {_position}");
                // spawn the player
                GameManager.instance.SpawnPlayer(_id, _username, _role_i, _avatar_i, _position, _rotation);

            }

            /// <summary>
            /// Update a player's position.
            /// </summary>
            /// <param name="_packet"></param>
            public static void PlayerPosition(Packet _packet)
            {
                int _id = _packet.ReadInt32();
                Vector3 _position = _packet.ReadVector3();
                Quaternion _rotation = _packet.ReadQuaternion();

                Vector3 _leftPosition = _packet.ReadVector3();
                Quaternion _leftRotation = _packet.ReadQuaternion();
                Vector3 _rightPosition = _packet.ReadVector3();
                Quaternion _rightRotation = _packet.ReadQuaternion();

                // TODO: needs to update both controllers' position
                

                // update corresponding player's position and hand position
                if (GameManager.players.ContainsKey(_id))
                {
                    //Player
                    GameManager.players[_id].SetPosition(_position, _rotation);
                    GameManager.players[_id].SetControllerPositions(_leftPosition, _leftRotation, _rightPosition, _rightRotation);
                }
                else
                {
                    Debug.LogWarning($"Player {_id} has not been created yet");
                }

            }

            /// <summary>
            /// Despawn (destroy) a player instructed by the packet.
            /// This process is tentative because it depends whether we need to store
            /// the player's history record.
            /// </summary>
            /// <param name="_packet"></param>
            public static void PlayerDisconnected(Packet _packet)
            {
                int _id = _packet.ReadInt32();

                // destroy the player's gameObject from the scene
                Destroy(GameManager.players[_id].gameObject);
                // remove the value in the player dictionary
                GameManager.players.Remove(_id);
            }

            public static void HeartBeatDetectionUDP(Packet _packet)
            {
                long sendTicks = _packet.getUtcTicks(); // the utc ticks that generates the received packet
                ClientSend.SendHeartBeatResponseUDP(sendTicks);
            }

            public static void HeartBeatDetectionTCP(Packet _packet)
            {
                long sendTicks = _packet.getUtcTicks(); // the utc ticks that generates the received packet
                ClientSend.SendHeartBeatResponseTCP(sendTicks);
            }

            public static void ItemList(Packet _packet)
            {
                int _listSize = _packet.ReadInt32();
                int _itemId = _packet.ReadInt32();
                int _itemTypeId = _packet.ReadInt32();

                Vector3 _position = _packet.ReadVector3();
                Quaternion _rotation = _packet.ReadQuaternion();

                ClientItemManager.instance.InitializeItem(_listSize, _itemId, _itemTypeId, _position, _rotation, _packet);
            }

            /// <summary>
            /// Update an item's position as instructed in packet
            /// </summary>
            /// <param name="_packet"></param>
            public static void ItemState(Packet _packet)
            {
                // interpret the packet
                int _itemId = _packet.ReadInt32();
                Vector3 _position = _packet.ReadVector3();
                Quaternion _rotation = _packet.ReadQuaternion();

                // update item
                ClientItemManager.instance.UpdateItemState(_itemId, _position, _rotation, _packet);
            }


            /// <summary>
            /// An item's ownership request sent by this client is denied by server, update ownership info accordingly
            /// </summary>
            /// <param name="_packet"></param>
            public static void OwnershipDeprivation(Packet _packet)
            {
                int _itemId = _packet.ReadInt32();
                ClientItemManager.instance.DropOwnership(_itemId, false);
            }


            public static void EnvironmentState(Packet _packet)
            {
                int _eventId = _packet.ReadInt32();
                GameManager.handleEventPacket(_eventId, _packet);
            }

            public static void VoiceChatData(Packet _packet)
            {
                ArraySegment<byte> _voiceData = _packet.ReadByteArraySegment();
                if (ClientInstance.instance.dissonanceClient != null)
                {
                    ClientInstance.instance.dissonanceClient.PacketDelivered(_voiceData); // any data, either TCP/UDP voice/message
                }
            }

            public static void VoiceChatPlayerId(Packet _packet)
            {
                int _fromClient = _packet.ReadInt32();
                string _playerId = _packet.ReadString();

                GameManager.players[_fromClient].gameObject.GetComponent<CEMSIMVoicePlayer>().ChangePlayerName(_playerId);
            }


        }
    }
}
