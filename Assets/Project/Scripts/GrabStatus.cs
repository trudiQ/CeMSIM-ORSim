using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabStatus : MonoBehaviour
{
    public bool isCurrentGrab = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetStatusTrue()
    {
        isCurrentGrab = true;
    }

    public void SetStatusFalse()
    {
        isCurrentGrab = false;
    }
}
