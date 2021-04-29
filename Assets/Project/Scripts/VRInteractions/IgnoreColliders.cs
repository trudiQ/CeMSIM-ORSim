using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreColliders : MonoBehaviour
{
    public Collider[] collidersToIgnore;
    // Start is called before the first frame update
    void Awake()
    {
        foreach(Collider col in collidersToIgnore)
        {
            if(col)
            {
                Physics.IgnoreCollision(col, this.GetComponent<Collider>(), true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
