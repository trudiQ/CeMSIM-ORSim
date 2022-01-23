using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Medications
    {
        /// <summary>
        /// This class handles the mixing of multiple medicine, add/remove a certain quantity/volume
        /// </summary>
        public class MedicationMixture
        {
            private Dictionary<Medications, float> mixture = new Dictionary<Medications, float>(); // type of medication and its volume
            private float volume = 0; // total volume of all medications

            public MedicationMixture() { }
        }
    }
}