using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Shared.HandPoser;

namespace CEMSIM
{
    public class SyncronizeHand : MonoBehaviour
    {
        public List<Transform> colliderTransforms;
        public List<Transform> visualTransforms;

        // Update is called once per frame
        void Update()
        {
            SynchronizeHands();
        }

        /// <summary>
        /// Synchronizes visual hand rotations with invisible hand rotations
        /// </summary>
        public void SynchronizeHands()
        {
            int i = 0;
            foreach(Transform t in colliderTransforms)
            {
                visualTransforms[i].rotation = t.rotation;
                i++;
            }
        }
    }
}