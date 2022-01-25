using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Medication
    {
        public class Medication : MonoBehaviour
        {
            public enum Drugs : int
            {
                empty,
                Epinephrine,
                Succinylcholine,
                Propofol,
                Rocuronium
            }

            public Drugs medication;
        }
    }
}