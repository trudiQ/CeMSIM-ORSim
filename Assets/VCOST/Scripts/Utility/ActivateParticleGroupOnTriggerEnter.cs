using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

namespace Utility
{
    public class ActivateParticleGroupOnTriggerEnter : MonoBehaviour
    {
        public ObiParticleAttachment particleAttachment;

        private void OnTriggerEnter(Collider other)
        {
            particleAttachment.target = other.transform;
            particleAttachment.enabled = true;
        }
    }
}
