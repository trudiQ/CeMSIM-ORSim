using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticCameraController : MonoBehaviour
{
    public HapticPlugin haptic;
    public KeyCode activationKey = KeyCode.C;

    // Start is called before the first frame update
    void Start()
    {
        if (haptic == null)
            Debug.LogError("Haptic device is not set!");
    }

    private Vector3 originalHapticPosition = Vector3.zero;
    private Vector3 originalCameraPosition = Vector3.zero;

    private Vector3 hapticOffset = Vector3.zero;
    private Transform hapticManipulator;
    private void FixedUpdate()
    {
        hapticManipulator = haptic.hapticManipulator.transform;
        if (hapticManipulator == null) return;

        if (Input.GetKeyDown(activationKey))
        {
            originalHapticPosition = hapticManipulator.position;
            originalCameraPosition = transform.position;
        }

        if (Input.GetKey(activationKey))
        {
            hapticOffset = hapticManipulator.position - originalHapticPosition;
            transform.position = Vector3.Lerp(transform.position, originalCameraPosition + hapticOffset, Time.fixedDeltaTime);
        }
    }
}
