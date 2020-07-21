using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
            _packet.Write("This is a welcome reply from Client");

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



    #endregion
}
