using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class LightController : MonoBehaviour
{
    public Material lightOnMaterial;
    public Material lightOffMaterial;
    public Light overheadLight;
    public MeshRenderer overheadLightMesh;
    public Light[] spotLights;
    public MeshRenderer[] articulatingLights;
    public Volume lightingVolume;
    private VolumeProfile profile;
    private IndirectLightingController indirectLightingController;
    private bool lightsToggled = false;


    // Start is called before the first frame update
    void Start()
    {
        InitializeIndirectLightController();
    }


    private void InitializeIndirectLightController()
    {
        IndirectLightingController temp;
        if(lightingVolume.profile.TryGet<IndirectLightingController>(out temp))
        {
            indirectLightingController = temp;
        }

        // indirectLightingController.active = false;
        indirectLightingController.indirectDiffuseIntensity.value = 1;
        
    }

    public void ToggleLights()
    {
        if(!lightsToggled)
        {
            TurnOverheadLightsOff();
            lightsToggled = true;
        }
        else
        {
            TurnOverheadLightsOn();
            lightsToggled = false;
        }
    }

    public void ToggleIndirectLighting(bool state)
    {
        indirectLightingController.active = state;
    }

    public void TurnOverheadLightsOff()
    {
        Material[] mats = overheadLightMesh.materials;
        mats[1] = lightOffMaterial;
        overheadLightMesh.materials = mats;

        overheadLight.enabled = false;
        foreach(var light in spotLights)
        {
            light.enabled = true;
        }
        foreach(var light in articulatingLights)
        {
            light.material = lightOnMaterial;
        }
        // ToggleIndirectLighting(true);
        indirectLightingController.indirectDiffuseIntensity.value = .3f;

    }

    public void TurnOverheadLightsOn()
    {
        Material[] mats = overheadLightMesh.materials;
        mats[1] = lightOnMaterial;
        overheadLightMesh.materials = mats;
        
        overheadLight.enabled = true;
        foreach(var light in spotLights)
        {
            light.enabled = false;
        }
        foreach(var light in articulatingLights)
        {
            light.material = lightOffMaterial;
        }
        // ToggleIndirectLighting(false);
        indirectLightingController.indirectDiffuseIntensity.value = 1f;

    }
}
