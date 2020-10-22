using System;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class ServerGameConstants
        {
                // Server Update Rate
                public const int TICKS_PER_SECOND = 30;
                public const int MS_PER_TICK = 1000 / TICKS_PER_SECOND; // milliumsecond per tick

                // Simulation Related Constants (in second)
                public const float MOVE_SPEED_PER_SECOND = 5f;
                public const float GRAVITY = -9.81f * 2; // x2 just to make the jump more interesting.
                public const float JUMP_SPEED_PER_SECOND = 9f;
        }
    }
}