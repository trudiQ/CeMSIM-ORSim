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

    private void Update()
    {
        UpdatePosition();

    }

    /// <summary>
    /// Update its position based on its belonged triangle
    /// </summary>
    public void UpdatePosition()
    {
        transform.position = GetVertPosition(belongedTriangleIndex * 3) * positionWeight.x + GetVertPosition(belongedTriangleIndex * 3 + 1) * positionWeight.y + GetVertPosition(belongedTriangleIndex * 3 + 2) * positionWeight.z;
    }

    /// <summary>
    /// Make sure this staple object always face towards its belonged triangle's normal
    /// </summary>
    public void UpdateRotation()
    {

    }

    public Vector3 GetVertPosition(int vertIndex)
    {
        return belongedObjet.TransformPoint(belongedObjectMeshFilter.sharedMesh.vertices[belongedObjectMeshFilter.sharedMesh.triangles[vertIndex]]);
    }
}
