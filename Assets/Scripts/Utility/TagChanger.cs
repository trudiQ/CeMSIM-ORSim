using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class TagChanger : MonoBehaviour
    {
        public void ChangeTag(string s)
        {
            transform.tag = s;
        }
    }
}
