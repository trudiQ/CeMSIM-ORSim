using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.GameLogic;

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
            public static void SpawnPlayer(int _toClient, ServerPlayer _player)
            {
                using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
                {
                    _packet.Write(_player.id);
                    _packet.Write(_player.username);
                    _packet.Write(_player.transform.position);
                    _packet.Write(_player.transform.rotation);

                    SendTCPData(_toClient, _packet, true);
                }
            }

            public static void PlayerPosition(ServerPlayer _player)
            {
                using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
                {
                    _packet.Write(_player.id);
                    _packet.Write(_player.transform.position);

                    //Do not update VR player position based on server
                    if (_player is ServerPlayerVR)
                        MulticastExceptOneUDPData(_player.id, _packet, true);
                    else
                        MulticastUDPData(_packet, true);
                }
            }

            public static void PlayerRotation(ServerPlayer _player)
            {
                using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
                {
                    _packet.Write(_player.id);
                    _packet.Write(_player.transform.rotation);

                    // no need to force update user's rotation
                    MulticastExceptOneUDPData(_player.id, _packet, true);
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
        }
    }
}