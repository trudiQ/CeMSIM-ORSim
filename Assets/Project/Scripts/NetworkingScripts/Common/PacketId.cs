using System;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        // This class contains the packet id for all supported type
        /// <summary>
        /// Server -> Client
        /// </summary>
        public enum ServerPackets
        {
            invalidPacket=1,    // an invalid packet
            welcome,            // welcome message sent in reply to client's tcp connection
            pingResponseTCP,    // server response to client's pingTCP
            pingResponseUDP,    // server response to client's pingUDP
            spawnPlayer,        // the spawn of a player (also a reply to spawn request)
            playerPosition,     // update of player position
            playerRotation,     // update of player rotation
            playerDisconnected, // inform player the disconnection of another player
            heartBeatDetectionUDP, // a packet containing the server's system time. used to check whether the target client is alive and get the round-trip-time.
            heartBeatDetectionTCP, 
            itemPositionUDP,     //UDP update of item position
            itemRotationUDP,     //UDP update of item rotation
            ownershipDenial,     //TCP packet that denies a client's request to an item's ownership
        }

        /// <summary>
        /// Client -> Server
        /// </summary>
        public enum ClientPackets
        {
            invalidPacket = 1,  // an invalid packet
            welcome,            // welcome packet sent by the client
            welcomeReceived,    // client's in response to server's welcome packet
            pingTCP,            // ping message to the server via TCP
            pingUDP,            // ping message to the server via UDP
            spawnRequest,       // player request to enter
            playerDesktopMovement, // client's control operations on the movement of the desktop player
            playerVRMovement,   // client's position and orientation of the VR player
            heartBeatDetectionUDP,  // in response to the server's system.
            heartBeatDetectionTCP,
            itemPositionUDP,     //UDP update of item position
            itemRotationUDP,     //UDP update of item rotation
            itemOwnershipChange, //TCP update of item's ownership
        }


        public class PacketId
        {
            public static Dictionary<int, string> ServerPacketsInfo = new Dictionary<int, string>();
            public static Dictionary<int, string> ClientPacketsInfo = new Dictionary<int, string>();

            /// <summary>
            /// The function is initialized when either the client (ClientInstance) and the server(ServerInstance) is initialized.
            /// </summary>
            public static void InitPacketIdDictionary()
            {
                Debug.Log("Initialize Packet Id Mappings");

                ServerPacketsInfo.Add(1, "invalidPacket");
                ServerPacketsInfo.Add(2, "welcome");
                ServerPacketsInfo.Add(3, "pingResponseTCP");
                ServerPacketsInfo.Add(4, "pingResponseUDP");
                ServerPacketsInfo.Add(5, "spawnPlayer");
                ServerPacketsInfo.Add(6, "playerPosition");
                ServerPacketsInfo.Add(7, "playerRotation");
                ServerPacketsInfo.Add(8, "playerDisconnected");
                ServerPacketsInfo.Add(9, "heartBeatDetectionUDP");
                ServerPacketsInfo.Add(10, "heartBeatDetectionTCP");
                ServerPacketsInfo.Add(11, "itemPositionUDP");
                ServerPacketsInfo.Add(12, "itemRotationUDP");
                ServerPacketsInfo.Add(13, "ownershipDenial");

                ClientPacketsInfo.Add(1, "invalidPacket");
                ClientPacketsInfo.Add(2, "welcomeReceived");
                ClientPacketsInfo.Add(3, "welcome");
                ClientPacketsInfo.Add(4, "pingTCP");
                ClientPacketsInfo.Add(5, "pingUDP");
                ClientPacketsInfo.Add(6, "spawnRequest");
                ClientPacketsInfo.Add(7, "playerDesktopMovement");
                ClientPacketsInfo.Add(8, "playerVRMovement");
                ClientPacketsInfo.Add(9, "heartBeatDetectionUDP");
                ClientPacketsInfo.Add(10, "heartBeatDetectionTCP");
                ClientPacketsInfo.Add(11, "itemPositionUDP");
                ClientPacketsInfo.Add(12, "itemRotationUDP");
                ClientPacketsInfo.Add(13, "itemOwnershipChange");
            }

        }

    }
}