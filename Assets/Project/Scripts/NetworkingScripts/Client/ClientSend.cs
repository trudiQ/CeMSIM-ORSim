using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        namespace Client
        {
            /// <summary>
            /// This class handles sending packets
            /// </summary>
            public class ClientSend : MonoBehaviour
            {
                private static void SendTCPData(Packet _packet)
                {
                    _packet.WriteLength(); // add the Data Length to the packet
                    Client.instance.tcp.SendData(_packet);
                }

                private static void SendUDPData(Packet _packet)
                {
                    _packet.WriteLength();
                    Client.instance.udp.SendData(_packet);
                }


                #region Generate Packets
                public static void WelcomeReceived()
                {
                    using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
                    {
                        _packet.Write(Client.instance.myId);
                        _packet.Write($"This is a welcome reply from Client {Client.instance.myId}");

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
                        _packet.Write(Client.instance.myId);
                        _packet.Write(_msg);

                        SendUDPData(_packet);
                    }
                }

                public static void SendSpawnRequest(string _username, bool _vrEnabled)
                //public static void SendSpawnRequest(string _username)
                {
                    using (Packet _packet = new Packet((int)ClientPackets.spawnRequest))
                    {
                        _packet.Write(_username);
                        _packet.Write(_vrEnabled);

                        Debug.Log("Sending Delay Spawn Request");
                        SendTCPData(_packet);
                    }
                }

                /// <summary>
                /// Send out player's movement
                /// </summary>
                /// <param name="_inputs"></param>
                public static void PlayerMovement(bool[] _inputs)
                {
                    using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
                    {
                        _packet.Write(_inputs.Length);
                        foreach (bool _input in _inputs)
                        {
                            _packet.Write(_input);
                        }
                        _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

                        SendUDPData(_packet);
                    }
                }

                public static void PlayerVRMovement(Vector3 _position, Quaternion _rotation)
                {
                    using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
                    {
                        _packet.Write(_position);
                        _packet.Write(_rotation);

                        SendUDPData(_packet);
                    }
                }
                #endregion
            }
        }
    }
}