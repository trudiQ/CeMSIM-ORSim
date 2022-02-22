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

        //https://forum.unity.com/threads/how-do-i-find-the-closest-point-on-a-line.340058/
        public static Vector3 NearestPointOnLine(Vector3 pointOnLine, Vector3 lineDirection, Vector3 pointToFindFor)
        {
            lineDirection.Normalize();//this needs to be a unit vector
            var v = pointToFindFor - pointOnLine;
            var d = Vector3.Dot(v, lineDirection);
            return pointOnLine + lineDirection * d;
        }
    }
}
