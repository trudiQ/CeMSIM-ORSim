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

        // connect udp
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    /// <summary>
    /// Handle the TCP ping response from the server.
    /// </summary>
    /// <param name="_packet"></param>
    public static void TCPPingResponse(Packet _packet)
    {
        string _msg = _packet.ReadString();

        UIManager.instance.serverMsgField.text = "TCP:" + _msg;

    }

    /// <summary>
    /// Handle the UDP ping response from the server
    /// </summary>
    /// <param name="_packet"></param>
    public static void UDPPingResponse(Packet _packet)
    {
        string _msg = _packet.ReadString();

        UIManager.instance.serverMsgField.text = "UDP:" + _msg;
    }

}
