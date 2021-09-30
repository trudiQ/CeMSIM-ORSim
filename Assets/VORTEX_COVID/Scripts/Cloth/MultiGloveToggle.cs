using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGloveToggle : MonoBehaviour
{
    public GameObject gloveObjectWithSkinnedMesh;
    public GameObject handObjectWithSkinnedMesh;
    public int gloveCount;

    private int currentGloveEquippedCount = 0;

    public void GloveEquipped()
    {
        if (++currentGloveEquippedCount >= 1)
        {
            gloveObjectWithSkinnedMesh.SetActive(true);
            handObjectWithSkinnedMesh.SetActive(false);
        }
    }

    public void GloveUnequipped()
    {
        if (--currentGloveEquippedCount == 0)
        {
            gloveObjectWithSkinnedMesh.SetActive(false);
            handObjectWithSkinnedMesh.SetActive(true);
        }
    }
}
