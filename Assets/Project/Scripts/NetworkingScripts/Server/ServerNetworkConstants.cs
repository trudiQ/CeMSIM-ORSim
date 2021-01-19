using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        public class ServerNetworkConstants
        {
            // Network Configuration
            public const int TCP_PORT = 54321;
            public const int CONCURRENT_CLIENTS = 40;

            public const int DATA_BUFFER_SIZE = 4096; //4KB for both Tx & Rx
        }
    }
}
