using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class ActivateGameObjectBehavior : MonoBehaviour
    {
        public List<GameObject> objectsToToggle;

        public void ToggleActive()
        {
            foreach (GameObject g in objectsToToggle)
                g.SetActive(!g.activeSelf);
        }
    }
}
