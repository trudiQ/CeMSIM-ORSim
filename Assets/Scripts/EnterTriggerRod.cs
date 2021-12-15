using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterTriggerRod : MonoBehaviour
{
    public RodSutureBehaviour needle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Suturable")
        {
            needle.StartSuturing(other);
        }
    }
}
