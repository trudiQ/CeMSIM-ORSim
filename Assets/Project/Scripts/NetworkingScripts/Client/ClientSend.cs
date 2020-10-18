using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        /// <summary>
        /// This class handles sending packets
        /// </summary>
        public class ClientSend : MonoBehaviour
        {
            private static void SendTCPData(Packet _packet)
            {
                _packet.WriteLength(); // add the Data Length to the packet
                ClientInstance.instance.tcp.SendData(_packet);
            }

            private static void SendUDPData(Packet _packet)
            {
                _packet.WriteLength();
                ClientInstance.instance.udp.SendData(_packet);
            }


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
                    Debug.Log("Warning: Client ID does not exist or has not been added yet");
                }
            }
            #endregion
        }
    }
}
