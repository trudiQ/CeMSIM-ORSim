using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pulse.CDM;

public class LungBehavior : MonoBehaviour
{
    public Material originalMaterial;
    public Material enterMaterial;
    public PulseEventManager eventManager;

    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Tool")
        {
            meshRenderer.material = enterMaterial;
            other.GetComponent<NeedleBehavior>().NeedleInserted(true);
            eventManager.TriggerPulseAction(Pulse.CDM.PulseAction.NeedleDecompressions);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Tool")
        {
            meshRenderer.material = originalMaterial;
            other.GetComponent<NeedleBehavior>().NeedleInserted(false);
        }
    }
}
