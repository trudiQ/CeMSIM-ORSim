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
            decompressionNeedle,
        }
    }
}
