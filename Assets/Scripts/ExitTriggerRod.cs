using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTriggerRod : MonoBehaviour
{
    public RodSutureBehaviour needle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Suturable")
        {
            needle.StopSuturing(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Suturable")
        {
            needle.CreateNewSutureLine(other, transform);
        }
    }
}
