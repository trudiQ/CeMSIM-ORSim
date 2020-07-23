using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class RoomLightsOnOff : MonoBehaviour
{
    public Volume postProcessingVolume;

    Vignette vignetteLayer = null;
    ColorAdjustments colorAdjustmentLayer = null;

    private bool isLightsOn;


    // Start is called before the first frame update
    void Start()
    {
        isLightsOn = false;

        postProcessingVolume.profile.TryGet(out vignetteLayer);
        postProcessingVolume.profile.TryGet(out colorAdjustmentLayer);
    }

    // Update is called once per frame
    void Update()
    {
        if (isLightsOn == false)
        {
            RoomLightsOff();
        }
        else if (isLightsOn == true)
        {
            RoomLightsOn();
        }
    }

    private void RoomLightsOn()
    {
        vignetteLayer.intensity.value = 0.3f;
        vignetteLayer.smoothness.value = 0.2f;
        vignetteLayer.rounded.value = false;
        colorAdjustmentLayer.saturation.value = 0f;
        Debug.Log("Lights Off");
    }

    private void RoomLightsOff()
    {
        vignetteLayer.intensity.value = 1.00f;
        vignetteLayer.smoothness.value = 1.00f;
        vignetteLayer.rounded.value = true;
        colorAdjustmentLayer.saturation.value = -100f;
        Debug.Log("Lights Off");
    }

    public void SwitchActivation()
    {
        isLightsOn = !isLightsOn;
    }
}