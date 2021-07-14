using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Sirenix.OdinInspector;
using System.Linq;

/// <summary>
/// Manage the NI DAQmx data sending from LabView through UDP connection
/// 
/// Some code are from https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/
/// </summary>
public class DAQmxLabViewUDPmanager : MonoBehaviour
{
    // receiving Thread
    Thread receiveThread;

    // udpclient object
    UdpClient client;

    // public
    // public string IP = "127.0.0.1"; default local
    public int port; // define > init

    // infos
    public string lastReceivedUDPPacket = "";

    public static List<float> readings; // Readings from LabView UDP connection, ordered by user's LabView .vi design
    public List<float> readingsObserver; // Show the readings

    // start from shell
    private static void Main()
    {
        DAQmxLabViewUDPmanager receiveObj = new DAQmxLabViewUDPmanager();
        receiveObj.init();

        string text = "";
        do
        {
            text = Console.ReadLine();
        }
        while (!text.Equals("exit"));
    }
    // start from unity3d
    public void Start()
    {
        init();

        readings = new List<float>();
        readingsObserver = new List<float>();
    }

    // OnGUI
    //void OnGUI()
    //{
    //    Rect rectObj = new Rect(40, 10, 200, 400);
    //    GUIStyle style = new GUIStyle();
    //    style.alignment = TextAnchor.UpperLeft;
    //    GUI.Box(rectObj, "# UDPReceive\n127.0.0.1 " + port + " #\n"
    //                + "shell> nc -u 127.0.0.1 : " + port + " \n"
    //                + "\nLast Packet: \n" + lastReceivedUDPPacket
    //                + "\n\nAll Messages: \n", style);
    //}

    // init
    private void init()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("UDPSend.init()");

        // define port
        port = 61557;

        // status
        print("Sending to 127.0.0.1 : " + port);
        print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");

        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }

    // receive thread
    private void ReceiveData()
    {

        client = new UdpClient(port);
        while (true)
        {

            try
            {
                // Bytes empfangen.
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                string text = Encoding.UTF8.GetString(data);

                // Den abgerufenen Text anzeigen.
                print(">> " + text);

                // latest UDPpacket
                lastReceivedUDPPacket = text;

                // Parse data
                readings.Clear();
                string[] parsedData = lastReceivedUDPPacket.Split('_');
                foreach (string d in parsedData)
                {
                    readings.Add(float.Parse(d));
                }
                readingsObserver = readings;
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // getLatestUDPPacket
    // cleans up the rest
    public string getLatestUDPPacket()
    {
        return lastReceivedUDPPacket;
    }

    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }

    void OnApplicationQuit()
    {

        if (receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        client.Close();
    }
}
