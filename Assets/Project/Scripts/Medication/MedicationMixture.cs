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
        public class MedicineMixture
        {
            public Dictionary<Medicine, float> mixture; // type of medication and its volume

            public float volume = 0; // total volume of all medications

            public MedicineMixture()
            {
                volume = 0;
                mixture = new Dictionary<Medicine, float>();
            }

            public MedicineMixture(Dictionary<Medicine, float> _mixture)
            {
                mixture = new Dictionary<Medicine, float>(_mixture);
                foreach (KeyValuePair<Medicine, float> kvp in mixture)
                {
                    volume += kvp.Value;
                }
            }

            public MedicineMixture(Dictionary<Medicine, float> _mixture, float _volume)
            {
                mixture = new Dictionary<Medicine, float>(_mixture);
                volume = _volume;
            }

            /// <summary>
            /// Mix this medicine mixture with another medicine mixture.
            /// </summary>
            /// <param name="anotherMedicine"></param>
            public void Mix(MedicineMixture anotherMedicine)
            {
                foreach (KeyValuePair<Medicine, float> kvp in anotherMedicine.mixture)
                {
                    if (mixture.ContainsKey(kvp.Key))
                        mixture[kvp.Key] += kvp.Value;
                    else
                        mixture[kvp.Key] = kvp.Value;
                    volume += kvp.Value;
                }
            }

            public void Mix(Medicine medicine, float _volume)
            {
                if (mixture.ContainsKey(medicine))
                    mixture[medicine] += _volume;
                else
                    mixture[medicine] = _volume;
                volume += _volume;
            }

            /// <summary>
            /// Separate tgtVolume amount of medicine mixture out
            /// </summary>
            /// <param name="volume"></param>
            public MedicineMixture Split(float tgtVolume)
            {
                tgtVolume = Mathf.Min(tgtVolume, volume);

                Dictionary<Medicine, float> tgtMixture = new Dictionary<Medicine, float>(); // the mixture to be split out
                float componentVolumn = 0;
                foreach (KeyValuePair<Medicine, float> kvp in mixture)
                {
                    componentVolumn = kvp.Value / volume * tgtVolume;

                    mixture[kvp.Key] -= componentVolumn;
                    tgtMixture[kvp.Key] = componentVolumn;
                }
                // we cannot modify volume inside the for-loop, because it is used in the calculation of componentVolume
                volume -= tgtVolume;
                return new MedicineMixture(tgtMixture, tgtVolume);
            }

        }
    }
}