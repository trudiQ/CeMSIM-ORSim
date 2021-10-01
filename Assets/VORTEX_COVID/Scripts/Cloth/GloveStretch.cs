using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;

[RequireComponent(typeof(HVRGrabbable))]
public class GloveStretch : MonoBehaviour
{
    public PPEOptionPoint[] stretchReferencePoints;

    public float stretchPointDistanceThreshold = 1f;

    private HVRGrabbable grabbable;
    private PPEOptionPoint closestPoint;
    private bool grabbed = false;
    private bool pointsActivated = false;

    void Start()
    {
        gameObject.SetActive(false);

        SetPointsActive(false);

        grabbable = GetComponent<HVRGrabbable>();
    }

    void Update()
    {
        if (grabbed)
        {
            if (!pointsActivated)
                SetPointsActive(true);

            float minimumDistance = float.MaxValue;
            int minimumIndex = -1;

            for (int i = 0; i < stretchReferencePoints.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, stretchReferencePoints[i].transform.position);

                if (distance < stretchPointDistanceThreshold && distance < minimumDistance)
                {
                    minimumDistance = distance;
                    minimumIndex = i;
                    Debug.Log("Closest: " + stretchReferencePoints[i].name);
                }
            }

            if (closestPoint != stretchReferencePoints[minimumIndex])
            {
                closestPoint?.Unhover();
                closestPoint = stretchReferencePoints[minimumIndex];
                closestPoint.Hover();
            }
        }
    }

    public void GrabStretchPoint(HVRHandGrabber grabber)
    {
        if (grabber)
        {
            Debug.Log("Grabbed");
            gameObject.SetActive(true);
            grabber.TryGrab(grabbable, true);
            grabbed = true;
        }
    }

    public void SelectClosestPoint()
    {
        Debug.Log("Selected: " + closestPoint.name);
        closestPoint.Select();

        gameObject.SetActive(false);
        SetPointsActive(false);

        grabbed = false;
    }

    private void SetPointsActive(bool state)
    {
        foreach (PPEOptionPoint point in stretchReferencePoints)
            point.gameObject.SetActive(state);

        pointsActivated = state;
    }
}
