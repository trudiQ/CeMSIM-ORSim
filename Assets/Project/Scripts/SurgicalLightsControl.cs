using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurgicalLightsControl : MonoBehaviour
{
    public MeshRenderer surgicalLight01;
    public MeshRenderer surgicalLight02;

    public Material surgicalLightsOnMaterial;
    public Material surgicalLightsOffMaterial;

    public GameObject surgicalSpotLightIllumination01;
    public GameObject surgicalSpotLightIllumination02;


    private bool isSurgicalLightsOn;

    // Start is called before the first frame update
    void Start()
    {
        isSurgicalLightsOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSurgicalLightsOn == false)
        {
            SurgicalLightsOff();
        }
        else if (isSurgicalLightsOn == true)
        {
            SurgicalLightsOn();
        }
    }

    private void SurgicalLightsOn()
    {
        Material[] mats01 = surgicalLight01.materials;
        Material[] mats02 = surgicalLight02.materials;

        mats01[7] = surgicalLightsOnMaterial;
        mats02[6] = surgicalLightsOnMaterial;

        surgicalLight01.materials = mats01;
        surgicalLight02.materials = mats02;

        surgicalSpotLightIllumination01.SetActive(true);
        surgicalSpotLightIllumination02.SetActive(true);
    }

    private void SurgicalLightsOff()
    {
        Material[] mats01 = surgicalLight01.materials;
        Material[] mats02 = surgicalLight02.materials;

        mats01[7] = surgicalLightsOffMaterial;
        mats02[6] = surgicalLightsOffMaterial;

        surgicalLight01.materials = mats01;
        surgicalLight02.materials = mats02;

        surgicalSpotLightIllumination01.SetActive(false);
        surgicalSpotLightIllumination02.SetActive(false);

    }

    public void ButtonPushedDown()
    {
        isSurgicalLightsOn = !isSurgicalLightsOn;
    }
}
