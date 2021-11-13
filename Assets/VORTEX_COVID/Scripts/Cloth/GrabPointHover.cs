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

    public void OnHandHovered(HVRGrabberBase grabber, HVRGrabbable grabbable)
    {
        HVRPosableGrabPoint grabPoint = GetHoveredGrabPoint(grabber, grabbable);
        GrabPointHoverEvent hoverEvent = grabPoints.Find((x) => x.grabPoint == grabPoint);

        hoverEvent.OnHovered.Invoke();
    }

    public void OnHandUnhovered(HVRGrabberBase grabber, HVRGrabbable grabbable)
    {
        HVRPosableGrabPoint grabPoint = GetHoveredGrabPoint(grabber, grabbable);
        GrabPointHoverEvent hoverEvent = grabPoints.Find((x) => x.grabPoint == grabPoint);

        hoverEvent.OnUnhovered.Invoke();
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
