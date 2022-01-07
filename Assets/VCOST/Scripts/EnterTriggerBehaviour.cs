using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterTriggerBehaviour : MonoBehaviour
{
    public DynamicSutureBehaviour needle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Suturable")
        {
            needle.StartSuturing(other);
        }
    }
}
