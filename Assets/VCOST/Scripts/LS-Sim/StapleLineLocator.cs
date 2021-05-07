using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StapleLineLocator : MonoBehaviour
{


    public List<Ray> contactRays; // Store information for rays that shoot at the contact points for this plane and the colon mesh

    ///// <summary>
    ///// Return a list of ray to be cast onto the target mesh to paint staple line
    ///// </summary>
    ///// <returns></returns>
    //public List<Ray> GetPaintContacts()
    //{

    //}

    private void OnCollisionStay(Collision collision)
    {
        contactRays.Clear();
        foreach (ContactPoint contact in collision.contacts)
        {
            contactRays.Add(new Ray(contact.point + contact.normal, -contact.normal));
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        contactRays.Clear();
    }
}
