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
        }

        public enum ToolCategory
        {
            scalpel=0,
            decompressionNeedle=1,
        }
    }
}
