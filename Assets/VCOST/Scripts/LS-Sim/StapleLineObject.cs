using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StapleLineObject : MonoBehaviour
{
    public Transform belongedObjet; // The transform of the mesh object it should attach to
    public MeshFilter belongedObjectMeshFilter;
    public int belongedTriangleIndex; // Use (belongedTriangleIndex * 3) and (belongedTriangleIndex * 3 + 1) and (belongedTriangleIndex * 3 + 2) to get the vertices
    public Vector3 positionWeight; // The weights of the position of this staple object towards the three vertices of its belonged triangle
    public Vector3 upDirection; // Which direction the staple object's y axis is facing towards

    public Vector3 vertA;
    public Vector3 vertB;
    public Vector3 vertC;

    private void Update()
    {
        vertA = GetVertPosition(belongedTriangleIndex * 3);
        vertB = GetVertPosition(belongedTriangleIndex * 3 + 1);
        vertC = GetVertPosition(belongedTriangleIndex * 3 + 2);
        UpdatePosition();
        UpdateRotation();
    }

    /// <summary>
    /// Update its position based on its belonged triangle
    /// </summary>
    public void UpdatePosition()
    {
        transform.position = vertA * positionWeight.x + vertB * positionWeight.y + vertC * positionWeight.z;
    }

    /// <summary>
    /// Make sure this staple object always face towards its belonged triangle's normal
    /// </summary>
    public void UpdateRotation()
    {
        Plane triangle = new Plane(vertA, vertB, vertC);
        transform.LookAt(transform.position + triangle.normal, upDirection);
    }

    public Vector3 GetVertPosition(int vertIndex)
    {
        return belongedObjet.TransformPoint(belongedObjectMeshFilter.sharedMesh.vertices[belongedObjectMeshFilter.sharedMesh.triangles[vertIndex]]);
    }

    public Vector3 GetVertNormal(int vertIndex)
    {
        return belongedObjet.TransformDirection(belongedObjectMeshFilter.sharedMesh.normals[belongedObjectMeshFilter.sharedMesh.triangles[vertIndex]]);
    }
}
