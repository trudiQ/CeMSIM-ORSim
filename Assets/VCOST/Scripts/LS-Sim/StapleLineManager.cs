using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PaintIn3D;
using Sirenix.OdinInspector;
using System.Linq;

namespace PaintIn3D
{
    /// <summary>
    /// Manager the rendering of staple lines on colon mesh
    /// </summary>
    public class StapleLineManager : P3dHitScreenBase
    {
        public List<Transform> testPaint;
        public MeshCollider col;
        public StapleLineLocator paintPointsLocator; // A plane that collide with the colon mesh to decide the position for painting staple line
        public float paintInterval; // Distance between each brush touch when paint from point A to point B
        public int paintCount; // How many times the brush should paint for each touch
        public int brushSize;
        public float brushAngle; // Angle of the texture (basically horizontal (0 degree) or vertical (90 degree)
        public P3dPaintSphere staplePainter; // Painter used for painting the staple line
        public P3dHitScreen staplePainterMouseSim; // Used to simulate paint staple line with mouse click
        public P3dPaintSphere eraser; // Painter used for erasing the staple line
        public P3dHitScreen eraserMouseSim; // Used to simulate erase with mouse click
        public Transform painterBrushControl; // Used to define the behavior of the staple line painter brush 



        private void Update()
        {
            col.sharedMesh = col.GetComponent<MeshFilter>().sharedMesh;
        }

        [ShowInInspector]
        public void TestPaint()
        {
            PaintAlongPlaneCollision(paintPointsLocator, staplePainterMouseSim, Vector3.one * brushSize, Vector3.up * brushAngle);
            //PaintAlongVertices(testPaint.Select(t => t.position).ToArray(), staplePainter, Vector3.one * brushSize, Vector3.zero);
        }

        [ShowInInspector]
        public void ResetPaint()
        {
            PaintAlongVertices(testPaint.Select(t => t.position).ToArray(), eraser, Vector3.one * brushSize, Vector3.zero);
        }

        /// <summary>
        /// Use a plane to collide with the mesh to paint, then raycast on the collision points to simulate mouse click paint
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="brush"></param>
        /// <param name="brushSize"></param>
        /// <param name="brushRotation"></param>
        public void PaintAlongPlaneCollision(StapleLineLocator plane, P3dHitScreen brush, Vector3 brushSize, Vector3 brushRotation)
        {
            painterBrushControl.localScale = brushSize;
            painterBrushControl.eulerAngles = brushRotation;

            List<Vector3> planeVetices = new List<Vector3>(plane.GetComponent<MeshFilter>().sharedMesh.vertices);
            foreach (Vector3 v in planeVetices)
            {
                List<Vector3> neighbors = GetNeighborVertices(v, planeVetices);

                Vector3 origin = plane.transform.TransformPoint(v);
                Vector3 beginRay = plane.transform.TransformPoint(neighbors[0]) - origin;
                Vector3 endRay = plane.transform.TransformPoint(neighbors[1]) - origin;

                foreach (Ray r in GetRaysBetweenTwoVectors(origin, beginRay, endRay))
                {
                    for (int c = 0; c < paintCount; c++)
                    {
                        SimulateMouseClickPaint(r, brush);
                    }
                }
            }
        }

        /// <summary>
        /// Get the rays in a fan shape between two vectors
        /// </summary>
        /// <param name="beginVector"></param>
        /// <param name="endVector"></param>
        /// <returns></returns>
        public List<Ray> GetRaysBetweenTwoVectors(Vector3 rayBeginPosition, Vector3 beginVector, Vector3 endVector)
        {
            List<Ray> rays = new List<Ray>();

            for (float t = 0; t < 1; t += 0.001f)
            {
                rays.Add(new Ray(rayBeginPosition, Vector3.Lerp(beginVector, endVector, t).normalized));
            }

            return rays;
        }

        /// <summary>
        /// Get two vertices that's closest to the given vertex
        /// </summary>
        /// <param name="connectingVertex"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public List<Vector3> GetNeighborVertices(Vector3 connectingVertex, List<Vector3> vertices)
        {
            if (vertices.Count < 2)
            {
                return null;
            }

            float closestDist = float.MaxValue;
            float nextClosestDist = float.MaxValue;
            Vector3 closestVertex = Vector3.zero;
            Vector3 nextClosestVertex = Vector3.zero;

            foreach (Vector3 v in vertices)
            {
                if (v == connectingVertex)
                {
                    continue;
                }

                float dist = Vector3.Distance(connectingVertex, v);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestVertex = v;
                    continue;
                }
                if (dist < nextClosestDist)
                {
                    nextClosestDist = dist;
                    nextClosestVertex = v;
                }
            }

            List<Vector3> neighbors = new List<Vector3>();
            neighbors.Add(closestVertex);
            neighbors.Add(nextClosestVertex);
            return neighbors;
        }

        public void SimulateMouseClickPaint(Ray hitRay, P3dHitScreen brush)
        {
            var hit3D = default(RaycastHit);
            var finalPosition = default(Vector3);
            var finalRotation = default(Quaternion);

            // Hit 3D?
            if (Physics.Raycast(hitRay, out hit3D, float.PositiveInfinity, brush.Layers))
            {
                CalcHitData(hit3D.point, hit3D.normal, hitRay, out finalPosition, out finalRotation);

                brush.Connector.SubmitPoint(brush.gameObject, false, 0, 1, finalPosition, finalRotation, new Link());
            }
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
            painterBrushControl.localScale = brushSize;
            painterBrushControl.eulerAngles = brushRotation;
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

        private void CalcHitData(Vector3 hitPoint, Vector3 hitNormal, Ray ray, out Vector3 finalPosition, out Quaternion finalRotation)
        {
            finalPosition = hitPoint + hitNormal * 0;
            finalRotation = Quaternion.identity;

            var finalNormal = default(Vector3);

            finalNormal = hitNormal;

            var finalUp = Vector3.up;

            finalRotation = Quaternion.LookRotation(-finalNormal, finalUp);
        }

        // This stores extra information for each finger unique to this component
        protected class Link : P3dInputManager.Link
        {
            public float Age;
            public bool Down;
            public int State;
            public float Distance;
            public Vector2 ScreenDelta;
            public Vector2 ScreenOld;

            public List<Vector2> History = new List<Vector2>();

            public void Move(Vector2 screenNew)
            {
                if (State == 0)
                {
                    ScreenOld = screenNew;
                    State = 1;
                }
                else
                {
                    if (TryMove(screenNew) == true || State == 2)
                    {
                        State += 1;
                    }
                }
            }

            private bool TryMove(Vector2 screenNew)
            {
                var threshold = 2.0f;
                var distance = Vector2.Distance(ScreenOld, screenNew);

                if (distance >= threshold)
                {
                    ScreenOld = Vector2.MoveTowards(ScreenOld, screenNew, distance - threshold * 0.5f);

                    return true;
                }

                return false;
            }

            public override void Clear()
            {
                Age = 0.0f;
                Down = false;
                State = 0;
                Distance = 0.0f;
                ScreenDelta = Vector2.zero;
                ScreenOld = Vector2.zero;

                History.Clear();
            }
        }
    }
}

