using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

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
                Vector3 _position = _packet.ReadVector3();
                Quaternion _rotation = _packet.ReadQuaternion();

                Debug.Log($"Spawn Player {_id} at {_position}");
                // spawn the player
                GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);

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
                    GameManager.players[_id].transform.position = _position;
                    GameManager.players[_id].transform.rotation = _rotation;
                    //Hands
                    GameManager.players[_id].GetComponent<PlayerManager>().leftHandController.transform.position = _leftPosition;
                    GameManager.players[_id].GetComponent<PlayerManager>().leftHandController.transform.rotation = _leftRotation;
                    GameManager.players[_id].GetComponent<PlayerManager>().rightHandController.transform.position = _rightPosition;
                    GameManager.players[_id].GetComponent<PlayerManager>().rightHandController.transform.rotation = _rightRotation;

                    //GameManager.players[_id].transform.GetChild(3).transform.position = _rightPosition;
                    //GameManager.players[_id].transform.GetChild(3).transform.rotation = _rightRotation;
                    //GameManager.players[_id].transform.GetChild(2).transform.position = _leftPosition;
                    //GameManager.players[_id].transform.GetChild(2).transform.rotation = _leftRotation;

                }
                else
                {
                    Debug.LogWarning($"Player {_id} has not been created yet");
                }

            }

            /// <summary>
            /// Update a player's rotation.
            /// </summary>
            /// <param name="_packet"></param>
            public static void PlayerRotation(Packet _packet)
            {
                //int _id = _packet.ReadInt32();
                //Quaternion _rotation = _packet.ReadQuaternion();

                //Debug.Log($"Player {_id} rotation to {_rotation}");

                //// update corresponding player's position
                //if (GameManager.players.ContainsKey(_id))
                //{
                //    GameManager.players[_id].transform.rotation = _rotation;
                //}
                //else
                //{
                //    Debug.LogWarning($"Player {_id} has not been created yet");
                //}
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

            /// <summary>
            /// Update an item's position as instructed in packet
            /// </summary>
            /// <param name="_packet"></param>
            public static void ItemPosition(Packet _packet)
            {
                // interpret the packet
                int _item_id = _packet.ReadInt32();
                Vector3 _position = _packet.ReadVector3();
                Quaternion _rotation = _packet.ReadQuaternion();

                // update item
                GameObject itemManager = GameObject.Find("ItemManager");
                ClientItemManager CIM = (ClientItemManager)itemManager.GetComponent(typeof(ClientItemManager));
                CIM.UpdateItemPosition(_item_id, _position, _rotation);
            }


            /// <summary>
            /// An item's ownership request sent by this client is denied by server, update ownership info accordingly
            /// </summary>
            /// <param name="_packet"></param>
            public static void OwnershipDenial(Packet _packet)
            {
                int _item_id = _packet.ReadInt32();
                GameObject itemManager = GameObject.Find("ItemManager");
                ClientItemManager CIM = (ClientItemManager)itemManager.GetComponent(typeof(ClientItemManager));
                CIM.DropOwnership(CIM.itemList[_item_id]);
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
                    ClientInstance.instance.dissonanceClient.NetworkReceivedPacket(_voiceData); // any data, either TCP/UDP voice/message
                else
                    Debug.LogWarning("DissonanceClient has not been configured");
            }


        }
    }
}
