using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace GameLogic
    {
        //
        public enum Roles
        {
            surgeon=0,
            anesthesiologist,
            scrubbingNurse,
            circulatingNurse,
            Clinician,
            Other,
        }

        public enum ToolType
        {
            simpleTool=0,       // a tool doesn't have any difformation behavior
            scalpel,
            catheter,
            N95Mask,
            boufant,
            visor,
            shoeCover,
            gown,
            glove,
            syringe,
        }
        public class GameConstants
        {
            public const int SINGLE_PLAYER_CLIENTID = -1; // the client id used to represent the single player mode
        }
    }
}
