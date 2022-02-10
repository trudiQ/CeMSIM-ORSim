using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CEMSIM.Network;
using CEMSIM.Tools;
using CEMSIM.Logger;
using UnityEngine;

namespace CEMSIM
{
    namespace Medication
    {
        /// <summary>
        /// This class handles the mixing of multiple medicine, add/remove a certain quantity/volume
        /// Current implementation use a dictionary to store contained drug and its volume.
        /// If the type of drugs won't be too much, using an array of # of drugs may be more efficient.
        /// </summary>
        public class MedicineMixture : NetworkStateInterface, LoggingEventInterface
        {
            public Dictionary<Medication.Drugs, float> mixture; // type of medication and its volume

            public float volume = 0; // total volume of all medications

            // state tracker
            public Dictionary<Medication.Drugs, float> delta; // the changes of each drug since last synchronization

            public MedicineMixture()
            {
                volume = 0;
                mixture = new Dictionary<Medication.Drugs, float>();
                delta = new Dictionary<Medication.Drugs, float>();
            }

            public MedicineMixture(Medication.Drugs _drug, float _volume)
            {
                mixture = new Dictionary<Medication.Drugs, float>();
                delta = new Dictionary<Medication.Drugs, float>();
                mixture[_drug] = _volume;
                delta[_drug] = _volume;
                volume = _volume;
            }

            public MedicineMixture(Dictionary<Medication.Drugs, float> _mixture)
            {
                mixture = new Dictionary<Medication.Drugs, float>(_mixture);
                delta = new Dictionary<Medication.Drugs, float>(_mixture);
                foreach (KeyValuePair<Medication.Drugs, float> kvp in mixture)
                {
                    volume += kvp.Value;
                }
            }

            public MedicineMixture(Dictionary<Medication.Drugs, float> _mixture, float _volume)
            {
                mixture = new Dictionary<Medication.Drugs, float>(_mixture);
                delta = new Dictionary<Medication.Drugs, float>(_mixture);
                volume = _volume;
            }

            public bool FromPacketPayload(Packet _remainderPacket)
            {
                bool isChanges = false;
                bool isDelta = _remainderPacket.ReadBool(); // whether this packet stores the delta of changes, or a full state broadcast
                int nComponents = _remainderPacket.ReadInt32();
                Medication.Drugs drug = Medication.Drugs.empty;
                float drugVolume = 0f;
                HashSet<Medication.Drugs> missingDrug = new HashSet<Medication.Drugs>(mixture.Keys);
                for (int i=0; i<nComponents; i++)
                {
                    drug = (Medication.Drugs)_remainderPacket.ReadInt32();
                    drugVolume = _remainderPacket.ReadFloat();
                    if (isDelta)
                    {
                        // apply the changes of volume
                        if (drugVolume > 0)
                        {
                            Mix(drug, drugVolume);
                            isChanges = true;
                        }
                        else
                        {
                            Remove(drug, -drugVolume);
                            isChanges = true;
                        }
                    }
                    else
                    {
                        // update the volume
                        isChanges |= UpdateVolume(drug, drugVolume);
                        missingDrug.Remove(drug);
                    }
                }

                if (!isDelta)
                {
                    if (missingDrug.Count > 0)
                    {
                        isChanges = true;
                        foreach (Medication.Drugs missingdrug in missingDrug)
                        {
                            Remove(drug);
                        }
                    }
                }
                return isChanges;
            }

            /// <summary>
            /// Serialize the current instance to byte array
            /// </summary>
            /// <param name="sendDelta">True: send the delta of mixture rather than all values</param>
            /// <returns></returns>
            public byte[] ToPacketPayload(bool sendDelta)
            {
                Debug.Log("in medicine class");
                List<byte> message = new List<byte>();
                
                message.AddRange(BitConverter.GetBytes(sendDelta));
                Dictionary<Medication.Drugs, float> component;

                if (sendDelta)
                    component = delta;
                else
                    component = mixture;
              
                message.AddRange(BitConverter.GetBytes(component.Count));
                foreach (KeyValuePair<Medication.Drugs, float> kvp in component)
                {
                    message.AddRange(BitConverter.GetBytes((int)kvp.Key));      // drug
                    message.AddRange(BitConverter.GetBytes((float)kvp.Value));  // volume
                }

                if (sendDelta)
                {
                    // which implementation is faster???
                    //delta.Clear();

                    // I didn't know that c# does not support using an iterator to change value. We must change it to a list (by copying it)
                    foreach (Medication.Drugs drug in delta.Keys.ToList()) 
                    {
                        delta[drug] = 0;
                    }
                }

                return message.ToArray();
            }

            /// <summary>
            /// By default, this function returns the delta changes of mixture
            /// </summary>
            /// <returns></returns>
            public byte[] ToPacketPayload()
            {
                return ToPacketPayload(true);
            }

            /// <summary>
            /// Mix this medicine mixture with another medicine mixture.
            /// </summary>
            /// <param name="anotherMedicine"></param>
            public void Mix(MedicineMixture anotherMedicine)
            {
                foreach (KeyValuePair<Medication.Drugs, float> kvp in anotherMedicine.mixture)
                {
                    Mix(kvp.Key, kvp.Value);
                }
            }

            public void Mix(Medication.Drugs drug, float _volume)
            {
                if (mixture.ContainsKey(drug))
                    mixture[drug] += _volume;
                else
                    mixture[drug] = _volume;
                volume += _volume;

                // update delta
                if (delta.ContainsKey(drug))
                    delta[drug] += _volume;
                else
                    delta[drug] = _volume;
            }

            /// <summary>
            /// Remove a certain amount of such drug from the mixture
            /// </summary>
            /// <param name="drug"></param>
            /// <param name="_volume"></param>
            public void Remove(Medication.Drugs drug, float _volume)
            {
                if (mixture.ContainsKey(drug))
                {
                    if(_volume >= mixture[drug]) // use up
                    {
                        _volume = mixture[drug];
                        mixture.Remove(drug);
                    }
                    else
                    {
                        mixture[drug] -= _volume;
                    }

                    volume -= _volume;


                    if (delta.ContainsKey(drug))
                        delta[drug] -= _volume;
                    else
                        delta[drug] = -_volume;
                }

            }

            /// <summary>
            /// Completely remove such drug from the mixture
            /// </summary>
            /// <param name="drug"></param>
            public void Remove(Medication.Drugs drug)
            {
                if (mixture.ContainsKey(drug))
                {
                    volume -= mixture[drug];

                    if (delta.ContainsKey(drug))
                        delta[drug] -= mixture[drug];
                    else
                        delta[drug] = -mixture[drug];

                    mixture.Remove(drug);
                }

            }

            /// <summary>
            /// Update the volume of each component
            /// </summary>
            /// <param name="drug"></param>
            /// <param name="_volume"></param>
            /// <returns>Whether there are any changes</returns>
            public bool UpdateVolume(Medication.Drugs drug, float _volume)
            {
                if (mixture.ContainsKey(drug))
                {
                    volume += _volume - mixture[drug];

                    if (delta.ContainsKey(drug))
                        delta[drug] += _volume - mixture[drug];
                    else
                        delta[drug] = _volume - mixture[drug];
                }
                else
                {
                    volume += mixture[drug];

                    if (delta.ContainsKey(drug))
                        delta[drug] += _volume;
                    else
                        delta[drug] = _volume;
                }
                mixture[drug] = _volume;

                return delta[drug] != 0f;
            }


            /// <summary>
            /// Separate tgtVolume amount of medicine mixture out
            /// </summary>
            /// <param name="volume"></param>
            public MedicineMixture Split(float tgtVolume)
            {
                tgtVolume = Mathf.Min(tgtVolume, volume);

                Dictionary<Medication.Drugs, float> tgtMixture = new Dictionary<Medication.Drugs, float>(); // the mixture to be split out
                float componentVolumn = 0;
                foreach (Medication.Drugs drug in mixture.Keys.ToList())
                {
                    componentVolumn = mixture[drug] / volume * tgtVolume;

                    mixture[drug] -= componentVolumn;
                    tgtMixture[drug] = componentVolumn;

                    if (delta.ContainsKey(drug))
                        delta[drug] -= componentVolumn;
                    else
                        delta[drug] = -componentVolumn;
                }

                // we cannot modify volume inside the for-loop, because it is used in the calculation of componentVolume
                volume -= tgtVolume;
                return new MedicineMixture(tgtMixture, tgtVolume);
            }

            public string ToCSV()
            {
                return "";
            }

            public string ToJson()
            {
                string msg = "";
                foreach (KeyValuePair<Medication.Drugs, float> kvp in mixture)
                {
                    msg += BaseEvent.JsonAddElement(kvp.Key.ToString(), kvp.Value.ToString());
                }

                return msg;
            }
        }
    }
}