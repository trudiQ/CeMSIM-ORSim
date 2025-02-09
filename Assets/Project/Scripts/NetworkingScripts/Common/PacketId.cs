﻿using System;
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
            welcomeUDP,         // udp welcome message in response to client's UDPWelcome
            pingResponseTCP,    // server response to client's pingTCP
            pingResponseUDP,    // server response to client's pingUDP
            spawnPlayer,        // the spawn of a player (also a reply to spawn request)
            playerPosition,     // update of player position
            playerDisconnected, // inform player the disconnection of another player
            heartBeatDetectionUDP, // a packet containing the server's system time. used to check whether the target client is alive and get the round-trip-time.
            heartBeatDetectionTCP, 
            itemState,          //update of item position
            ownershipDeprivation,    //TCP packet that denies a client's request to an item's ownership
            environmentState,   // carry the environment state information (not player state)
            itemList,           // inform client the list of items in the current simulation scene
            voiceChatData,      // voice chat data 
            voiceChatPlayerId,  // inform other clients the dissonance player id
        }

        /// <summary>
        /// Client -> Server
        /// </summary>
        public enum ClientPackets
        {
            invalidPacket = 1,  // an invalid packet
            welcome,            // welcome packet sent by the client
            welcomeReceived,    // client's in response to server's welcome packet
            welcomeUDP,         // UDP welcome packet sent after welcome (TCP) packet
            pingTCP,            // ping message to the server via TCP
            pingUDP,            // ping message to the server via UDP
            spawnRequest,       // player request to enter
            playerDesktopMovement, // client's control operations on the movement of the desktop player
            playerVRMovement,   // client's position and orientation of the VR player
            heartBeatDetectionUDP,  // in response to the server's system.
            heartBeatDetectionTCP,
            itemState,     //UDP update of item position
            itemOwnershipChange, //TCP update of item's ownership
            environmentState,    // carry the environment state information (not player state)
            voiceChatData,       // voice chat data
            voiceChatPlayerId,   // inform the server (then other clients) the setting of dissonance player id
        }

    }
}