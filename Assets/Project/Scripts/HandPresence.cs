using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public bool showController = false;
    public InputDeviceCharacteristics controllerCharacteristics;
    public List<GameObject> controllerPrefabs;
    public GameObject handModelPrefab;

    private InputDevice targetDevice;
    private GameObject spawnController;
    private GameObject spanwHandModel;
    private Animator handAnimator;

    // Start is called before the first frame update
    void Start()
    {
        TryInitialize();
    }

    // Update is called once per frame
    void Update()
    {
        //To ensure hand presence is instantiated correctly
        if (!targetDevice.isValid)
        {
            TryInitialize();
        }
        else
        {
            if (showController)
            {
                spanwHandModel.SetActive(false);
                spawnController.SetActive(true);
            }
            else
            {
                spanwHandModel.SetActive(true);
                spawnController.SetActive(false);
                UpdateHandAnimation();
            }
        }
    }

    void TryInitialize()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        foreach (var item in devices)
        {
            Debug.Log(item.name + item.characteristics);
        }

        if (devices.Count > 0)
        {
            //find target models by referencing controller name
            targetDevice = devices[0];
            GameObject prefab = controllerPrefabs.Find(controller => controller.name == targetDevice.name);
            if (prefab)
            {
                spawnController = Instantiate(prefab, transform);
            }
            else
            {
                Debug.LogError("Cannot find corresponding controller model");
                spawnController = Instantiate(controllerPrefabs[0], transform);
            }

            spanwHandModel = Instantiate(handModelPrefab, transform);
            handAnimator = spanwHandModel.GetComponent<Animator>();
        }
    }

    void UpdateHandAnimation()
    {
        //If models have animation then play corresponding animation
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }
}
