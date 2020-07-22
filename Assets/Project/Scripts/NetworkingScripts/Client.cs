using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    // server ip and port
    public string ip = Constants.SERVER_IP;
    public int port = Constants.SERVER_PORT;

    public int myId = 0;
    public TCP tcp;
    public UDP udp;

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
    void Start()
    {
        //initialize TCP and UDP connections
        tcp = new TCP();
        udp = new UDP();
    }


    public void ConnectedToServer()
    {
        InitializeClientData();
        tcp.Connect();
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
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        /// <summary>
        /// we open a local port for UDP communication.
        /// </summary>
        /// <param name="_localPort">The local port we open for UDP</param>
        public void Connect(int _localPort)
        {
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
                    return;
                }


                HandleData(_data);


            }
            catch (Exception _e)
            {
                Debug.Log($"UDP socket encountered an error and is diconnected. Exception {_e}");
                // disconnect
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt32();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt32();
                    packetHandlers[_packetId](_packet);
                }
            });


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
                ReceiveBufferSize = Client.dataBufferSize,
                SendBufferSize = Client.dataBufferSize
            };

            receiveBuffer = new byte[Client.dataBufferSize];
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
            stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);

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
                    // disconnect
                    return;
                }
                // start to process the data
                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength); ///> copy the received data to the temporary data buffer


                // process data
                receivedData.Reset(HandleData(_data));

                // prepare for the next data
                stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);

            }
            catch (Exception e)
            {
                Debug.Log($"Server exception {e}");

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


                ThreadManager.ExecuteOnMainThread(() =>
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

        };

        Debug.Log("Client Data Initialization Complete.");

    }
}
