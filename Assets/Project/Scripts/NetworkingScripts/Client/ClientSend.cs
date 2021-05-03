using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CEMSIM.GameLogic;

namespace CEMSIM
{
    namespace Network
    {
        /// <summary>
        /// This class handles sending packets
        /// </summary>
        public class ClientSend : MonoBehaviour
        {
            #region Basic TCP and UDP packet sending functions
            /// <summary>
            /// Send packet to the server via TCP
            /// </summary>
            /// <param name="_packet">The packet including only the payload</param>
            /// <param name="addTime">Whether to include the generation time of the packet</param>
            private static void SendTCPData(Packet _packet, bool addTime = false)
            {
                //_packet.WriteLength(); // add the Data Length to the packet
                _packet.WriteHeader(addTime);// 
                ClientInstance.instance.tcp.SendData(_packet);
            }

            /// <summary>
            /// Send packet to the server via UDP
            /// </summary>
            /// <param name="_packet">The packet including only the payload</param>
            /// <param name="addTime">Whether to include the generation time of the packet</param>
            private static void SendUDPData(Packet _packet, bool addTime = false)
            {
                _packet.WriteHeader(addTime);// 
                ClientInstance.instance.udp.SendData(_packet);
            }
            #endregion

            #region Generate Packets
            public static void WelcomeReceived()
            {
                using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
                {
                    _packet.Write(ClientInstance.instance.myId);
                    _packet.Write(ClientInstance.instance.myUsername);

                    SendTCPData(_packet);
                }
            }

            public static void SendTCPPing(string _msg = "")
            {
                using (Packet _packet = new Packet((int)ClientPackets.pingTCP))
                {
                    _packet.Write(_msg);

                    SendTCPData(_packet);
                }
            }

            public static void SendUDPPing(string _msg = "")
            {
                using (Packet _packet = new Packet((int)ClientPackets.pingUDP))
                {
                    _packet.Write(ClientInstance.instance.myId);
                    _packet.Write(_msg);

                    SendUDPData(_packet);
                }
            }

            public static void SendSpawnRequest(string _username, bool _vrEnabled)
            {
                using (Packet _packet = new Packet((int)ClientPackets.spawnRequest))
                {
                    _packet.Write(_username);
                    _packet.Write(_vrEnabled);

                    SendTCPData(_packet);
                }
            }

            /// <summary>
            /// Send out player's movement
            /// </summary>
            /// <param name="_inputs"></param>
            public static void PlayerDesktopMovement(bool[] _inputs)
            {
                using (Packet _packet = new Packet((int)ClientPackets.playerDesktopMovement))
                {
                    _packet.Write(_inputs.Length);
                    foreach (bool _input in _inputs)
                    {
                        _packet.Write(_input);
                    }
                    _packet.Write(GameManager.players[ClientInstance.instance.myId].transform.rotation);

                    SendUDPData(_packet);
                }
            }

            /// <summary>
            /// Send out player's movement
            /// </summary>
            /// <param name="_inputs"></param>
            public static void PlayerVRMovement()
            {
                if (GameManager.players.ContainsKey(ClientInstance.instance.myId))
                {
                    using (Packet _packet = new Packet((int)ClientPackets.playerVRMovement))
                    {
                        _packet.Write(GameManager.players[ClientInstance.instance.myId].GetComponent<PlayerVRController>().VRCamera.position);
                        _packet.Write(GameManager.players[ClientInstance.instance.myId].GetComponent<PlayerVRController>().VRCamera.rotation);

                        SendUDPData(_packet);
                    }
                }
                else
                {
                    Debug.Log($"Local Warning: Client ID {ClientInstance.instance.myId} does not exist or has not been added yet");
                }
            }

            public static void SendHeartBeatResponseTCP(long sendTicks)
            {
                using (Packet _packet = new Packet((int)ClientPackets.heartBeatDetectionTCP))
                {
                    _packet.Write(sendTicks);
                    SendTCPData(_packet);
                }
            }

            public static void SendHeartBeatResponseUDP(long sendTicks)
            {
                using (Packet _packet = new Packet((int)ClientPackets.heartBeatDetectionUDP))
                {
                    _packet.Write(sendTicks);
                    SendUDPData(_packet);
                }
            }

            public static void SendItemPosition(GameObject _item)                             //Send Item position to server via UDP
            {
                ItemController itemCon = _item.GetComponent<ItemController>();
                using (Packet _packet = new Packet((int)ClientPackets.itemPositionUDP))
                {
                    _packet.Write(itemCon.id);
                    _packet.Write(_item.transform.position);
                    SendUDPData(_packet);
                }
            }

            public static void SendItemRotation(GameObject _item)                             //Send Item rotation to server via UDP
            {
                ItemController itemCon = _item.GetComponent<ItemController>();
                using (Packet _packet = new Packet((int)ClientPackets.itemRotationUDP))
                {
                    _packet.Write(itemCon.id);
                    _packet.Write(_item.transform.rotation);
                    SendUDPData(_packet);
                }
            }

            public static void SendOnwershipChange(GameObject _item)                          //Send Item rotation to server via TCP
            {
                using (Packet _packet = new Packet((int)ClientPackets.itemOwnershipChange))
                {
                    ItemController itemCon = _item.GetComponent<ItemController>();
                    _packet.Write(itemCon.id);
                    _packet.Write(itemCon.ownerId);
                    SendTCPData(_packet);
                }

            }
            #endregion

        }
    }
}
