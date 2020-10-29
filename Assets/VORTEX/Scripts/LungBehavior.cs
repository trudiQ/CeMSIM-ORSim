using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LungBehavior : MonoBehaviour
{
    public Material originalMaterial;
    public Material enterMaterial;

    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Tool")
        {
            //var tool = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
            meshRenderer.material = enterMaterial;
            other.GetComponent<NeedleBehavior>().NeedleInserted(true);
            //tool.gravityOnDetach = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Tool")
        {
            //var tool = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
            meshRenderer.material = originalMaterial;
            other.GetComponent<NeedleBehavior>().NeedleInserted(false);
            //tool.gravityOnDetach = true;
        }
    }
}
