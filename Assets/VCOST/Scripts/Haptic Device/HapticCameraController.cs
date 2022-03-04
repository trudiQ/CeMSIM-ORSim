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
    private bool firstObserved = true;
    private void Update()
    {
        hapticManipulator = haptic.hapticManipulator.transform;
        if (hapticManipulator == null) return;

        if (Input.GetKey(activationKey))
        {
            if (firstObserved)
            {
                originalHapticPosition = hapticManipulator.position;
                originalCameraPosition = transform.position;
                firstObserved = false;
            }
            else
            {
                hapticOffset = hapticManipulator.position - originalHapticPosition;
                transform.position = Vector3.Lerp(transform.position, originalCameraPosition + hapticOffset, Time.deltaTime);
            }
        }

        if (Input.GetKeyUp(activationKey))
        {
            firstObserved = true;
        }
    }
}
