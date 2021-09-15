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
    public class StapleLineManagerPaintIn3D : P3dHitScreenBase
    {
        public List<GameObject> lsSimStepTwoHideStaples;
        public List<GameObject> lsSimStepThreeShowStaples;
        public List<GameObject> lsSimStepFourShowStaplesZero;
        public List<GameObject> lsSimStepFourShowStaplesOne;

        public List<Transform> testPaint;
        public float animatePaintInterval;
        public MeshCollider col;
        public List<MeshFilter> stapleObjectAttachedMeshes;
        public LayerMask paintLayers; // For raycast physics collision
        public StapleLineLocator paintPointsLocator; // A plane that collide with the colon mesh to decide the position for painting staple line
        public List<Transform> paintPointsLocators; // A list of transform that are on the same plane, which decide the position for staple line
        public float paintInterval; // Distance between each brush touch when paint from point A to point B

        public int paintCount; // How many times the brush should paint for each touch
        public int brushSize;
        public float brushAngle; // Angle of the texture (basically horizontal (0 degree) or vertical (90 degree)
        public P3dPaintSphere staplePainter; // Painter used for painting the staple line
        public P3dHitScreen staplePainterMouseSim; // Used to simulate paint staple line with mouse click
        public P3dPaintSphere eraser; // Painter used for erasing the staple line
        public P3dHitScreen eraserMouseSim; // Used to simulate erase with mouse click
        public Transform painterBrushControl; // Used to define the behavior of the staple line painter brush 

        public GameObject stapleLinePrefab; // GameObject to be used as staple line segment
        public Vector3 stapleObjectUpDirection; // Which direction staple object's up (y) axis should face
        public Transform stapleLineParent;
        public Transform circleCenter;
        public Transform circleRadA;
        public Transform circleRadB;

        public Transform twoRayStartStart;
        public Transform twoRayStartEnd;
        public Transform twoRayEndStart;
        public Transform twoRayEndEnd;

        public GameObject meshToAssignToStapleObjects;
        public Transform stapleObjectsParentToAssignMesh;

        public static StapleLineManagerPaintIn3D instance;
        public static Dictionary<MeshFilter, Mesh> updatingMeshDataForStapleObjects;
        public static Dictionary<MeshFilter, List<Vector3>> updatingMeshDataVertices;
        public static Dictionary<MeshFilter, List<int>> meshDataTriangles;

        public List<StapleLineObject> stapleObjectToEdit; // List of staple objects to edit the data by coping the data from another list of staple objects
        public List<StapleLineObject> dataToCopy; // Staple objects to provide edited data for the original staple objects to receive

        private void Start()
        {
            instance = this;

            updatingMeshDataForStapleObjects = new Dictionary<MeshFilter, Mesh>();
            stapleObjectAttachedMeshes.ForEach(mf => updatingMeshDataForStapleObjects.Add(mf, mf.sharedMesh));
            updatingMeshDataVertices = new Dictionary<MeshFilter, List<Vector3>>();
            stapleObjectAttachedMeshes.ForEach(mf => updatingMeshDataVertices.Add(mf, updatingMeshDataForStapleObjects[mf].vertices.ToList()));
            meshDataTriangles = new Dictionary<MeshFilter, List<int>>();
            stapleObjectAttachedMeshes.ForEach(mf => meshDataTriangles.Add(mf, updatingMeshDataForStapleObjects[mf].triangles.ToList()));
        }

        private void Update()
        {
            //col.sharedMesh = col.GetComponent<MeshFilter>().sharedMesh;

            stapleObjectAttachedMeshes.ForEach(mf => updatingMeshDataForStapleObjects[mf] = mf.sharedMesh);
            stapleObjectAttachedMeshes.ForEach(mf => updatingMeshDataVertices[mf] = updatingMeshDataForStapleObjects[mf].vertices.ToList());
        }

        [ShowInInspector]
        public void TestPaint()
        {
            //PaintBetweenTwoVectorsRaycast(twoRayStartStart, twoRayStartEnd, twoRayEndStart, twoRayEndEnd, stapleLineParent);
            PaintFromCircleRaycast(circleCenter.position, circleRadA.position - circleCenter.position, circleRadB.position - circleCenter.position, stapleLineParent);
            //PaintAlongPlaneCollision(paintPointsLocator, staplePainterMouseSim, Vector3.one * brushSize, Vector3.up * brushAngle);
            //PaintAlongVertices(testPaint.Select(t => t.position).ToArray(), staplePainter, Vector3.one * brushSize, Vector3.zero);
        }
        [ShowInInspector]
        public void TestPaintAnimated()
        {
            StartCoroutine(PaintAlongPlaneCollisionAnimated(paintPointsLocator, staplePainterMouseSim, Vector3.one * brushSize, Vector3.up * brushAngle));
        }

        [ShowInInspector]
        public void ResetPaint()
        {
            //PaintAlongVertices(testPaint.Select(t => t.position).ToArray(), eraser, Vector3.one * brushSize, Vector3.zero);
        }

        [ShowInInspector]
        public void AssignMeshToStapleObjects()
        {
            Transform meshTrans = meshToAssignToStapleObjects.transform;
            MeshFilter mesh = meshToAssignToStapleObjects.GetComponent<MeshFilter>();

            foreach (StapleLineObject s in stapleObjectsParentToAssignMesh.GetComponentsInChildren<StapleLineObject>(true))
            {
                s.belongedObjet = meshTrans;
                s.belongedObjectMeshFilter = mesh;
            }
        }

        /// <summary>
        /// Copy the staple object data from a list of staple objects to another list
        /// If the edited list is shorter then it will only take as many from the data provider list
        /// If the edited list is longer then the objects exceeding provider list object count will not be changed
        /// </summary>
        /// <param name="editBelongedObject"></param> Should the belonged mesh be edited
        /// <param name="editTransformData"></param> Should the position/rotation data be edited
        [ShowInInspector]
        public void EditStapleObjectData(bool editBelongedObject, bool editTransformData)
        {
            if (editBelongedObject)
            {
                for (int i = 0; i < stapleObjectToEdit.Count; i++)
                {
                    if (i > dataToCopy.Count - 1)
                    {
                        break;
                    }

                    stapleObjectToEdit[i].belongedObjet = dataToCopy[i].belongedObjet;
                    stapleObjectToEdit[i].belongedObjectMeshFilter = dataToCopy[i].belongedObjectMeshFilter;
                }
            }

            if (editTransformData)
            {
                for (int i = 0; i < stapleObjectToEdit.Count; i++)
                {
                    if (i > dataToCopy.Count - 1)
                    {
                        break;
                    }

                    stapleObjectToEdit[i].belongedTriangleIndex = dataToCopy[i].belongedTriangleIndex;
                    stapleObjectToEdit[i].positionWeight = dataToCopy[i].positionWeight;
                    stapleObjectToEdit[i].upDirection = dataToCopy[i].upDirection;
                    stapleObjectToEdit[i].vertA = dataToCopy[i].vertA;
                    stapleObjectToEdit[i].vertB = dataToCopy[i].vertB;
                    stapleObjectToEdit[i].vertC = dataToCopy[i].vertC;
                }
            }
        }

        /// <summary>
        /// Hide cut corner staple objects for step 2 of the LS Sim
        /// </summary>
        public void LSSimStepTwo()
        {
            lsSimStepTwoHideStaples.ForEach(g => g.SetActive(false));
        }
        public void LSSimStepTwo(int objIdx, int LorR)
        {
            if (lsSimStepTwoHideStaples == null || lsSimStepTwoHideStaples.Count <= 0)
            {
                Debug.Log("Error in LSSimStepTwo: no staple objects!");
                return;
            }

            string stapleGroup = (LorR == 0) ? "Left" : "Right";
            string stapleObjName = stapleGroup + objIdx.ToString(); // objIdx == 0 or 1
            int i, j;
            for (i = 0; i < lsSimStepTwoHideStaples.Count; i++)
            {
                if (lsSimStepTwoHideStaples[i].name == stapleObjName) // cut0 or cut1
                {
                    for (j = 0; j < lsSimStepTwoHideStaples[i].transform.childCount; j++)
                    {
                        lsSimStepTwoHideStaples[i].transform.GetChild(j).gameObject.SetActive(false);
                    }
                    return;
                }
            }
        }

        public void LSSimStepThree()
        {
            foreach (StapleLineObject[] ss in lsSimStepThreeShowStaples.Select(g => g.GetComponentsInChildren<StapleLineObject>(true)))
            {
                foreach (StapleLineObject s in ss)
                {
                    s.ManualUpdate();
                }
            }

            lsSimStepThreeShowStaples.ForEach(g => g.SetActive(true));
        }

        public void LSSimStepThree(int layerIdx)
        {
            if (lsSimStepThreeShowStaples == null || lsSimStepThreeShowStaples.Count <= 0)
            {
                Debug.Log("Error in LSSimStepThree: no staple objects!");
                return;
            }

            foreach (StapleLineObject[] ss in lsSimStepThreeShowStaples.Select(g => g.GetComponentsInChildren<StapleLineObject>(true)))
            {
                foreach (StapleLineObject s in ss)
                {
                    s.ManualUpdate();
                }
            }

            int endStapleIdx = 5 * layerIdx - 1;
            int i, j;
            for (i = 0; i < lsSimStepThreeShowStaples.Count; i++) // for each colon model
            {
                lsSimStepThreeShowStaples[i].SetActive(true);
                for (j = 0; j < lsSimStepThreeShowStaples[i].transform.childCount; j++)
                {
                    if (j <= endStapleIdx)
                        lsSimStepThreeShowStaples[i].transform.GetChild(j).gameObject.SetActive(true);
                    else
                        lsSimStepThreeShowStaples[i].transform.GetChild(j).gameObject.SetActive(false);
                }
            }
        }

        public void LSSimStepFour(int layer)
        {
            foreach (StapleLineObject s in lsSimStepFourShowStaplesZero[layer].GetComponentsInChildren<StapleLineObject>(true))
            {
                s.ManualUpdate();
            }
            foreach (StapleLineObject s in lsSimStepFourShowStaplesOne[layer].GetComponentsInChildren<StapleLineObject>(true))
            {
                s.ManualUpdate();
            }

            lsSimStepFourShowStaplesZero[layer].SetActive(true);
            lsSimStepFourShowStaplesOne[layer].SetActive(true);
        }

        public void LSSimStepFour(int layer, bool notShowLeft = false, bool notShowRight = false)
        {
            foreach (StapleLineObject s in lsSimStepFourShowStaplesZero[layer].GetComponentsInChildren<StapleLineObject>(true))
            {
                s.ManualUpdate();
            }
            foreach (StapleLineObject s in lsSimStepFourShowStaplesOne[layer].GetComponentsInChildren<StapleLineObject>(true))
            {
                s.ManualUpdate();
            }

            lsSimStepFourShowStaplesZero[layer].SetActive(true);
            lsSimStepFourShowStaplesOne[layer].SetActive(true);
            for (int j = 0; j < lsSimStepFourShowStaplesZero[layer].transform.childCount; j++)
            {
                // not show left-side of staples
                if (notShowLeft == true && (j < 4 || j > 38))
                    lsSimStepFourShowStaplesZero[layer].transform.GetChild(j).gameObject.SetActive(false);
                // not show right-side of staples
                if (notShowRight == true && (j >= 4 && j <= 38))
                    lsSimStepFourShowStaplesZero[layer].transform.GetChild(j).gameObject.SetActive(false);
            }
        }

        public void PaintBetweenTwoVectorsRaycast(Transform vectorAstart, Transform vectorAend, Transform vectorBstart, Transform vectorBend, Transform stapleObjectParent)
        {
            int stapleCount = 0;

            foreach (Ray r in GetRaysBetweenTwoVectors(vectorAstart.position, vectorAend.position, vectorBstart.position, vectorBend.position))
            {
                PaintStapleLineObjects(r, stapleLinePrefab, "Staple" + stapleCount.ToString(), stapleLineParent);

                stapleCount++;
            }
        }

        /// <summary>
        /// Use a circle to collide with the mesh to paint, raycast from circle side to center, paint staple objects
        /// 
        /// ### Can add algorithm to control the object spacing better
        /// </summary>
        /// <param name="circleCenter"></param> 
        /// <param name="circleVectorA"></param> One vector going from the circle's center to the circle's edge
        /// <param name="circleVectorB"></param> A none parallel vector going from ...
        /// <param name="stapleObjectParent"></param> 
        public void PaintFromCircleRaycast(Vector3 circleCenter, Vector3 circleVectorA, Vector3 circleVectorB, Transform stapleObjectParent)
        {
            int stapleCount = 0;

            foreach (Ray r in GetInvertFanRaysBetweenTwoVectors(circleCenter, circleVectorA, circleVectorB))
            {
                PaintStapleLineObjects(r, stapleLinePrefab, "Staple" + stapleCount.ToString(), stapleLineParent);

                stapleCount++;
            }
            foreach (Ray r in GetInvertFanRaysBetweenTwoVectors(circleCenter, circleVectorB, -circleVectorA))
            {
                PaintStapleLineObjects(r, stapleLinePrefab, "Staple" + stapleCount.ToString(), stapleLineParent);

                stapleCount++;
            }
            foreach (Ray r in GetInvertFanRaysBetweenTwoVectors(circleCenter, -circleVectorA, -circleVectorB))
            {
                PaintStapleLineObjects(r, stapleLinePrefab, "Staple" + stapleCount.ToString(), stapleLineParent);

                stapleCount++;
            }
            foreach (Ray r in GetInvertFanRaysBetweenTwoVectors(circleCenter, -circleVectorB, circleVectorA))
            {
                PaintStapleLineObjects(r, stapleLinePrefab, "Staple" + stapleCount.ToString(), stapleLineParent);

                stapleCount++;
            }
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

                foreach (Ray r in GetFanRaysBetweenTwoVectors(origin, beginRay, endRay))
                {
                    for (int c = 0; c < paintCount; c++)
                    {
                        SimulateMouseClickPaint(r, brush);
                    }
                }
            }
        }

        /// <summary>
        /// Use a plane to collide with the mesh to paint, then raycast on the collision points to simulate mouse click paint
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="brush"></param>
        /// <param name="brushSize"></param>
        /// <param name="brushRotation"></param>
        public IEnumerator PaintAlongPlaneCollisionAnimated(StapleLineLocator plane, P3dHitScreen brush, Vector3 brushSize, Vector3 brushRotation)
        {
            painterBrushControl.localScale = brushSize;
            painterBrushControl.eulerAngles = brushRotation;

            WaitForSeconds wait = new WaitForSeconds(animatePaintInterval);

            List<Vector3> planeVetices = new List<Vector3>(plane.GetComponent<MeshFilter>().sharedMesh.vertices);
            foreach (Vector3 v in planeVetices)
            {
                List<Vector3> neighbors = GetNeighborVertices(v, planeVetices);

                Vector3 origin = plane.transform.TransformPoint(v);
                Vector3 beginRay = plane.transform.TransformPoint(neighbors[0]) - origin;
                Vector3 endRay = plane.transform.TransformPoint(neighbors[1]) - origin;

                foreach (Ray r in GetFanRaysBetweenTwoVectors(origin, beginRay, endRay))
                {
                    for (int c = 0; c < paintCount; c++)
                    {
                        SimulateMouseClickPaint(r, brush);
                        Debug.DrawRay(r.origin, r.direction * 5, Color.white, 1f);
                    }
                    yield return wait;
                }
            }
        }

        /// <summary>
        /// Get rays between two vectors
        /// </summary>
        /// <param name="beginRayBegin"></param> Beginning position of the beginning ray
        /// <param name="beginRayEnd"></param> Ending position of the beginning ray
        /// <param name="endRayBegin"></param>
        /// <param name="endRayEnd"></param>
        /// <returns></returns>
        public List<Ray> GetRaysBetweenTwoVectors(Vector3 beginRayBegin, Vector3 beginRayEnd, Vector3 endRayBegin, Vector3 endRayEnd)
        {
            List<Ray> rays = new List<Ray>();

            Vector3 beginVector = beginRayEnd - beginRayBegin;
            Vector3 endVector = endRayEnd - endRayBegin;

            for (float t = 0; t < 1; t += paintInterval)
            {
                rays.Add(new Ray(Vector3.Lerp(beginRayBegin, endRayBegin, t), Vector3.Lerp(beginVector, endVector, t).normalized));
            }

            return rays;
        }

        /// <summary>
        /// Get the rays in a fan shape between two vectors
        /// </summary>
        /// <param name="beginVector"></param>
        /// <param name="endVector"></param>
        /// <returns></returns>
        public List<Ray> GetFanRaysBetweenTwoVectors(Vector3 rayBeginPosition, Vector3 beginVector, Vector3 endVector)
        {
            List<Ray> rays = new List<Ray>();

            for (float t = 0; t < 1; t += paintInterval)
            {
                rays.Add(new Ray(rayBeginPosition, Vector3.Lerp(beginVector, endVector, t).normalized));
            }

            return rays;
        }

        /// <summary>
        /// Get the rays in a fan shape between two vectors, invert the ray direction
        /// </summary>
        /// <param name="beginVector"></param>
        /// <param name="endVector"></param>
        /// <returns></returns>
        public List<Ray> GetInvertFanRaysBetweenTwoVectors(Vector3 rayBeginPosition, Vector3 beginVector, Vector3 endVector)
        {
            List<Ray> rays = new List<Ray>();

            for (float t = 0; t < 1; t += paintInterval)
            {
                rays.Add(new Ray(rayBeginPosition + Vector3.Lerp(beginVector, endVector, t), -Vector3.Lerp(beginVector, endVector, t).normalized));
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

        /// <summary>
        /// Paint objects to the raycast hit position
        /// </summary>
        /// <param name="hitRay"></param>
        /// <param name="staplePrefab"></param>
        /// <param name="objectName"></param>
        /// <param name="stapleParentTransform"></param>
        public void PaintStapleLineObjects(Ray hitRay, GameObject staplePrefab, string objectName, Transform stapleParentTransform)
        {
            var hit3D = default(RaycastHit);
            var finalPosition = default(Vector3);
            var finalRotation = default(Quaternion);

            // Hit 3D?
            if (Physics.Raycast(hitRay, out hit3D, float.PositiveInfinity, paintLayers))
            {
                CalcHitData(hit3D.point, hit3D.normal, hitRay, out finalPosition, out finalRotation);

                GameObject staple = Instantiate(staplePrefab);
                staple.name = objectName;

                // Setup staple object
                StapleLineObject stapleData = staple.GetComponent<StapleLineObject>();
                stapleData.belongedObjet = hit3D.transform;
                stapleData.belongedObjectMeshFilter = hit3D.transform.GetComponent<MeshFilter>();
                stapleData.belongedTriangleIndex = hit3D.triangleIndex;
                stapleData.upDirection = stapleObjectUpDirection;

                staple.transform.position = hit3D.point;
                staple.transform.LookAt(hit3D.point + hit3D.normal, stapleObjectUpDirection);
                staple.transform.parent = stapleParentTransform;

                GetStapleLineObjectTriangularWeight(stapleData, stapleData.GetVertPosition(stapleData.belongedTriangleIndex * 3), stapleData.GetVertPosition(stapleData.belongedTriangleIndex * 3 + 1), stapleData.GetVertPosition(stapleData.belongedTriangleIndex * 3 + 2));
            }
        }

        /// <summary>
        /// Get and store the triangular interpolation weight for the given staple object based on the vertices of its belonged triangle
        /// Triangle vertices also need to be in world coord
        /// </summary>
        /// <param name="staple"></param>
        /// <param name="vertexA"></param>
        /// <param name="vertexB"></param>
        /// <param name="vertexC"></param>
        public void GetStapleLineObjectTriangularWeight(StapleLineObject staple, Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            // calculate vectors from point f to vertices p1, p2 and p3:
            var f1 = vertexA - staple.transform.position;
            var f2 = vertexB - staple.transform.position;
            var f3 = vertexC - staple.transform.position;
            // calculate the areas and factors (order of parameters doesn't matter):
            var a = Vector3.Cross(vertexA - vertexB, vertexA - vertexC).magnitude; // main triangle area a
            staple.positionWeight.x = Vector3.Cross(f2, f3).magnitude / a; // p1's triangle area / a
            staple.positionWeight.y = Vector3.Cross(f3, f1).magnitude / a; // p2's triangle area / a 
            staple.positionWeight.z = Vector3.Cross(f1, f2).magnitude / a; // p3's triangle area / a
                                                                           // find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):

            //// ### Test
            //if (staple.gameObject.name == "Staple0")
            //{
            //    GameObject vertexAO = new GameObject();
            //    vertexAO.name = "vA";
            //    vertexAO.transform.parent = stapleLineParent;
            //    vertexAO.transform.position = vertexA;
            //    GameObject vertexBO = new GameObject();
            //    vertexBO.name = "vB";
            //    vertexBO.transform.parent = stapleLineParent;
            //    vertexBO.transform.position = vertexB;
            //    GameObject vertexCO = new GameObject();
            //    vertexCO.name = "vC";
            //    vertexCO.transform.parent = stapleLineParent;
            //    vertexCO.transform.position = vertexC;
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hitRay"></param>
        /// <param name="brush"></param>
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

public class GFG
{
    public static double[] find_Centroid(double[,] v)
    {
        double[] ans = new double[2];

        int n = v.GetLength(0);
        double signedArea = 0;

        // For all vertices 
        for (int i = 0; i < n; i++)
        {
            double x0 = v[i, 0], y0 = v[i, 1];
            double x1 = v[(i + 1) % n, 0],
                    y1 = v[(i + 1) % n, 1];

            // Calculate value of A 
            // using shoelace formula 
            double A = (x0 * y1) - (x1 * y0);
            signedArea += A;

            // Calculating coordinates of 
            // centroid of polygon 
            ans[0] += (x0 + x1) * A;
            ans[1] += (y0 + y1) * A;
        }
        signedArea *= 0.5;
        ans[0] = (ans[0]) / (6 * signedArea);
        ans[1] = (ans[1]) / (6 * signedArea);

        return ans;
    }
}
// https://www.geeksforgeeks.org/find-the-centroid-of-a-non-self-intersecting-closed-polygon/
// This code is contributed by PrinciRaj1992