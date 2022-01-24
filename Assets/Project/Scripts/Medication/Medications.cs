using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CEMSIM
{
    namespace Medications
    {
        /// <summary>
        /// A enumeration of all possible medications.
        /// </summary>
        public enum Medicine
        {
            empty,              // empty, nothing, even no air. 
            air,                // air
            distilledWater,     //
            saline_09,          // 0.9% saline, aka normal saline
        }
    }
}