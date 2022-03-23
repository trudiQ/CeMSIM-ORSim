using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScroller : MonoBehaviour
{
    public float sensitivity = 10f;

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * sensitivity;
    }
}
