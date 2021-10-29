using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPPEController : MonoBehaviour
{
    public SkinnedMeshRenderer bouffantRenderer;
    public SkinnedMeshRenderer faceMaskRenderer;

    private enum HeadPPETypes { Bouffant, FaceMask, FaceShield }

    private List<HeadPPETypes> equipOrder = new List<HeadPPETypes>();

    public void BouffantEquipped()
    {
        if (equipOrder.Count == 1) // shield -> bouffant
        {
            if (equipOrder[0] == HeadPPETypes.FaceShield)
                bouffantRenderer.SetBlendShapeWeight(0, 1); // FaceShieldUnder
        }
        else if (equipOrder.Count == 2) // mask -> shield -> bouffant
        {
            bouffantRenderer.SetBlendShapeWeight(0, 1); // FaceShieldUnder
        }

        equipOrder.Add(HeadPPETypes.Bouffant);
    }

    public void BouffantUnequipped()
    {
        if (equipOrder.Count == 2)
        {
            if (equipOrder.Contains(HeadPPETypes.FaceShield))
            {
                if (equipOrder.IndexOf(HeadPPETypes.Bouffant) > equipOrder.IndexOf(HeadPPETypes.FaceShield)) // shield -> bouffant
                {
                    bouffantRenderer.SetBlendShapeWeight(0, 0); // FaceShieldUnder
                    bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
                }
            }
            else // bouffant -> mask OR bouffant -> shield
            {
                RejectUnequipOrder();
                return;
            }

        }
        else if (equipOrder.Count == 3)
        {
            if (equipOrder[2] == HeadPPETypes.Bouffant) // mask -> shield -> bouffant
            {
                bouffantRenderer.SetBlendShapeWeight(0, 0); // FaceShieldUnder
                bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
            }
            else // bouffant -> mask -> shield OR mask -> bouffant -> shield
            {
                RejectUnequipOrder();
                return;
            }
        }

        equipOrder.Remove(HeadPPETypes.Bouffant);
    }

    public void FaceMaskEquipped()
    {
        if (equipOrder.Count == 1)
        {
            if (equipOrder[0] == HeadPPETypes.Bouffant) // bouffant -> mask
            {
                faceMaskRenderer.SetBlendShapeWeight(0, 1); // OverBouffant
                bouffantRenderer.SetBlendShapeWeight(1, 1); // FaceShieldOver
            }
            else if (equipOrder[0] == HeadPPETypes.FaceShield) // shield -> mask
            {
                RejectEquipOrder();
                return;
            }
        }
        else if (equipOrder.Count == 2) // bouffant -> mask -> shield OR mask -> bouffant -> shield
        {
            RejectEquipOrder();
            return;
        }

        equipOrder.Add(HeadPPETypes.FaceMask);
    }

    public void FaceMaskUnequipped()
    {
        if (equipOrder.Count == 2)
        {
            if (equipOrder[0] == HeadPPETypes.Bouffant) // bouffant -> mask
            {
                bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
                faceMaskRenderer.SetBlendShapeWeight(0, 0); // OverBouffant
            }
            else // mask -> bouffant OR mask -> shield
            {
                RejectUnequipOrder();
                return;
            }
        }
        else if (equipOrder.Count == 3) // bouffant -> mask -> shield OR mask -> bouffant -> shield
        {
            RejectUnequipOrder();
            return;
        }

        equipOrder.Remove(HeadPPETypes.FaceMask);
    }

    public void FaceShieldEquipped()
    {
        if (equipOrder.Count == 1)
        {
            if (equipOrder[0] == HeadPPETypes.Bouffant) // bouffant -> shield
                bouffantRenderer.SetBlendShapeWeight(1, 1); // FaceShieldOver
        }
        else if (equipOrder.Count == 2) // bouffant -> mask -> shield OR mask -> bouffant -> shield
        {
            bouffantRenderer.SetBlendShapeWeight(1, 1); // FaceShieldOver
        }

        equipOrder.Add(HeadPPETypes.FaceShield);
    }

    public void FaceShieldUnequipped()
    {
        if (equipOrder.Count == 2 && equipOrder.Contains(HeadPPETypes.Bouffant))
        {
            if (equipOrder.IndexOf(HeadPPETypes.Bouffant) < equipOrder.IndexOf(HeadPPETypes.FaceShield)) // bouffant -> shield
            {
                bouffantRenderer.SetBlendShapeWeight(0, 0); // FaceShieldUnder
                bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
            }
            else // shield -> bouffant
            {
                RejectUnequipOrder();
                return;
            }

        }
        else if (equipOrder.Count == 3)
        {
            if (equipOrder.IndexOf(HeadPPETypes.Bouffant) < equipOrder.IndexOf(HeadPPETypes.FaceShield)
                && equipOrder.IndexOf(HeadPPETypes.Bouffant) > equipOrder.IndexOf(HeadPPETypes.FaceMask)) // mask -> bouffant -> shield
            {
                bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
            }
            else if (equipOrder.IndexOf(HeadPPETypes.Bouffant) > equipOrder.IndexOf(HeadPPETypes.FaceShield)) // mask -> shield -> bouffant
            {
                RejectUnequipOrder();
                return;
            }
        }

        equipOrder.Remove(HeadPPETypes.FaceShield);
    }

    public void RejectEquipOrder()
    {
        Debug.Log("Bad Equip Order");
    }

    public void RejectUnequipOrder()
    {
        Debug.Log("Bad Unequip Order");
    }
}
