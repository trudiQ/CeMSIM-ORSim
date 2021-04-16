using UnityEngine;
using UnityEngine.SceneManagement;

namespace HurricaneVR.Framework.Utils
{
    public class HVRSetAllRBInterpolate : MonoBehaviour
    {
        public void Awake()
        {
            foreach (var rb in FindObjectsOfType<Rigidbody>())
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
    }
}