using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CEMSIM
{
    namespace Medications
    {
        /// <summary>
        /// A enumeration of all possible medicine.
        /// </summary>
        public enum Medications
        {
            empty,              // empty
            air,                // not empty, but with air
            distilledWater,
            saline_09,          // 0.9% saline, aka normal saline
        }
    }
}