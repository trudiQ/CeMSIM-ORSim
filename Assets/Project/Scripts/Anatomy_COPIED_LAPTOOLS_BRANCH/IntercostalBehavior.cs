using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IntercostalSpace: int
{
    ICS1,
    ICS2
}

public class IntercostalBehavior : MonoBehaviour
{
    public LungBehavior lung;
    public IntercostalSpace iCS;
    public bool inSpace = false;

    [Header("Debugging")]
    public bool debugging;
    public Material defaultMaterial;
    public Material enterMaterial;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();

        if (debugging)
        {
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tool")
        {
            if(!inSpace)
            {
                inSpace = true;
                meshRenderer.material = enterMaterial;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Tool")
        {
            if (!lung.inLung)
            {
                inSpace = false;
                meshRenderer.material = defaultMaterial;
            }
        }
    }
}
