using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.GameLogic;
using System;

namespace CEMSIM
{
    namespace Network
    {
        public class ServerSend : MonoBehaviour
        {

            #region Send TCP Packets
            /// <summary>
            /// Send a packet to a particular client
            /// </summary>
            /// <param name="_toClient">Client id</param>
            /// <param name="_packet">The packet</param>
            /// <param name="addTime"> Whether include the generate time in the packet header</param>
            private static void SendTCPData(int _toClient, Packet _packet, bool addTime = false)
            {
                _packet.WriteHeader(addTime);   // write the header information including length
                ServerInstance.clients[_toClient].tcp.SendData(_packet);

            }

            /// <summary>
            /// Multicast packet to all clients
            /// </summary>
            /// <param name="_packet">The packet to multicast</param>
            /// <param name="addTime"> Whether include the generate time in the packet header</param>
            private static void MulticastTCPData(Packet _packet, bool addTime = false)
            {
                _packet.WriteHeader(addTime);   // write the header information including length
                for (int i = 1; i < ServerInstance.maxPlayers; i++)
                {
                    ServerInstance.clients[i].tcp.SendData(_packet);
                }
            }

            /// <summary>
            /// Multicast packet to all but one client.
            /// </summary>
            /// <param name="_exceptClient">The client to who no packet is sent</param>
            /// <param name="_packet"> The packet to multicast</param>
            /// <param name="addTime"> Whether include the generate time in the packet header</param>
            private static void MulticastExceptOneTCPData(int _exceptClient, Packet _packet, bool addTime = false)
            {
                _packet.WriteHeader(addTime);   // write the header information including length
                for (int i = 1; i < ServerInstance.maxPlayers; i++)
                {
                    if (i != _exceptClient)
                    {
                        ServerInstance.clients[i].tcp.SendData(_packet);
                    }
                }
            }

            #endregion

            #region Send UDP Packets

            /// <summary>
            /// Send a packet to a particular client
            /// </summary>
            /// <param name="_toClient">Client id</param>
            /// <param name="_packet">The packet to send</param>
            private static void SendUDPData(int _toClient, Packet _packet, bool addTime = false)
            {
                _packet.WriteHeader(addTime);
                ServerInstance.clients[_toClient].udp.SendData(_packet);
            }

            /// <summary>
            /// Multicast packet to all clients
            /// </summary>
            /// <param name="_packet">The packet to multicast</param>
            private static void MulticastUDPData(Packet _packet, bool addTime = false)
            {
                _packet.WriteHeader(addTime);
                for (int i = 1; i < ServerInstance.maxPlayers; i++)
                {
                    ServerInstance.clients[i].udp.SendData(_packet);
                }
            }

            /// <summary>
            /// Multicast packet to all but one client.
            /// </summary>
            /// <param name="_exceptClient">The client to who no packet is sent</param>
            /// <param name="_packet"> The packet to multicast</param>
            private static void MulticastExceptOneUDPData(int _exceptClient, Packet _packet, bool addTime = false)
            {
                _packet.WriteHeader(addTime);
                for (int i = 1; i < ServerInstance.maxPlayers; i++)
                {
                    if (i != _exceptClient)
                    {
                        ServerInstance.clients[i].udp.SendData(_packet);
                    }
                }
            }

            #endregion


            #region Generate Server->Client Packets
            /// <summary>
            /// Send a welcome packet to a particular client
            /// </summary>
            /// <param name="_toClient">id of the client to receive the packet</param>
            /// <param name="_msg"></param>
            public static void Welcome(int _toClient, string _msg)
            {
                using (Packet _packet = new Packet((int)ServerPackets.welcome))
                {
                    /*
                        * Packet format: <---|Data length|msg string|client id|
                        * Data length is taken cared by the SendTCPData function. 
                        * Here, user focuses on "msg string" and "client id" .
                        * **/
                    _packet.Write(_msg);
                    _packet.Write(_toClient); // add the client id assigned to the client

                    SendTCPData(_toClient, _packet, true);
                }
            }

            public static void WelcomeUDP(int _toClient)
            {
                using(Packet _packet = new Packet((int)ServerPackets.welcomeUDP))
                {
                    _packet.Write(_toClient);
                    SendUDPData(_toClient, _packet, true);
                }
            }

            /// <summary>
            /// Reply client's TCP ping
            /// </summary>
            /// <param name="_toClient"></param>
            /// <param name="_msg"></param>
            public static void TCPPingReply(int _toClient, string _msg)
            {
                using (Packet _packet = new Packet((int)ServerPackets.pingResponseTCP))
                {
                    _packet.Write(_msg);

                    SendTCPData(_toClient, _packet, true);
                }
            }


            /// <summary>
            /// Reply client's UDP ping
            /// </summary>
            /// <param name="_toClient"></param>
            /// <param name="_msg"></param>
            public static void UDPPingReply(int _toClient, string _msg)
            {
                using (Packet _packet = new Packet((int)ServerPackets.pingResponseUDP))
                {
                    _packet.Write(_msg);

                    SendUDPData(_toClient, _packet, true);
                }
            }

            /// <summary>
            /// Inform the spawn of a player
            /// </summary>
            /// <param name="_toClient"></param>
            /// <param name="_player"></param>
            public static void SpawnPlayer(int _toClient, PlayerManager _player)
            {
                Transform _avatar = _player.body.transform;

                using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
                {
                    _packet.Write(_player.id);
                    _packet.Write(_player.username);
                    _packet.Write((int)_player.role);
                    _packet.Write(_avatar.position);
                    _packet.Write(_avatar.rotation);

                    SendTCPData(_toClient, _packet, true);
                }
            }

            public static void PlayerPosition(PlayerManager _player)
            {
                //TODO: Get rid of function GetChild
                // get the position of both VR controllers
                Transform _avatar = _player.body.transform;
                Transform _lefthand = _player.leftHandController.transform;
                Transform _righthand = _player.rightHandController.transform;

                using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
                {
                    _packet.Write(_player.id);
                    _packet.Write(_avatar.position);
                    _packet.Write(_avatar.rotation);

                    _packet.Write(_lefthand.position);
                    _packet.Write(_lefthand.rotation);
                    _packet.Write(_righthand.position);
                    _packet.Write(_righthand.rotation);

                    //Do not update VR player position based on server
                    if (_player.isVR)
                        MulticastExceptOneUDPData(_player.id, _packet, true);
                    else
                        MulticastUDPData(_packet, true);
                }
            }


            public static void PlayerDisconnect(int _playerId)
            {
                using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
                {
                    _packet.Write(_playerId);

                    // This packet is important, we cannot afford losing it.
                    MulticastTCPData(_packet, true);
                }
            }

            public static void HeartBeatDetection()
            {
                using (Packet _packet = new Packet((int)ServerPackets.heartBeatDetectionUDP))
                {
                    MulticastUDPData(_packet, true);
                }

                using (Packet _packet = new Packet((int)ServerPackets.heartBeatDetectionTCP))
                {
                    MulticastTCPData(_packet, true);
                }
            }

            public static void BroadcastItemState(GameObject _item, bool isUDP)
            {
                ItemController _itemCon = _item.GetComponent<ItemController>();
                // ServerItemManager.cs calls this method to multicase an item's position
                using (Packet _packet = new Packet((int)ServerPackets.itemState))
                {
                    _packet.Write(_itemCon.id);
                    _packet.Write(_item.transform.position);
                    _packet.Write(_item.transform.rotation);
                    _packet.Write(_itemCon.GetItemState());

                    if (isUDP)
                        MulticastExceptOneUDPData(_itemCon.ownerId, _packet);                  //Does not update data to owner
                    else
                        MulticastExceptOneTCPData(_itemCon.ownerId, _packet);
                }
            }

            public static void SendInitialItemState(int _toClient, GameObject _item)
            {
                ItemController _itemCon = _item.GetComponent<ItemController>();
                // ServerItemManager.cs calls this method to multicase an item's position
                using (Packet _packet = new Packet((int)ServerPackets.itemList))
                {
                    _packet.Write(ServerItemManager.instance.GetItemNum());     // different from BroadcastItemState
                    _packet.Write(_itemCon.id);
                    _packet.Write((int)_itemCon.toolType);                      // different from BroadcastItemState
                    //_packet.Write(_itemCon.ownerId);                          // the newly spawned user cannot hold an item. So no need to transmit this value

                    _packet.Write(_item.transform.position);
                    _packet.Write(_item.transform.rotation);
                    _packet.Write(_itemCon.GetItemState());

                    SendTCPData(_toClient, _packet);                            // different from BroadcastItemState
                }
            }


            /// <summary>
            /// Tell the current owner to pass the item's ownership to new user
            /// </summary>
            /// <param name="_toClient"></param>
            /// <param name="_itemId"></param>
            public static void ownershipDeprivation(int _toClient, int _itemId)              //Deny a clien's ownership via TCP
            {
                using (Packet _packet = new Packet((int)ServerPackets.ownershipDeprivation))
                {
                    _packet.Write(_itemId);
                    SendTCPData(_toClient, _packet);
                }
            }


            public static void SendEnvironmentState(int _fromClient, int eventId, byte[] message, bool isUnicast = false)
            {
                using (Packet _packet = new Packet((int)ServerPackets.environmentState))
                {
                    _packet.Write(eventId); // id of the environment event
                    _packet.Write(message); // message

                    if (isUnicast)
                    {
                        SendTCPData(_fromClient, _packet);
                    }
                    else
                    {
                        MulticastExceptOneTCPData(_fromClient, _packet);
                    }
                }

            }

            public static void SendVoiceChatData(int _toClient, ArraySegment<byte> _voiceData, bool _isUDP = true)
            {


                using (Packet _packet = new Packet((int)ServerPackets.voiceChatData))
                {
                    _packet.Write(_voiceData);
                    if (_isUDP)
                        SendUDPData(_toClient, _packet);
                    else
                        SendTCPData(_toClient, _packet);
                }
            }

            /// <summary>
            /// Inform other users the dissonance player id (string) of user _fromClient.
            /// </summary>
            /// <param name="_tgtClient">id of the player who would like to inform its chosen dissonance playerId</param>
            /// <param name="_playerId">Dissonance player id</param>
            /// <param name="_needMulticast">whether to multicast except the tgtClient or only unicast to the tgtClient</param>
            public static void SendVoiceChatPlayerId(int _tgtClient, string _playerId, bool _needMulticast=true)
            {
                using (Packet _packet = new Packet((int)ServerPackets.voiceChatPlayerId))
                {
                    _packet.Write(_tgtClient);
                    _packet.Write(_playerId);

                    if (_needMulticast)
                    {
                        MulticastExceptOneTCPData(_tgtClient, _packet);
                    }
                    else
                    {
                        SendTCPData(_tgtClient, _packet);
                    }
                }
            }

            #endregion

        }
    }
}