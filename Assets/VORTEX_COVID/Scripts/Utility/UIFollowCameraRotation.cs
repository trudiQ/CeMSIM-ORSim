using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowCameraRotation : MonoBehaviour
{
    public Transform cameraTransform;

    private void LateUpdate()
    {
        if (gameObject.activeInHierarchy)
            transform.LookAt(transform.position + cameraTransform.forward, cameraTransform.rotation * Vector3.up);
    }
}
