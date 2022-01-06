using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;

public class GloveStretch : MonoBehaviour
{
    public enum GloveStretchState { AtWrist, UnderGown, OverGown }

    public PPEOptionPoint[] stretchReferencePoints;
    public GloveStretchState stretchState { get; private set; }
    public GameObject grabPointPrefab;
    public InteractableClothController clothController;
    public string clothName;

    private HVRGrabbable grabPoint;
    private PPEOptionPoint closestPoint;
    private ClothPair pair;
    private bool grabbed = false;
    private bool gownEquipped = false;

    void Start()
    {
        pair = clothController.clothingPairs.Find((x) => x.clothName == clothName);

        SetPointsActive(false);
    }

    void Update()
    {
        if (grabbed)
        {
            float minimumDistance = float.MaxValue;
            int minimumIndex = -1;

            for (int i = 0; i < stretchReferencePoints.Length; i++)
            {
                float distance = Vector3.Distance(grabPoint.transform.position, stretchReferencePoints[i].transform.position);

                if (distance < minimumDistance)
                {
                    minimumDistance = distance;
                    minimumIndex = i;
                }
            }

            if (closestPoint != stretchReferencePoints[minimumIndex])
            {
                if (closestPoint)
                    closestPoint.Unhover();

                closestPoint = stretchReferencePoints[minimumIndex];
                closestPoint.Hover();
            }
        }
    }

    public void GownEquipped()
    {
        gownEquipped = true;
    }

    public void GownUnequipped()
    {
        if (pair.equipCount > 0 && stretchState == GloveStretchState.OverGown)
        {
            stretchReferencePoints[0].Hover();
            stretchReferencePoints[0].Select();
        }

        gownEquipped = false;
    }

    public void GloveEquipped(HVRHandGrabber grabber)
    {
        if (pair.equipCount == 1)
        {
            if (gownEquipped)
            {
                GrabStretchPoint(grabber);
            }
            else
            {
                stretchReferencePoints[0].Hover();
                stretchReferencePoints[0].Select();
            }
        }
    }

    public void GloveUnequipped()
    {
        if (closestPoint && pair.equipCount == 0)
            closestPoint.Unhover();
    }

    private void GrabStretchPoint(HVRHandGrabber grabber)
    {
        if (grabber)
        {
            grabPoint = Instantiate(grabPointPrefab, transform.position, Quaternion.identity).GetComponent<HVRGrabbable>();
            grabber.TryGrab(grabPoint, true);
            grabPoint.HandReleased.AddListener((_1, _2) => SelectClosestPoint());
            SetPointsActive(true);

            grabbed = true;
        }
    }


    public void SelectClosestPoint()
    {
        closestPoint.Select();
        Destroy(grabPoint.gameObject);
        SetPointsActive(false);

        grabbed = false;
    }

    public void SetStretchState(int state)
    {
        stretchState = (GloveStretchState)state;
    }

    private void SetPointsActive(bool state)
    {
        foreach (PPEOptionPoint point in stretchReferencePoints)
            point.gameObject.SetActive(state);
    }
}
