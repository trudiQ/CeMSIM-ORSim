using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Shared.HandPoser;

public class GrabPointHover : MonoBehaviour
{
    public List<GrabPointHoverEvent> grabPoints;

    private bool hovered = false;
    private GrabPointHoverEvent closestHoverPoint;
    private HVRGrabberBase currentGrabber;
    private HVRGrabbable currentGrabbable;

    private void Update()
    {
        if (hovered)
        {
            HVRPosableGrabPoint grabPoint = GetHoveredGrabPoint(currentGrabber, currentGrabbable);
            GrabPointHoverEvent newClosestHoverPoint = grabPoints.Find((x) => x.grabPoint == grabPoint);

            if (newClosestHoverPoint != closestHoverPoint)
            {
                closestHoverPoint?.OnUnhovered.Invoke();
                closestHoverPoint = newClosestHoverPoint;
                closestHoverPoint.OnHovered.Invoke();
            }
        }
    }

    public void OnHandHovered(HVRGrabberBase grabber, HVRGrabbable grabbable)
    {
        currentGrabber = grabber;
        currentGrabbable = grabbable;
        
        hovered = true;
    }

    public void OnHandUnhovered(HVRGrabberBase grabber, HVRGrabbable grabbable)
    {
        closestHoverPoint?.OnUnhovered.Invoke();

        closestHoverPoint = null;
        currentGrabber = null;
        currentGrabbable = null;

        hovered = false;
    }

    private HVRPosableGrabPoint GetHoveredGrabPoint(HVRGrabberBase grabber, HVRGrabbable grabbable)
    {
        List<GrabPointMeta> grabPoints = grabbable.GrabPointsMeta;
        List<GrabPointDistance> nearbyGrabPoints = new List<GrabPointDistance>();

        // Below code is modified from the HVRHandGrabber GetGrabPoint function
        foreach (GrabPointMeta grabPoint in grabPoints)
        {
            if (!grabPoint.GrabPoint)
                continue;

            // Check how far away the grab point is from the hand's anchor
            Vector3 grabbableWorldAnchor = grabPoint.GrabPoint.position;
            float distance = Vector3.Distance(grabbableWorldAnchor, grabber.JointAnchorWorldPosition);

            nearbyGrabPoints.Add(new GrabPointDistance(grabPoint, distance));
        }

        if (nearbyGrabPoints.Count > 0)
            nearbyGrabPoints.Sort((x, y) => x.distance.CompareTo(y.distance));

        return nearbyGrabPoints[0].grabPoint.PosableGrabPoint;
    }

    private struct GrabPointDistance
    {
        public GrabPointMeta grabPoint;
        public float distance;

        public GrabPointDistance(GrabPointMeta grabPoint, float distance)
        {
            this.grabPoint = grabPoint;
            this.distance = distance;
        }
    }

    [System.Serializable]
    public class GrabPointHoverEvent
    {
        public HVRPosableGrabPoint grabPoint;
        public UnityEvent OnHovered;
        public UnityEvent OnUnhovered;
    }
}
