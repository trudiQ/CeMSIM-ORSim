using System;

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
            itemManagerTCP,     //TCP update of item status(position/rotation/id/owner)
            itemManagerUDP,     //UDP update of item status(position/rotation/id/owner)
        }

        /// <summary>
        /// Client -> Server
        /// </summary>
        public enum ClientPackets
        {
            invalidPacket = 1,  // an invalid packet
            welcomeReceived,    // client's in response to server's welcome packet
            pingTCP,            // ping message to the server via TCP
            pingUDP,            // ping message to the server via UDP
            spawnRequest,       // player request to enter
            playerDesktopMovement, // client's control operations on the movement of the desktop player
            playerVRMovement,   // client's position and orientation of the VR player
            heartBeatDetectionUDP,  // in response to the server's system.
            heartBeatDetectionTCP,
            itemManagerTCP,     //TCP update of item status(position/rotation/id/owner)
            itemManagerUDP,     //UDP update of item status(position/rotation/id/owner)
        }
    }
}