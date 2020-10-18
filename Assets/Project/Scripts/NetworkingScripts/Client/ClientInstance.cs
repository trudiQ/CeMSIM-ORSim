using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        public class ClientInstance : MonoBehaviour
        {
            public static ClientInstance instance;
            public static int dataBufferSize = 4096;

            // server ip and port
            public string ip = ClientNetworkConstants.SERVER_IP;
            //[HideInInspector]
            //public string ip;
            public int port = ClientNetworkConstants.SERVER_PORT;

            public int myId = 0;
            public string myUsername = "DEFAULT_USERNAME";
            public TCP tcp;
            public UDP udp;

            public bool isConnected = false;
            // data structures dealing with packet fragments due to TCP streaming
            private delegate void PacketHandler(Packet _packet);
            private static Dictionary<int, PacketHandler> packetHandlers;

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


            // Start is called before the first frame update
            private void Start()
            {
                //ip = defaultIP;
                //initialize TCP and UDP connections
                Debug.Log($"Current server IP is {ip}");
                tcp = new TCP();
                udp = new UDP();

                if(GameManager.instance.localPlayerVR.activeInHierarchy)
                {
                    //To do: Handle this in XR & Menu Manager Instances
                    // disable the manu and request to enter the OR
                    ClientPCConnetMenu.Instance.gameObject.SetActive(false);
                    ClientInstance.instance.ConnectToServer(ip, port);

                    //Delays the Spawn request to ensure the client is connected
                    StartCoroutine(DelaySpawnRequest());
                }
            }


            IEnumerator DelaySpawnRequest()
            {
                yield return new WaitForSeconds(5f);
                string _username = "Player" + ClientInstance.instance.myId.ToString();

                //TO DO: ConnectOnStart is used for VR mode at the moment. 
                //Add feature for entering in VR or desktop mode
                ClientSend.SendSpawnRequest(_username, true);
                GameManager.instance.localPlayerVR.GetComponent<PlayerVRController>().enabled = true;
            }

            private void OnApplicationQuit()
            {
                // disconnect all network connections when quit the game
                Disconnect();
            }

            public void SetUsername(string _username)
            {
                instance.myUsername = _username;
            }

            public void ConnectToServer(string _ipAddress, int _port)
            {
                if(_ipAddress != "")
                    ip = _ipAddress;
                port = _port;
                InitializeClientData();
                tcp.Connect();
                udp.Connect(_port);
                isConnected = true;
            }



            /// <summary>
            /// The UDP class used by the client.
            /// </summary>
            public class UDP
            {
                public UdpClient socket;
                public IPEndPoint endPoint;

                public UDP()
                {
                        
                }

                /// <summary>
                /// we open a local port for UDP communication.
                /// </summary>
                /// <param name="_localPort">The local port we open for UDP</param>
                public void Connect(int _localPort)
                {
                    IPAddress _ip;
                    // convert hostname to corresponding ip
                    if (!System.Net.IPAddress.TryParse(instance.ip, out _ip))
                    {
                        // ip is actually a hostname
                        _ip = Dns.GetHostAddresses(instance.ip)[0];
                    }

                    Debug.Log($"UDP is connecting to the server with ip:{instance.ip} {_ip}");
                    endPoint = new IPEndPoint(_ip, instance.port);

                    socket = new UdpClient(_localPort);
                    socket.Connect(endPoint);

                    socket.BeginReceive(ReceiveCallback, null);

                    // send a welcome packet
                    using (Packet _packet = new Packet())
                    {
                        // since user id has already been added to the packet, no need to manually add it again.
                        SendData(_packet);
                    }
                }


                public void SendData(Packet _packet)
                {
                    try
                    {
                        // Since UDP socket won't bind a user to a session,
                        // we manually add user id to the packet to let the server identify
                        // But there is a potential that there is a security issue here.
                        //
                        _packet.InsertInt32(instance.myId);

                        if (socket != null)
                        {
                            socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                        }
                        else
                        {

                        }

                    }
                    catch (Exception _e)
                    {
                        Debug.Log($"UDP socket is unable to send the packet. Disconnected. Exception {_e}");
                        // disconnect
                    }
                }

                private void ReceiveCallback(IAsyncResult _result)
                {
                    try
                    {
                        byte[] _data = socket.EndReceive(_result, ref endPoint);

                        socket.BeginReceive(ReceiveCallback, null);


                        /// Here, we discard the packet if its packet length is smaller than
                        /// the 4-byte packet length (int32). However, UDP is less reliable
                        /// than TCP, it's possible that the network lost the first half
                        /// of the packet and we received the second half. For UDP, we are expected
                        /// to accept lossing packets. And we may need to verify the packet intactivity
                        /// ourselves.
                        if (_data.Length < 4)
                        {
                            // discard the packet
                            instance.Disconnect();
                            return;
                        }


                        HandleData(_data);


                    }
                    catch (Exception _e)
                    {
                        Debug.Log($"UDP socket encountered an error and is diconnected. Exception {_e}");
                        // disconnect
                        Disconnect();
                    }
                }

                private void HandleData(byte[] _data)
                {
                    using (Packet _packet = new Packet(_data))
                    {
                        int _packetLength = _packet.ReadInt32();
                        _data = _packet.ReadBytes(_packetLength);
                    }

                    ClientThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_data))
                        {
                            int _packetId = _packet.ReadInt32();
                            packetHandlers[_packetId](_packet);
                        }
                    });


                }

                private void Disconnect()
                {
                    instance.Disconnect();
                    endPoint = null;
                    socket = null;
                }

            }

            /// <summary>
            /// The TCP class used by the client.
            /// </summary>
            public class TCP
            {
                public TcpClient socket;
                public NetworkStream stream;
                private Packet receivedData;
                private byte[] receiveBuffer;

                public void Connect()
                {
                    socket = new TcpClient
                    {
                        ReceiveBufferSize = ClientNetworkConstants.DATA_BUFFER_SIZE,
                        SendBufferSize = ClientNetworkConstants.DATA_BUFFER_SIZE
                    };

                    receiveBuffer = new byte[ClientNetworkConstants.DATA_BUFFER_SIZE];
                    Debug.Log($"TCP is connecting to the server with ip:{instance.ip}");

                    socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);

                }

                private void ConnectCallback(IAsyncResult _result)
                {
                    socket.EndConnect(_result);

                    if (!socket.Connected)
                    {
                        return;
                    }

                    stream = socket.GetStream();
                    stream.BeginRead(receiveBuffer, 0, ClientNetworkConstants.DATA_BUFFER_SIZE, ReceiveCallback, null);

                    receivedData = new Packet(); // create an empty packet to parse incoming packets


                }

                private void ReceiveCallback(IAsyncResult _result)
                {
                    try
                    {
                        int _byteLength = stream.EndRead(_result);

                        // disconnect if no response
                        if (_byteLength <= 0)
                        {
                            instance.Disconnect();
                            return;
                        }
                        // start to process the data
                        byte[] _data = new byte[_byteLength];
                        Array.Copy(receiveBuffer, _data, _byteLength); ///> copy the received data to the temporary data buffer


                        // process data
                        receivedData.Reset(HandleData(_data));

                        // prepare for the next data
                        stream.BeginRead(receiveBuffer, 0, ClientNetworkConstants.DATA_BUFFER_SIZE, ReceiveCallback, null);

                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Server exception {e}");
                        Disconnect();
                        // disconnect
                    }
                }

                public void SendData(Packet _packet)
                {
                    try
                    {
                        if (socket != null)
                        {
                            stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error sending packet to Server via TCP. Exception {e}");
                    }
                }

                /// <summary>
                /// This function indicates whether this packet is an intact packet or a combination of multiple packets.
                /// Return "true" when finish digesting the last packet of a complete packet, and
                /// indicate the Packet class to Reset the packet space.
                /// Return "false" when if still waiting for further sub-packets.
                /// </summary>
                /// <param name="_data"></param>
                /// <returns>Whether to reset the packet space</returns>
                private bool HandleData(byte[] _data)
                {
                    int _packetLength = 0;
                    receivedData.SetBytes(_data); // append _data to packet            

                    // Since the first segment is the data length, an int32 of size 4,
                    // by checking whether the unread length >= 4, we know whether this
                    // packet is a split packet of a big one or a standalone packet.
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt32();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }

                    // After reading a packet based on the _packetLength, we finish processing
                    // a packet. However, if the received data contains more than one packet,
                    // we need to begin processing the next one. If the number of UnreadLength() is greater
                    // than the current packet's length, there is another packet squeezed in current data payload.
                    // Of course, we don't need to process a zero-data packet.
                    while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                    {
                        byte[] _packetBytes = receivedData.ReadBytes(_packetLength);


                        ClientThreadManager.ExecuteOnMainThread(() =>
                        {
                            // create a packet containing just the data
                            using (Packet _packet = new Packet(_packetBytes))
                            {
                                int _packetId = _packet.ReadInt32();

                                Debug.Log($"Receive a packet with id {_packetId}");
                                // call proper handling function based on packet id
                                packetHandlers[_packetId](_packet);
                            }
                        });

                        _packetLength = 0;

                        if (receivedData.UnreadLength() >= 4)
                        {
                            _packetLength = receivedData.ReadInt32();
                            if (_packetLength <= 0)
                            {
                                return true;
                            }
                        }

                    }

                    if (_packetLength <= 1)
                    {
                        return true;
                    }
                    return false;
                }

                /// <summary>
                /// Disconnect tcp connection and free resources.
                /// </summary>
                /// <returns></returns>
                private void Disconnect()
                {
                    instance.Disconnect();

                    stream = null;
                    receivedData = null;
                    receiveBuffer = null;
                    socket = null;
                }
            }

            private static void InitializeClientData()
            {
                packetHandlers = new Dictionary<int, PacketHandler>() {
                { (int)ServerPackets.welcome, ClientHandle.Welcome },
                { (int)ServerPackets.pingResponseTCP, ClientHandle.TCPPingResponse },
                { (int)ServerPackets.pingResponseUDP, ClientHandle.UDPPingResponse },
                { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
                { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition},
                { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation},
                { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected},
            };

                Debug.Log("Client Data Initialization Complete.");

            }

            private void Disconnect()
            {
                if (isConnected)
                {
                    isConnected = false;
                    tcp.socket.Close();
                    if (udp.socket != null)
                        udp.socket.Close();

                    Debug.Log("Disconnect from server.");
                }
            }
        }
    }
}