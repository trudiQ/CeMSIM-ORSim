using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGloveToggle : MonoBehaviour
{
    public int currentGloveEquippedCount = 0;

    public void GloveEquipped() => currentGloveEquippedCount++;
   
    public void GloveUnequipped() => currentGloveEquippedCount--;
}
