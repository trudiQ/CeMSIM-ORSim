using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmLightController : MonoBehaviour
{
    public Material[] materials;
    public MeshRenderer[] meshRenderers;
    private int colorIndex = 0;

    public void TurnOnRedLight()
    {
        foreach(var renderer in meshRenderers)
        {
            renderer.material = materials[1];
        }

        colorIndex = 0;
    }

    public void TurnOnYellowLight()
    {
        foreach(var renderer in meshRenderers)
        {
            renderer.material = materials[3];
        }

        colorIndex = 1;
    }

    public void AlteranateLitLight(int index)
    {
        switch (colorIndex)
        {
            case 0:
                if(index == 0)
                {
                    meshRenderers[0].material = materials[2];
                    meshRenderers[1].material = materials[1];
                }
                else if(index == 1)
                {
                    meshRenderers[0].material = materials[1];
                    meshRenderers[1].material = materials[2];
                }
                break;
            case 1:
                if(index == 0)
                {
                    meshRenderers[0].material = materials[4];
                    meshRenderers[1].material = materials[3];
                }
                else if(index == 1)
                {
                    meshRenderers[0].material = materials[3];
                    meshRenderers[1].material = materials[4];
                }
                break;
        }
    }

    

    public void TurnOffLight()
    {
        foreach(var renderer in meshRenderers)
        {
            renderer.material = materials[0];
        }
    }

}
