using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility
{
    public class ChangeCamera : MonoBehaviour
    {
        public Camera camera;

        private static List<Camera> cameras = null;

        private void Start()
        {
            if (cameras != null) return;

            cameras = FindObjectsOfType<Camera>().ToList();
        }

        public void ChangeCameraToSelected()
        {
            foreach (Camera c in cameras)
            {
                c.enabled = false;
            }
            camera.enabled = true;
        }
    }
}
