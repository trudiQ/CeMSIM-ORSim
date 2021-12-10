using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtil : MonoBehaviour
{
    public static float DistancePointToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    public static Vector3 ProjectionPointOnLine(Ray ray, Vector3 point)
    {
        return ray.origin + Vector3.Project(point - ray.origin, ray.direction);
    }
}
