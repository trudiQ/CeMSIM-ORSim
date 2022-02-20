using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public static class MathHelper
    {
        public static float Map(float oldValue, float oldBottom, float oldTop, float newBottom, float newTop)
        {
            if (oldTop - oldBottom == 0)
            {
                Debug.Log("Error in mapping value - invalid range");
                return -1f;
            }

            return (oldValue - oldBottom) / (oldTop - oldBottom) * (newTop - newBottom) + newBottom;
        }

        public static float Map01(float oldValue, float oldBottom, float oldTop)
        {
            if (oldTop - oldBottom == 0)
            {
                Debug.Log("Error in mapping value - invalid range");
                return -1f;
            }

            return (oldValue - oldBottom) / (oldTop - oldBottom);
        }
    }
}
