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
            playerDisconnected, // inform player the disconnection of another player
            heartBeatDetectionUDP, // a packet containing the server's system time. used to check whether the target client is alive and get the round-trip-time.
            heartBeatDetectionTCP, 
            itemPositionUDP,    // UDP update of item position
            ownershipDenial,    // TCP packet that denies a client's request to an item's ownership
            environmentState,   // carry the environment state information (not player state)
            voiceChatTCP,       // reliable voice chat data 
            voiceChatUDP,       // unreliable voice chat data
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
            itemOwnershipChange, //TCP update of item's ownership
            environmentState,    // carry the environment state information (not player state)
            voiceChatTCP,       // reliable voice chat data 
            voiceChatUDP,       // unreliable voice chat data
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
                ServerPacketsInfo.Add(7, "playerDisconnected");
                ServerPacketsInfo.Add(8, "heartBeatDetectionUDP");
                ServerPacketsInfo.Add(9, "heartBeatDetectionTCP");
                ServerPacketsInfo.Add(10, "itemPositionUDP");
                ServerPacketsInfo.Add(11, "ownershipDenial");
                ServerPacketsInfo.Add(12, "environmentState");
                ServerPacketsInfo.Add(13, "voiceChatTCP");
                ServerPacketsInfo.Add(14, "voiceChatUDP");

                ClientPacketsInfo.Add(1, "invalidPacket");
                ClientPacketsInfo.Add(2, "welcome");
                ClientPacketsInfo.Add(3, "welcomeReceived");
                ClientPacketsInfo.Add(4, "pingTCP");
                ClientPacketsInfo.Add(5, "pingUDP");
                ClientPacketsInfo.Add(6, "spawnRequest");
                ClientPacketsInfo.Add(7, "playerDesktopMovement");
                ClientPacketsInfo.Add(8, "playerVRMovement");
                ClientPacketsInfo.Add(9, "heartBeatDetectionUDP");
                ClientPacketsInfo.Add(10, "heartBeatDetectionTCP");
                ClientPacketsInfo.Add(11, "itemPositionUDP");
                ClientPacketsInfo.Add(12, "itemOwnershipChange");
                ClientPacketsInfo.Add(13, "environmentState");
                ClientPacketsInfo.Add(14, "voiceChatTCP");
                ClientPacketsInfo.Add(15, "voiceChatUDP");

            }

        }

    }
}