using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/// <summary>
/// This class process packets received from the server
/// </summary>
public class ClientHandle : MonoBehaviour
{

    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myid = _packet.ReadInt32();

        Debug.Log($"Message from Server:{_msg} My Id:{_myid}");

        // set the client id based on received packet
        Client.instance.myId = _myid;

        // send response
        ClientSend.WelcomeReceived();

        // connect TCP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

}
