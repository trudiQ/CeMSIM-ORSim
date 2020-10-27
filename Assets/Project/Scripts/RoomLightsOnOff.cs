using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class RoomLightsOnOff : MonoBehaviour
{
    public Volume postProcessingVolume;

    public GameObject ceilingAreaLights;
    public GameObject ceilingPointLights;

    public MeshRenderer ceilingLightsMesh;
    public MeshRenderer surgicalLight01;
    public MeshRenderer surgicalLight02;

    public Material ceilingLightsOnMaterial;
    public Material ceilingLightsOffMaterial;
    public Material surgicalLightsOnMaterial;
    public Material surgicalLightsOffMaterial;


    Vignette vignetteLayer = null;
    ColorAdjustments colorAdjustmentLayer = null;

    private bool isLightsOn;

    // Start is called before the first frame update
    void Start()
    {
        isLightsOn = true;

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
        //To do (9/29/20): This should only be called when switching the light, seems like
        //its being called on update atm

        //Change visual by using post-processing and switching materials
        vignetteLayer.intensity.value = 0.3f;
        vignetteLayer.smoothness.value = 0.2f;
        vignetteLayer.rounded.value = false;
        colorAdjustmentLayer.saturation.value = 0f;
        //Debug.Log("Lights On");
        ceilingAreaLights.SetActive(true);
        ceilingPointLights.SetActive(true);
        //ceilingLightsMesh.material = ceilingLightsOnMaterial;
    }

    private void RoomLightsOff()
    {
        //To do (9/29/20): This should only be called when switching the light, seems like
        //its being called on update atm

        vignetteLayer.intensity.value = 1.00f;
        vignetteLayer.smoothness.value = 1.00f;
        vignetteLayer.rounded.value = true;
        colorAdjustmentLayer.saturation.value = -100f;
        //Debug.Log("Lights Off");
        ceilingAreaLights.SetActive(false);
        ceilingPointLights.SetActive(false);
        //ceilingLightsMesh.material = ceilingLightsOffMaterial;
        //surgicalLight01.materials[7] = surgicalLightsOffMaterial;
        //surgicalLight02.materials[6] = surgicalLightsOffMaterial;
    }

    public void SwitchActivation()
    {
        isLightsOn = !isLightsOn;
    }
}