using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Grabbers;

public class HeadPPEController : MonoBehaviour
{
    public SkinnedMeshRenderer bouffantRenderer;
    public SkinnedMeshRenderer faceMaskRenderer;
    public WarningMessage warning;

    private enum HeadPPETypes { Bouffant, FaceMask, FaceShield }
    private List<HeadPPETypes> equipOrder = new List<HeadPPETypes>();

    private bool BouffantEquipped()
    {
        if (equipOrder.Count == 1) // shield -> bouffant
        {
            if (equipOrder[0] == HeadPPETypes.FaceShield)
                bouffantRenderer.SetBlendShapeWeight(0, 100); // FaceShieldUnder
        }
        else if (equipOrder.Count == 2) // mask -> shield -> bouffant
        {
            bouffantRenderer.SetBlendShapeWeight(0, 100); // FaceShieldUnder
        }

        equipOrder.Add(HeadPPETypes.Bouffant);
        return true;
    }

    public void CheckBouffantEquip(HVRHandGrabber grabber, ClothPair pair)
    {
        if (BouffantEquipped())
            pair?.ManuallyEquip(grabber);
        else
            RejectEquipOrder(pair.clothName);
    }

    private bool BouffantUnequipped()
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
                return false;

        }
        else if (equipOrder.Count == 3)
        {
            if (equipOrder[2] == HeadPPETypes.Bouffant) // mask -> shield -> bouffant
            {
                bouffantRenderer.SetBlendShapeWeight(0, 0); // FaceShieldUnder
                bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
            }
            else // bouffant -> mask -> shield OR mask -> bouffant -> shield
                return false;
        }

        equipOrder.Remove(HeadPPETypes.Bouffant);
        return true;
    }

    public void CheckBouffantUnequip(HVRHandGrabber grabber, ClothPair pair)
    {
        if (BouffantUnequipped())
            pair?.ManuallyUnequip(grabber);
        else
            RejectUnequipOrder(pair.clothName);
    }

    private bool FaceMaskEquipped()
    {
        if (equipOrder.Count == 1)
        {
            if (equipOrder[0] == HeadPPETypes.Bouffant) // bouffant -> mask
            {
                faceMaskRenderer.SetBlendShapeWeight(0, 100); // OverBouffant
                bouffantRenderer.SetBlendShapeWeight(1, 100); // FaceShieldOver
            }
            else if (equipOrder[0] == HeadPPETypes.FaceShield) // shield -> mask
                return false;
        }
        else if (equipOrder.Count == 2) // bouffant -> mask -> shield OR mask -> bouffant -> shield
            return false;

        equipOrder.Add(HeadPPETypes.FaceMask);
        return true;
    }

    public void CheckFaceMaskEquip(HVRHandGrabber grabber, ClothPair pair)
    {
        if (FaceMaskEquipped())
            pair?.ManuallyEquip(grabber);
        else
            RejectEquipOrder(pair.clothName);
    }

    private bool FaceMaskUnequipped()
    {
        if (equipOrder.Count == 2)
        {
            if (equipOrder[0] == HeadPPETypes.Bouffant) // bouffant -> mask
            {
                bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
                faceMaskRenderer.SetBlendShapeWeight(0, 0); // OverBouffant
            }
            else // mask -> bouffant OR mask -> shield
                return false;
        }
        else if (equipOrder.Count == 3) // bouffant -> mask -> shield OR mask -> bouffant -> shield
            return false;

        equipOrder.Remove(HeadPPETypes.FaceMask);
        return true;
    }

    public void CheckFaceMaskUnequip(HVRHandGrabber grabber, ClothPair pair)
    {
        if (FaceMaskUnequipped())
            pair?.ManuallyUnequip(grabber);
        else
            RejectUnequipOrder(pair.clothName);
    }

    private bool FaceShieldEquipped()
    {
        if (equipOrder.Count == 1)
        {
            if (equipOrder[0] == HeadPPETypes.Bouffant) // bouffant -> shield
                bouffantRenderer.SetBlendShapeWeight(1, 100); // FaceShieldOver
        }
        else if (equipOrder.Count == 2) // bouffant -> mask -> shield OR mask -> bouffant -> shield
        {
            bouffantRenderer.SetBlendShapeWeight(1, 100); // FaceShieldOver
        }

        equipOrder.Add(HeadPPETypes.FaceShield);
        return true;
    }

    public void CheckFaceShieldEquip(HVRHandGrabber grabber, ClothPair pair)
    {
        if (FaceShieldEquipped())
            pair?.ManuallyEquip(grabber);
        else
            RejectEquipOrder(pair.clothName);
    }

    private bool FaceShieldUnequipped()
    {
        if (equipOrder.Count == 2 && equipOrder.Contains(HeadPPETypes.Bouffant))
        {
            if (equipOrder.IndexOf(HeadPPETypes.Bouffant) < equipOrder.IndexOf(HeadPPETypes.FaceShield)) // bouffant -> shield
            {
                bouffantRenderer.SetBlendShapeWeight(0, 0); // FaceShieldUnder
                bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
            }
            else // shield -> bouffant
                return false;

        }
        else if (equipOrder.Count == 3)
        {
            if (equipOrder.IndexOf(HeadPPETypes.Bouffant) < equipOrder.IndexOf(HeadPPETypes.FaceShield)
                && equipOrder.IndexOf(HeadPPETypes.Bouffant) > equipOrder.IndexOf(HeadPPETypes.FaceMask)) // mask -> bouffant -> shield
            {
                bouffantRenderer.SetBlendShapeWeight(1, 0); // FaceShieldOver
            }
            else if (equipOrder.IndexOf(HeadPPETypes.Bouffant) > equipOrder.IndexOf(HeadPPETypes.FaceShield)) // mask -> shield -> bouffant
                return false;
        }

        equipOrder.Remove(HeadPPETypes.FaceShield);
        return true;
    }

    public void CheckFaceShieldUnequip(HVRHandGrabber grabber, ClothPair pair)
    {
        if (FaceShieldUnequipped())
            pair?.ManuallyUnequip(grabber);
        else
            RejectUnequipOrder(pair.clothName);
    }

    public void RejectEquipOrder(string name)
    {
        if (warning)
            warning.DispayWarning("Cannot don " + name + " in this order.");
    }

    public void RejectUnequipOrder(string name)
    {
        if (warning)
            warning.DispayWarning("Cannot doff " + name + " in this order.");
    }
}
