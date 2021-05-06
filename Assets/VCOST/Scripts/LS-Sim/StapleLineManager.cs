using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PaintIn3D;
using Sirenix.OdinInspector;
using System.Linq;

/// <summary>
/// Manager the rendering of staple lines on colon mesh
/// </summary>
public class StapleLineManager : MonoBehaviour
{
    public List<Transform> testPaint;
    public MeshCollider col;
    public float paintInterval; // Distance between each brush touch when paint from point A to point B
    public int paintCount; // How many times the brush should paint for each touch
    public int brushSize;
    public P3dPaintSphere staplePainter; // Painter used for painting the staple line
    public P3dPaintSphere eraser; // Painter used for erasing the staple line
    public Transform staplePainterBrushControl; // Used to define the behavior of the staple line painter brush 



    private void Update()
    {
        //col.sharedMesh = col.GetComponent<MeshFilter>().sharedMesh;
    }

    [ShowInInspector]
    public void TestPaint()
    {
        PaintAlongVertices(testPaint.Select(t => t.position).ToArray(), staplePainter, Vector3.one * brushSize, Vector3.zero);
    }

    [ShowInInspector]
    public void ResetPaint()
    {
        PaintAlongVertices(testPaint.Select(t => t.position).ToArray(), eraser, Vector3.one * brushSize, Vector3.zero);
    }

    /// <summary>
    /// Paint the staple line along a line along a set of vertices on a mesh
    /// </summary>
    /// <param name="path"></param>
    /// <param name="brushType"></param>
    /// <param name="brushSize"></param>
    /// <param name="brushRotation"></param>
    public void PaintAlongVertices(Vector3[] path, P3dPaintSphere brushType, Vector3 brushSize, Vector3 brushRotation)
    {
        staplePainterBrushControl.localScale = brushSize;
        staplePainterBrushControl.eulerAngles = brushRotation;
        for (int i = 0; i < path.Length - 1; i++)
        {
            for (int c = 0; c < paintCount; c++)
            {
                PaintLine(path[i], path[i + 1], brushType);
            }
        }
    }

    public void PaintLine(Vector3 startPos, Vector3 endPos, P3dPaintSphere brush)
    {
        float distance = Vector3.Distance(startPos, endPos);
        int step = Mathf.RoundToInt(distance / paintInterval);
        for (float t = 0; t < 1; t += 1f / step)
        {
            brush.HandleHitPoint(false, 0, 1, 0, Vector3.Lerp(startPos, endPos, t), Quaternion.identity);
        }
    }
}
