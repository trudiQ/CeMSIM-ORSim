using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NeedleBehavior : MonoBehaviour
{
    XRGrabInteractable _xrInteractable;
    Rigidbody _rb;

    // Start is called before the first frame update
    void Start()
    {
        _xrInteractable = this.GetComponent<XRGrabInteractable>();
        _rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NeedleInserted(bool isInserted)
    {
        if(isInserted)
        {
            _xrInteractable.gravityOnDetach = false;
            //_rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            _xrInteractable.gravityOnDetach = true;
            //_rb.constraints = RigidbodyConstraints.None;
        }
    }
}
