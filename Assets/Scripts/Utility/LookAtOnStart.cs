using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class LookAtOnStart : MonoBehaviour
    {
        public Transform lookAt;
        void Start()
        {
            transform.LookAt(lookAt, Vector3.up);
        }
    }
}
