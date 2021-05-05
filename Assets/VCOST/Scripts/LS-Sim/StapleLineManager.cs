using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PaintIn3D;

/// <summary>
/// Manager the rendering of staple lines on colon mesh
/// </summary>
public class StapleLineManager : MonoBehaviour
{
    public P3dPaintSphere verticalStaplePainter; // Painter used for painting the staple line that is vertical along the texture (for step 3)
    public P3dPaintSphere horizontalStaplePainter; // Painter userd for painting the staple line that is horizontal along the texture (for step 2 & 4)
    public P3dPaintSphere eraser; // Painter used for erasing the staple line
    public MeshCollider colon1MeshCollider;

    // Update is called once per frame
    void Update()
    {
        colon1MeshCollider.sharedMesh = colon1MeshCollider.GetComponent<MeshFilter>().sharedMesh;
    }

    /// <summary>
    /// Paint the staple line along a line along a set of vertices on a mesh
    /// </summary>
    /// <param name="path"></param>
    public void PaintAlongVertices(Vector3[] path)
    {

    }
}
