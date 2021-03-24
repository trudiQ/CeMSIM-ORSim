using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;//IJPFor use
using Unity.Jobs;//IJPFor use
using System;
using System.Linq;

[ExecuteInEditMode] //allow start() to run in both edit/play mode
public class colonMesh : MonoBehaviour
{
    Mesh originalMesh;
    Mesh clonedMesh;
    MeshFilter meshFilter;
    int[] triangles;

    [HideInInspector]
    public List<int> triIOLayerInfo = new List<int>(); // length: #triangles

    [HideInInspector]
    public int sphereJointObjIdx = -1;
    public string sphereJointObjName = " ";
    public int sphereJointObjIdx2Join = -1;
    [HideInInspector]
    sphereJointModel physicsModel;
    [HideInInspector]
    public sphereJointModel physicsModel2Join;

    [HideInInspector]
    public Vector3[] vertices;

    [HideInInspector]
    public Vector3[] verticesProj;

    [HideInInspector]
    public List<List<List<int>>> neighborSpheres = new List<List<List<int>>>(); // vertex: 4 sphere indices (layer,sphereIdx per layer, objIdx)

    [HideInInspector]
    public List<List<float>> utCoords = new List<List<float>>(); // vertex: (u, t) coord w.r.t. 4 neighbor spheres

    public bool isBound = false;
    public int verticesNum;

    // handle duplicate vertices
    public bool bRemoveDuplicate = false;
    [HideInInspector]
    public Dictionary<int, List<int>> verDupMap = new Dictionary<int, List<int>>(); // key: vertex -> values: duplicates
    [HideInInspector]
    public List<bool> bDuplicate = new List<bool>(); // dup tag for each vertex in vertices

    [HideInInspector]
    public bool isCloned = false;

    // For Editor
    public bool moveVertexPoint = true;
    public float handleSize = 0.03f;
    public float pickSize = 0.03f;

    [System.Serializable]
    public class selectedVertices
    {
        public List<int> indices = new List<int>();
        public List<Vector3> positions = new List<Vector3>();
    }
    public selectedVertices seleVertexList = new selectedVertices();

    [HideInInspector]
    public List<int> splitVertexList = new List<int>();

    [HideInInspector]
    public List<int> affectedVertexList = new List<int>(); // vertices are within split sphere quads but not split
    [HideInInspector]
    public List<int> affectedVerTagList = new List<int>(); // above(1)/below(-1) tags of the above vertices list

    [HideInInspector]
    Dictionary<int, List<int>> splitFaceList = new Dictionary<int, List<int>>();
    [HideInInspector]
    public List<int> splitFaceTagList = new List<int>(); // 1: above split line; -1: below

    [HideInInspector]
    public bool bFullSplit = false;
    [HideInInspector]
    public Dictionary<string, int> startEndSplitVers = new Dictionary<string, int>(); // "start_outer"/"end_outer"/"start_inner"/"end_inner": vertex index (within 'vertices')
    [HideInInspector]
    public Dictionary<string, Vector3> endSplitVers2Join = new Dictionary<string, Vector3>(); // "end_outer" or "end_inner" vertices positions of the colon to join

    // functions access member variables
    public Vector3 getNormal(int vIdx)
    {
        return clonedMesh.normals[vIdx];
    }
    // Start is called before the first frame update
    void Start()
    {
        InitMesh();
        
        if (Application.isPlaying)
        {
            // examine and handle duplicated vertices
            if (!bRemoveDuplicate && verDupMap.Count <= 0)
            {
                if (!checkDuplicateVertices())
                {
                    removeDuplicateVertices();
                    bRemoveDuplicate = true;
                }
                Debug.Log("Duplicated already handled");
            }
            // check inner/outer layer info
            if (triIOLayerInfo.Count <= 0)
            {
                evaluateTriIOLayerInfo();
                Debug.Log("Done build inner/outer layer info");
            }
            // bind colon mesh to sphereJointModel
            if (!isBound && neighborSpheres.Count == 0)
            {
                GameObject sphereJointObj = GameObject.Find(sphereJointObjName);//"sphereJointGameObj0"
                if (sphereJointObj)
                {
                    physicsModel = sphereJointObj.GetComponent<sphereJointModel>();
                    if (physicsModel)
                    {
                        if (neighborSphereSearching1(physicsModel))
                        {
                            bindSphereJointModel();
                            isBound = true;
                        }
                    }
                }
                Debug.Log("Binding sphereJointModel done!");
            }
        }
    }

    public void InitMesh()
    {   
        // avoid overwritting built-in shapes, clone it first
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh; //get the mesh originally assigned to meshFilter
        clonedMesh = new Mesh(); //create a new mesh instance

        clonedMesh.name = "clone";
        clonedMesh.vertices = originalMesh.vertices;
        clonedMesh.triangles = originalMesh.triangles;
        clonedMesh.normals = originalMesh.normals;
        clonedMesh.uv = originalMesh.uv;
        meshFilter.mesh = clonedMesh;  //assign the cloned mesh to mesh filter

        vertices = clonedMesh.vertices; //update local variables
        triangles = clonedMesh.triangles;
        isCloned = true; //indicate the mesh assigned to mesh filter is cloned

        verticesProj = vertices;

        //Debug.Log("Init & Cloned");
    }

    // determine mesh vertex within line segment formed by neighboring spheres of the sam layer
      // using 3D point-line segment projection
      // Return true: vertex is within line seg (>=S1, <s2) and its projection
      //        false: vertex is outside line seg (<s1, >=s2)
    public bool pointLineSegmentProj (Vector3 vertex, Vector3 s1, Vector3 s2, ref float t/*Vector3 v_p*/)
    {
        Vector3 s12 = s2 - s1;
        float squaredDist12 = Vector3.Dot(s12, s12); // squared dist from s1 to s2
        if (squaredDist12 == 0.0)
        {
            Debug.Log("Error pointLineSegmentProj: line seg is a point!");
            return false;
        }
        // check if the proj of vertex is within the ling seg s12
        Vector3 s1Vertex = vertex - s1;
        t = Vector3.Dot(s1Vertex, s12) / squaredDist12;
        if (t < 0.0) // before s1
        {
            return false;
        }
        else if (t > 1.0) // after or on s2
        {
            return false;
        }
        // inbetween s12
        //v_p = s1 + t * s12;
        return true;
    }

    // fill out neighborSpheres
    public bool neighborSphereSearching(sphereJointModel sphereJointModel)
    {
        int layerNum = sphereJointModel.m_numLayers;
        int sphereNum = sphereJointModel.m_numSpheres;
        float layerThick = sphereJointModel.m_layerThickness;
        // search for neighborSpheres for each vertex
        bool bAllFound = true;
        bool bProj1, bProj2;
        int on1or2;
        int count = 0, count1 = 0, count2 = 0, count3 = 0;
        int layerIdx;
        Vector3 spherePos1, spherePos2, spherePos3;
        float zMin = sphereJointModel.getOriginalSpherePos(0,0).z;
        verticesNum = vertices.Length;
        Vector3 vertex, vertexZ, vertexProj, vertexProjZ; ; // vertex in world space
        float t1, t2, dist;
        float minDist = 2.0f * sphereJointModel.m_circleRadius;
        int[] sphereIdices = { 0, 0 };
        int[] tmpSphereIdices = { 0, 0 };
        float[] utCoord = { -1.0f, -1.0f }; // (0, 1)
        float[] tmpUtCoord = { -1.0f, -1.0f };
        foreach (Vector3 vertexL in vertices)
        {
            vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
            vertexProj = new Vector3(0.0f, 0.0f, 0.0f);

            layerIdx = (int)((vertex.z - zMin) / layerThick); // determine layer indices
            if (layerIdx < 0 || layerIdx >= (layerNum - 1)) //vertex is not within spherejoint model z range
            {
                Debug.Log("Error1 in 'neighborSphereSearching'!");
                continue;
            }
            
            neighborSpheres.Add(new List<List<int>>());
            utCoords.Add(new List<float>());
            // within layer of layerIdx
            minDist = 2.0f * sphereJointModel.m_circleRadius;
            sphereIdices[0] = -1; sphereIdices[1] = -1;
            utCoord[0] = -1.0f; utCoord[1] = -1.0f;
            for (count1 = 0; count1 < sphereNum; count1++)
            {
                on1or2 = -1;
                // obtain the two adjacent spheres of the same layer
                spherePos1 = sphereJointModel.getOriginalSpherePos(layerIdx, count1);
                count2 = count1 + 1;
                if (count2 >= sphereNum)
                    count2 -= sphereNum;
                spherePos2 = sphereJointModel.getOriginalSpherePos(layerIdx, count2);
                count3 = count1 + 2;
                if (count3 >= sphereNum)
                    count3 -= sphereNum;
                spherePos3 = sphereJointModel.getOriginalSpherePos(layerIdx, count3);

                // project to two segments
                t1 = 0.0f; t2 = 0.0f;
                vertexZ = new Vector3(vertex.x, vertex.y, spherePos1.z);
                bProj1 = pointLineSegmentProj(vertexZ, spherePos1, spherePos2, ref t1);
                bProj2 = pointLineSegmentProj(vertexZ, spherePos2, spherePos3, ref t2);

                if (bProj1) // bProj1 = true, bProj2 = true/false
                    on1or2 = 1;
                else
                {
                    if (bProj2) // bProj1 = false, bProj2 = true;
                        on1or2 = 2;
                    else // bProj1 = false, bProj2 = false
                    {
                        if (t1 > 1.0f && t2 < 0.0f) // at the conjunction of two line segments
                        {
                            t1 = 1.0f;
                            on1or2 = 1;
                        }
                    }
                }
                // if the projection is on one of the line segments, calculate the exact projection
                vertexProjZ = new Vector3(0.0f, 0.0f, 0.0f);
                if (on1or2 > 0)
                {
                    if (on1or2 == 1)
                    {
                        vertexProjZ = spherePos1 + t1 * (spherePos2 - spherePos1);
                        tmpSphereIdices[0] = count1;
                        tmpSphereIdices[1] = count2;
                        tmpUtCoord[1] = t1;
                        tmpUtCoord[0] = (vertex.z - spherePos1.z) / layerThick;
                    }
                    else // on1or2 == 2
                    {
                        vertexProjZ = spherePos2 + t2 * (spherePos3 - spherePos2);
                        tmpSphereIdices[0] = count2;
                        tmpSphereIdices[1] = count3;
                        tmpUtCoord[1] = t2;
                        tmpUtCoord[0] = (vertex.z - spherePos2.z) / layerThick;
                    }
                    
                    dist = Vector3.Distance(vertex, vertexProjZ);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        sphereIdices[0] = tmpSphereIdices[0];
                        sphereIdices[1] = tmpSphereIdices[1];
                        vertexProj = vertexProjZ;
                        vertexProj.z = vertex.z;
                        utCoord[0] = tmpUtCoord[0];
                        utCoord[1] = tmpUtCoord[1];
                    }
                }
            }

            // check if four neighbors are found
            if (sphereIdices[0] != -1 && sphereIdices[1] != -1)
            {
                neighborSpheres[count].Add(new List<int> { layerIdx, sphereIdices[0], sphereJointObjIdx });
                neighborSpheres[count].Add(new List<int> { layerIdx, sphereIdices[1], sphereJointObjIdx });
                neighborSpheres[count].Add(new List<int> { layerIdx + 1, sphereIdices[0], sphereJointObjIdx });
                neighborSpheres[count].Add(new List<int> { layerIdx + 1, sphereIdices[1], sphereJointObjIdx });
                verticesProj[count] = this.transform.InverseTransformPoint(vertexProj);
                // ut coordinates w.r.t. the neighbor spheres
                utCoords[count].Add(utCoord[0]);
                utCoords[count].Add(utCoord[1]);
                // debug use
                if (utCoord[0] < 0.0 || utCoord[0] > 1.0)
                    Debug.Log("vertex " + count.ToString() + " :u: " + utCoord[0].ToString());
                if (utCoord[1] < 0.0 || utCoord[1] > 1.0)
                    Debug.Log("vertex " + count.ToString() + " :t: " + utCoord[1].ToString());
                //
            }
            else
            {
                bAllFound = false;
                Debug.Log("Vertex " + count.ToString() + "cannot find neighbors!");
                // add the such vertices in selection list for display
                seleVertexList.indices.Add(count);
                seleVertexList.positions.Add(vertex);
            }
            count++;
        }

        if (!bAllFound)
        {
            // clear neighborSpheres
            neighborSpheres.Clear();
            utCoords.Clear();
            Debug.Log("colonMesh.NeighborhoodSphereSearching: not all 4 neighbors found!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Description: Search 4 neighboring spheres for each mesh vertex and record utCoordinates 
    /// Procedure:
    ///     1. Make sure the vertices of each triangle are mapped to consecutive layers ((max-min) <= 1) of sphere-quads
    ///     2. record neighborSpheres based on the updated layers of mesh vertices
    ///     3. record utCoords
    /// </summary>
    /// <returns> neighborSpheres; utCoords </returns>
    public bool neighborSphereSearching1(sphereJointModel sphereJointModel)
    {
        int layerNum = sphereJointModel.m_numLayers;
        int sphereNum = sphereJointModel.m_numSpheres;
        float layerThick = sphereJointModel.m_layerThickness;
        verticesNum = vertices.Length;

        // Traverse vertices, record each vertex's original corresponding layerIdx and position along layer direction, i.e., z
        int layerIdx, count = 0;
        float zMin = sphereJointModel.getOriginalSpherePos(0, 0).z;
        Vector3 vertex;
        int[] layerIdxList = new int[verticesNum];
        float[] vertexZList = new float[verticesNum];
        foreach (Vector3 vertexL in vertices)
        {
            vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
            layerIdx = (int)((vertex.z - zMin) / layerThick); // determine layer indices
            if (layerIdx < 0 || layerIdx >= (layerNum - 1)) //vertex is not within spherejoint model z range
            {
                Debug.Log("Error1 in 'neighborSphereSearching'!");
                continue;
            }
            // record 
            layerIdxList[count] = layerIdx;
            vertexZList[count] = vertex.z;
            count++;
        }

        // Traverse triangles, check if the vertices of each triangle span no more than 2 sphere-quads
        //  if yes, alter the vertex (the one with largest layerIdx) causing the issue in layerIdxList and vertexZList
        int i, j, k, dictCount = 1;
        int fLayerMin, fLayerMax, tempVer;
        int alterLayerCount = 0, alterVerCount = 0;
        int[] vLayers = { -1, -1, -1 };
        bool[] vertexAltered = new bool[verticesNum];
        int[] verNewIdx = new int[verticesNum];
        List<int> tempVersInDict = new List<int>();
        var dict = new SortedDictionary<int, List<int>>(); // key: layerIdx, List: vertices
        for (i = 0; i < vertexAltered.Length; i++)
        {
            vertexAltered[i] = false;
            verNewIdx[i] = -1;
        }
        while (dictCount > 0)
        {
            // record the vertices should be altered first
            for (i = 0; i < triangles.Length; i += 3)
            {
                for (j = 0; j < 3; j++)
                {
                    vLayers[j] = layerIdxList[triangles[i + j]];
                }
                fLayerMin = vLayers.Min();
                fLayerMax = vLayers.Max();
                if ((fLayerMax - fLayerMin) <= 1) 
                    continue;

                // current triangle spans more than 2 sphere-quads ((fLayerMax - fLayerMin) > 1)
                for (j = 0; j < 3; j++)
                {
                    if (vLayers[j] == fLayerMax)
                    {
                        // the vertex not visisted before OR the vertex's new index needs update
                        if ((verNewIdx[triangles[i + j]] == -1) || (verNewIdx[triangles[i + j]] > (fLayerMin + 1)))
                        {
                            verNewIdx[triangles[i + j]] = fLayerMin + 1;
                            if (!dict.ContainsKey(vLayers[j]))
                                dict.Add(vLayers[j], new List<int>());
                            if (!dict[vLayers[j]].Contains(triangles[i + j]))
                                dict[vLayers[j]].Add(triangles[i + j]);
                        }
                    }

                }
            }

            // only alter the vertices with the SMALLEST layerIdx recorded in dict
            if (dict.Count() > 0)
            {
                // alter vertices in the FIRST (smallest layerIdx) item of dict 
                tempVersInDict = dict.First().Value;
                for (k = 0; k < tempVersInDict.Count(); k++)
                {
                    tempVer = tempVersInDict[k];
                    if (!vertexAltered[tempVer])
                    {
                        layerIdxList[tempVer] = verNewIdx[tempVer];
                        vertexZList[tempVer] = (float)(verNewIdx[tempVer] + 1) * layerThick + zMin - 0.01f;
                        // debug use
                        layerIdx = (int)((vertexZList[tempVer] - zMin) / layerThick);
                        if (layerIdx != layerIdxList[tempVer])
                        {
                            Debug.Log("Error0 in 'neighborSphereSearching1'");
                            return false;
                        }
                        //
                        vertexAltered[tempVer] = true;
                    }
                    else // any vertex should be only altered once at most
                    {
                        Debug.Log("Error1 in 'neighborSphereSearching1'");
                        return false;
                    }
                }
                alterLayerCount += 1;
                alterVerCount += tempVersInDict.Count();
                dict.Remove(dict.First().Key);
            }

            dictCount = dict.Count();
        }
        Debug.Log("'neighborSphereSearching1' for outer" + sphereJointObjIdx.ToString() + ": " +
                  alterVerCount.ToString() + " vertices of " + alterLayerCount.ToString() + " layers altered!" ); // debug use only
       
        // Search all the four neigbhboring spheres for each mesh vertex and record their UT coords
        bool bAllFound = true;
        bool bProj1, bProj2;
        int on1or2;
        int count1 = 0, count2 = 0, count3 = 0;
        Vector3 spherePos1, spherePos2, spherePos3;
        Vector3 vertexZ, vertexProj, vertexProjZ; ; // vertex in world space
        float t1, t2, dist;
        float minDist = 2.0f * sphereJointModel.m_circleRadius;
        int[] sphereIdices = { 0, 0 };
        int[] tmpSphereIdices = { 0, 0 };
        float[] utCoord = { -1.0f, -1.0f }; // (0, 1)
        float[] tmpUtCoord = { -1.0f, -1.0f };
        count = 0;
        foreach (Vector3 vertexL in vertices)
        {
            vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
            vertexProj = new Vector3(0.0f, 0.0f, 0.0f);

            layerIdx = layerIdxList[count];// retrive the processed layer indices 
            if (layerIdx < 0 || layerIdx >= (layerNum - 1)) //vertex is not within spherejoint model z range
            {
                Debug.Log("Error1 in 'neighborSphereSearching'!");
                continue;
            }

            neighborSpheres.Add(new List<List<int>>());
            utCoords.Add(new List<float>());
            // within layer of layerIdx
            minDist = 2.0f * sphereJointModel.m_circleRadius;
            sphereIdices[0] = -1; sphereIdices[1] = -1;
            utCoord[0] = -1.0f; utCoord[1] = -1.0f;
            for (count1 = 0; count1 < sphereNum; count1++)
            {
                on1or2 = -1;
                // obtain the two adjacent spheres of the same layer
                spherePos1 = sphereJointModel.getOriginalSpherePos(layerIdx, count1);
                count2 = count1 + 1;
                if (count2 >= sphereNum)
                    count2 -= sphereNum;
                spherePos2 = sphereJointModel.getOriginalSpherePos(layerIdx, count2);
                count3 = count1 + 2;
                if (count3 >= sphereNum)
                    count3 -= sphereNum;
                spherePos3 = sphereJointModel.getOriginalSpherePos(layerIdx, count3);

                // project to two segments
                t1 = 0.0f; t2 = 0.0f;
                vertexZ = new Vector3(vertex.x, vertex.y, spherePos1.z);
                bProj1 = pointLineSegmentProj(vertexZ, spherePos1, spherePos2, ref t1);
                bProj2 = pointLineSegmentProj(vertexZ, spherePos2, spherePos3, ref t2);

                if (bProj1) // bProj1 = true, bProj2 = true/false
                    on1or2 = 1;
                else
                {
                    if (bProj2) // bProj1 = false, bProj2 = true;
                        on1or2 = 2;
                    else // bProj1 = false, bProj2 = false
                    {
                        if (t1 > 1.0f && t2 < 0.0f) // at the conjunction of two line segments
                        {
                            t1 = 1.0f;
                            on1or2 = 1;
                        }
                    }
                }
                // if the projection is on one of the line segments, calculate the exact projection
                vertexProjZ = new Vector3(0.0f, 0.0f, 0.0f);
                if (on1or2 > 0)
                {
                    if (on1or2 == 1)
                    {
                        vertexProjZ = spherePos1 + t1 * (spherePos2 - spherePos1);
                        tmpSphereIdices[0] = count1;
                        tmpSphereIdices[1] = count2;
                        tmpUtCoord[1] = t1;
                        tmpUtCoord[0] = (vertexZList[count] - spherePos1.z) / layerThick;
                    }
                    else // on1or2 == 2
                    {
                        vertexProjZ = spherePos2 + t2 * (spherePos3 - spherePos2);
                        tmpSphereIdices[0] = count2;
                        tmpSphereIdices[1] = count3;
                        tmpUtCoord[1] = t2;
                        tmpUtCoord[0] = (vertexZList[count] - spherePos2.z) / layerThick;
                    }

                    dist = Vector3.Distance(new Vector3(vertex.x, vertex.y, vertexZList[count]), vertexProjZ);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        sphereIdices[0] = tmpSphereIdices[0];
                        sphereIdices[1] = tmpSphereIdices[1];
                        vertexProj = vertexProjZ;
                        vertexProj.z = vertexZList[count];
                        utCoord[0] = tmpUtCoord[0];
                        utCoord[1] = tmpUtCoord[1];
                    }
                }
            }

            // check if four neighbors are found
            if (sphereIdices[0] != -1 && sphereIdices[1] != -1)
            {
                neighborSpheres[count].Add(new List<int> { layerIdx, sphereIdices[0], sphereJointObjIdx });
                neighborSpheres[count].Add(new List<int> { layerIdx, sphereIdices[1], sphereJointObjIdx });
                neighborSpheres[count].Add(new List<int> { layerIdx + 1, sphereIdices[0], sphereJointObjIdx });
                neighborSpheres[count].Add(new List<int> { layerIdx + 1, sphereIdices[1], sphereJointObjIdx });
                verticesProj[count] = this.transform.InverseTransformPoint(vertexProj);
                // ut coordinates w.r.t. the neighbor spheres
                utCoords[count].Add(utCoord[0]);
                utCoords[count].Add(utCoord[1]);
                // debug use
                if (utCoord[0] < 0.0 || utCoord[0] > 1.0)
                    Debug.Log("vertex " + count.ToString() + " :u: " + utCoord[0].ToString());
                if (utCoord[1] < 0.0 || utCoord[1] > 1.0)
                    Debug.Log("vertex " + count.ToString() + " :t: " + utCoord[1].ToString());
                //
            }
            else
            {
                bAllFound = false;
                Debug.Log("Vertex " + count.ToString() + "cannot find neighbors!");
                // add the such vertices in selection list for display
                seleVertexList.indices.Add(count);
                seleVertexList.positions.Add(vertex);
            }
            count++;
        }

        // clear temp data
        Array.Clear(layerIdxList, 0, layerIdxList.Length);
        Array.Clear(vertexZList, 0, vertexZList.Length);
        Array.Clear(vertexAltered, 0, vertexAltered.Length);
        Array.Clear(verNewIdx, 0, verNewIdx.Length);
        tempVersInDict.Clear();
        dict.Clear();

        // check if succeed 
        if (!bAllFound)
        {
            // clear neighborSpheres
            neighborSpheres.Clear();
            utCoords.Clear();
            Debug.Log("colonMesh.NeighborhoodSphereSearching: not all 4 neighbors found!");
            return false;
        }

        Debug.Log("'NeighborSphereSearching1' Done for" + "outer" + sphereJointObjIdx.ToString());
        return true;
    }

    public void clearNeighbors()
    {
       neighborSpheres.Clear();
       utCoords.Clear();
       isBound = false;
    }
    public void Reset()
    {
        if (clonedMesh != null && originalMesh != null) //make sure both original and cloned meshes exist
        {
            clonedMesh.vertices = originalMesh.vertices; //reset properties of cloned mesh to those of the original mesh
            clonedMesh.triangles = originalMesh.triangles;
            clonedMesh.normals = originalMesh.normals;
            clonedMesh.uv = originalMesh.uv;
            meshFilter.mesh = clonedMesh; //assign cloned mesh back to the Mesh Filter component (the mesh being drawn?)

            vertices = clonedMesh.vertices; //upadate local variables
            triangles = clonedMesh.triangles;
        }
        //ClearAllData();
    }

    public void bindSphereJointModel()
    {
        if (neighborSpheres.Count <= 0)
        {
            Debug.Log("Error in bindSphereJointModel: no neighborSpheres recorded!");
            return;
        }

        vertices = verticesProj;
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    public void unbindSphereJointModel()
    {
        Reset();
        verticesProj = vertices;
    }

    public void DoAction(int index, Vector3 localPos)
    {
        // pull a single mesh vertex
        //PullOneVertex(index, localPos);

        // pull all vertices sharing the same position
        PullSimilarVertices(index, localPos);
    }

    // returns List of int that is related to the targetPt.
    private List<int> FindRelatedVertices(Vector3 targetPt, bool findConnected)
    {
        // list of int
        List<int> relatedVertices = new List<int>();

        int idx = 0;
        Vector3 pos;

        // loop through triangle array of indices
        for (int t = 0; t < triangles.Length; t++)
        {
            // current idx return from tris
            idx = triangles[t];
            // current pos of the vertex
            pos = vertices[idx];
            // if current pos is same as targetPt
            if (pos == targetPt)
            {
                // add to list
                relatedVertices.Add(idx);
                // if find connected vertices
                if (findConnected)
                {
                    // min
                    // - prevent running out of count
                    if (t == 0)
                    {
                        relatedVertices.Add(triangles[t + 1]);
                    }
                    // max 
                    // - prevent runnign out of count
                    if (t == triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                    }
                    // between 1 ~ max-1 
                    // - add idx from triangles before t and after t 
                    if (t > 0 && t < triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                        relatedVertices.Add(triangles[t + 1]);
                    }
                }
            }
        }
        // return compiled list of int
        return relatedVertices;
    }

    public void BuildTriangleList()
    {
    }

    public void ShowTriangle(int idx)
    {
    }

    // Pulling only one vertex pt, results in broken mesh.
    private void PullOneVertex(int index, Vector3 newPos)
    {
        vertices[index] = newPos; //update the vertex to new position
        clonedMesh.vertices = vertices; //assign the updated vertices back to the cloned mesh
        clonedMesh.RecalculateNormals(); // re-draw the mesh to reflect the change
    }

    private void PullSimilarVertices(int index, Vector3 newPos)
    {
        Vector3 targetVertexPos = vertices[index]; //get the target vertex position 
        List<int> relatedVertices = FindRelatedVertices(targetVertexPos, false); // find all vertices sharing the same position
        foreach (int i in relatedVertices) //update the positions of all related vertices
        {
            vertices[i] = newPos;
        }
        clonedMesh.vertices = vertices; //assign updated vertices back to cloned mesh and redraw it
        clonedMesh.RecalculateNormals();
    }

    // To test Reset function
    public void EditMesh()
    {
        vertices[2] = new Vector3(2, 3, 4);
        vertices[3] = new Vector3(1, 2, 4);
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    // Clear all selected vertices
    public void ClearAllData()
    {
        // triangle inner/outer layer info
        triIOLayerInfo.Clear();
        // vertex selection
        seleVertexList.indices.Clear();
        seleVertexList.positions.Clear();
        // duplicate handling
        verDupMap.Clear();
        bDuplicate.Clear();
        // split
        splitVertexList.Clear();
        affectedVertexList.Clear();
        affectedVerTagList.Clear();
        splitFaceList.Clear();
        splitFaceTagList.Clear();
        startEndSplitVers.Clear();
        endSplitVers2Join.Clear();
    }

    // Deform colon mesh based on sphereJointModel 
    //     (discard due to performance issue caused by "Find" spheres for every frame)
    public void deform(int objIdx)
    {
        if (neighborSpheres.Count <= 0 || utCoords.Count <= 0)
        {
            Debug.Log("Error in deform: neighborspheres or utCoords is empty!");
            return;
        }

        int count1 = 0;
        int layerIdx, sIdx;
        Vector3 newPosWorld, newPosLocal;
        Vector3[] spherePos = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f) };
        GameObject sphere;
        for (int count = 0; count < vertices.Length; count++ )
        {
            //vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
            for (count1 = 0; count1 < 4; count1 ++)
            {
                layerIdx = neighborSpheres[count][count1][0];
                sIdx = neighborSpheres[count][count1][1];
                if (layerIdx != -1 && sIdx != -1)
                {
                    sphere = GameObject.Find("sphere_" + objIdx.ToString() + "_" + layerIdx.ToString() + "_" + sIdx.ToString());
                    if (sphere)
                        spherePos[count1] = sphere.transform.position;
                    else
                    {
                        Debug.Log("Error in deform: cannot find sphere_" + objIdx.ToString() + "_" + layerIdx.ToString() + "_" + sIdx.ToString());
                        Debug.Log("Deform terminated...");
                        return;
                    }
                }
                else
                {
                    Debug.Log("Error in deform: invalid layerIdx or sIdx!");
                    return;
                }
            }

            // interpolate mesh vertex position based on ut coord
            if (utCoords[count][0] < 0.0f || utCoords[count][0] > 1.0f ||
                utCoords[count][1] < 0.0f || utCoords[count][1] > 1.0f)
            {
                Debug.Log("Error in deform: utCoords[" + count.ToString() + "] invalid");
                return;
            }

            newPosWorld = spherePos[0] + utCoords[count][1] * (spherePos[1] - spherePos[0]) + utCoords[count][0] * (spherePos[2] - spherePos[0]);
            newPosLocal = this.transform.InverseTransformPoint(newPosWorld);
            vertices[count] = newPosLocal;
        }

        // update clone mesh and normals
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    // Deform colon mesh based on sphereJointModel by reading sphere positions directly from the physics model
    public void deform(int physSpheNum, Vector3[] spherePosList, int physSpheNum2Join, Vector3[] spherePosList2Join)
    {
        if (neighborSpheres.Count <= 0 || utCoords.Count <= 0)
        {
            Debug.Log("Error in deform: neighborspheres or utCoords is empty!");
            return;
        }

        int count1 = 0;
        int layerIdx, sIdx, objIdx;
        Vector3 newPosWorld, newPosLocal;
        Vector3[] spherePos = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f) };
        for (int count = 0; count < vertices.Length; count++)
        {
            //vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
            for (count1 = 0; count1 < 4; count1 ++)
            {
                layerIdx = neighborSpheres[count][count1][0];
                sIdx = neighborSpheres[count][count1][1];
                objIdx = neighborSpheres[count][count1][2];
                if (layerIdx != -1 && sIdx != -1)
                {
                    if (objIdx == sphereJointObjIdx)
                        spherePos[count1] = spherePosList[layerIdx * physSpheNum + sIdx];
                    else if (objIdx == sphereJointObjIdx2Join)
                        spherePos[count1] = spherePosList2Join[layerIdx * physSpheNum2Join + sIdx];
                }
                else
                {
                    Debug.Log("Error in deform: invalid layerIdx or sIdx!");
                    return;
                }
            }

            // interpolate mesh vertex position based on ut coord
            if (utCoords[count][0] < 0.0f || utCoords[count][0] > 1.0f ||
                utCoords[count][1] < 0.0f || utCoords[count][1] > 1.0f)
            {
                Debug.Log("Error in deform: utCoords[" + count.ToString() + "] invalid");
                return;
            }

            newPosWorld = spherePos[0] + utCoords[count][1] * (spherePos[1] - spherePos[0]) + utCoords[count][0] * (spherePos[2] - spherePos[0]);
            newPosLocal = this.transform.InverseTransformPoint(newPosWorld);

            //// end-split vertices handling
            //if (!bFullSplit && startEndSplitVers.Count > 0 && endSplitVers2Join.Count > 0)
            //{
            //    if (count == startEndSplitVers["end_outer"])
            //    {
            //        newPosLocal = 0.5f * (newPosLocal + endSplitVers2Join["end_outer"]);
            //    }
            //    else if (count == startEndSplitVers["end_inner"])
            //    {
            //        newPosLocal = 0.5f * (newPosLocal + endSplitVers2Join["end_inner"]);
            //    }
            //}

            vertices[count] = newPosLocal;
        }

        // update clone mesh and normals
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    /// Parallelize the orignal "Deform" function 
    ///   Discared due to several issues within 'Execute':
    ///    1) not support GameObject.Find, which can only be done in mainthread
    ///    2) all the input NativeArrays should have consistent index with 'i' 
    // Create a parallel job using IJPFor
    struct vertexPosUpdateJob : IJobParallelFor
    {
        // Jobs declare all data that will be accessed in the job
        public int sphereJointObjIdx_IJPFor;
        public NativeArray<float> utCoords_IJPFor; // ~'utCoords', 1D array: [2*vertices.Length]
        public NativeArray<int> neighborSpheresLayIdx_IJPFor; // ~'neighborSpheres' layer info, 1D array: [4*vertices.Length]
        public NativeArray<int> neighborSpheresSphIdx_IJPFor; // ~'neighborSpheres' sphere info, 1D array: [4*vertices.Length]
        public NativeArray<Vector3> vertices_IJPFor; // ~'vertices' 1D array: [vertices.Length]

        // the code actually running on the job: update each vertex position
        public void Execute(int i)
        {
            GameObject sphere;
            Vector3[] spherePos = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f) };
            for (int count1 = 0; count1 < 4; count1++)
            {
                //sphere = GameObject.Find("sphere_" + sphereJointObjIdx_IJPFor.ToString() + "_" 
                //                                                + neighborSpheresLayIdx_IJPFor[4 * i + count1].ToString() + "_" 
                //                                                    + neighborSpheresSphIdx_IJPFor[4 * i + count1].ToString());
                //spherePos[count1] = sphere.transform.position;
            }
            Vector3 newPosWorld = spherePos[0] + utCoords_IJPFor[2*i+1] * (spherePos[1] - spherePos[0]) + utCoords_IJPFor[2*i+0] * (spherePos[2] - spherePos[0]);
            //Vector3 newPosLocal = transform_IJPFor.InverseTransformPoint(newPosWorld);
            vertices_IJPFor[i] = newPosWorld;
        }
    }

    public void deform_parallel()
    {
        if (neighborSpheres.Count <= 0 || utCoords.Count <= 0)
        {
            Debug.Log("Error in deform: neighborspheres or utCoords is empty!");
            return;
        }

        // convert data to be processed into 'NativeArray' stucture for IJPFor
        var neighborSpheresLayIdx_IJPFor = new NativeArray<int>(4 * vertices.Length, Allocator.Persistent);
        var neighborSpheresSphIdx_IJPFor = new NativeArray<int>(4 * vertices.Length, Allocator.Persistent);
        var utCoords_IJPFor = new NativeArray<float>(2 * vertices.Length, Allocator.Persistent);
        var vertices_IJPFor = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);

        int count, count1;
        for (count = 0; count < vertices.Length; count++)
        {
            for (count1 = 0; count1 < 4; count1++)
            {
                neighborSpheresLayIdx_IJPFor[4 * count + count1] = neighborSpheres[count][count1][0];
                neighborSpheresSphIdx_IJPFor[4 * count + count1] = neighborSpheres[count][count1][1];
            }
            utCoords_IJPFor[2 * count + 0] = utCoords[count][0];
            utCoords_IJPFor[2 * count + 1] = utCoords[count][1];
        }

        // initialize the parallel job data
        var job = new vertexPosUpdateJob()
        {
            sphereJointObjIdx_IJPFor = sphereJointObjIdx,
            utCoords_IJPFor = utCoords_IJPFor,
            neighborSpheresLayIdx_IJPFor = neighborSpheresLayIdx_IJPFor,
            neighborSpheresSphIdx_IJPFor = neighborSpheresSphIdx_IJPFor,
            vertices_IJPFor = vertices_IJPFor
        };

        // Schedule a parallel-for job
        //  first: how many for-each iteraions to perform
        //  second: batch size
        JobHandle jobHandle = job.Schedule(vertices_IJPFor.Length, 64);

        // Ensure the job has completed
        jobHandle.Complete();

        // copy vertices_IJPFor data back to vertices & convert world to local
        //vertices_IJPFor.CopyTo(vertices);
        for (count = 0; count < vertices.Length; count++)
        {
            vertices[count] = this.transform.InverseTransformPoint(vertices_IJPFor[count]);
        }

        // update clone mesh and normals
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();

        // Native arrays must be disposed mannually
        neighborSpheresLayIdx_IJPFor.Dispose();
        neighborSpheresSphIdx_IJPFor.Dispose();
        utCoords_IJPFor.Dispose();
        vertices_IJPFor.Dispose();
    }
    /// End of Parallelize the orignal "Deform" function

    /// ========== Updated version of findnig split vertices and split faces (since 7/22) ========= ///
    /// Prior version of finding split vertices and split faces (before 7/22)
    // Procedure
    //      build a map between sphere-squares to be splited and mesh vertices inside
    //      identify all the mesh vertices inside the sphere-suqares to be split 
    // Return
    //      map: Dictionary<front layer index of splitting sphere-squares, List<vertices within those squares>>
    //      affectedVertexList: List<vertex index> all the mesh vertices (unique) within sphere-squares to be split
    private bool buildMap1(int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel, ref Dictionary<int, List<int>> map)
    {
        // obtain the sphere on the other side of spliting line
        int numSpheres = sphereJointModel.m_numSpheres;
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= numSpheres)
            sphereIdxB = sphereIdxB - numSpheres;

        // iterate mesh vertices to find the ones within the sphere quads to be split
        int count1, count2;
        int layerIdx, sIdx;
        Vector3 vertex;
        for (int count = 0; count < vertices.Length; count++)
        {
            count2 = 0;
            vertex = this.transform.TransformPoint(vertices[count]); //convert vertex's local position into world space
            for (count1 = 0; count1 < 4; count1++)
            {
                layerIdx = neighborSpheres[count][count1][0];
                sIdx = neighborSpheres[count][count1][1];
                // check if all the neighborspheres are split
                if ((layerIdx >= layers2split[0] && layerIdx <= layers2split[1]) &&
                    (sIdx == sphereIdxA || sIdx == sphereIdxB))
                {
                    count2++;
                }
            }
            // highlight the vertices within split sphere quads
            if (count2 == 4)
            {
                // save split mesh vertices by the first sphere layers
                layerIdx = neighborSpheres[count][0][0];
                // add the vertex to the map
                if (!map.ContainsKey(layerIdx))
                    map.Add(layerIdx, new List<int>());
                map[layerIdx].Add(count);
                //splitVertexList.Add(count);
                affectedVertexList.Add(count);
            }
        }

        if (map.Count <= 0)
        {
            Debug.Log("Error in split: no vertices to split!");
            return false;
        }

        // sort the map by the keys (layer index) in ascending order
        map = map.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        // remove replicas in the affectedVertexList
        affectedVertexList = affectedVertexList.Distinct().ToList();

        return true;
    }

    /// determine vertex v between nSLayerIdxA and nSLayerIdxB "above" or "below"
    //    the given sphereIdxA and sphereIdxB
    //      return: 1: above/ -1: below / 0: error
    private int aboveOrBelow(int v, int sphereIdxA, int sphereIdxB, int nSSphIdxA, int nSSphIdxB, int numSpheres)
    {
        bool B_LEE_nA = false; // true: sphereIdxB <= nSSphIdxA / false: sphereIdxB > nSSphIdxA
        bool A_LAE_nB = false; // true: sphereIdxA >= nSSpnIdxB / false: sphereIdxA < nSSpnIdxB
        if (nSSphIdxA == sphereIdxA && nSSphIdxB == sphereIdxB) // within the same split neighbor sphere range
        {

            if (utCoords[v][1] >= 0.5) // above
            {
                return 1;
            }
            else // below
            {
                return -1;
            }
        }
        else
        {
            // B_LEE_nA: compare sphereIdxB with nSSphIdxA
            if (Math.Abs(sphereIdxB - nSSphIdxA) <= (numSpheres / 2))
            {
                if (sphereIdxB <= nSSphIdxA)
                    B_LEE_nA = true;
                else
                    B_LEE_nA = false;
            }
            else
            {
                if (sphereIdxB > nSSphIdxA)
                    B_LEE_nA = true;
                else
                    B_LEE_nA = false;
            }
            // A_LAE_nB: compare sphereIdxA with nSSphIdxB
            if (Math.Abs(sphereIdxA - nSSphIdxB) <= (numSpheres / 2))
            {
                if (sphereIdxA >= nSSphIdxB)
                    A_LAE_nB = true;
                else
                    A_LAE_nB = false;
            }
            else
            {
                if (sphereIdxA < nSSphIdxB)
                    A_LAE_nB = true;
                else
                    A_LAE_nB = false;
            }
            
            if (B_LEE_nA) // above
            {
                return 1;
            }
            else
            {
                if (A_LAE_nB) // below
                    return -1;
                else
                {
                    Debug.Log("Error: in aboveOrBelow: evaluate above/below side of a vertex");
                    return 0;
                }
            }
        }
    }

    // check the connectivity of the triangles in splitFaceList
    //  Criterion: every triangle except the ending one should share the edges
    //    with the other 2 triangles in the list
    //  Output: 
    //      fConnectMap: {triID, List<edge-shared triIDs>} for split triangles
    //      bPartialSplit: true: partial split / false: full split
    //      layers2split_mesh: split faces' corresponding first and last layers, for both outer and inner layers
    //  Return: 
    //      true: good connectivity; false: bad connecivity 
    private bool checkSplitFaceConnectivity(int[] layers2split, int numLayers, ref Dictionary<int, List<int>>fConnectMap, ref bool bPartialSplit, ref int[,] layers2split_mesh)
    {
        // obtain the list of split face indices
        List<int> spFaceIdxList = new List<int>(splitFaceList.Keys);

        // traverse each face and identify other faces share the same edges
        int i, j, k, count = 0;
        int fIdx_i, fIdx_j;
        List<int> vers_i = new List<int>();
        List<int> vers_j = new List<int>();
        //Dictionary<int, List<int>> fConnectMap = new Dictionary<int, List<int>>();
        for (i = 0; i < spFaceIdxList.Count; i ++)
        {
            fIdx_i = spFaceIdxList[i];
            fConnectMap.Add(fIdx_i, new List<int>());
            vers_i.Clear();
            for (k = 0; k < 3; k++) // vertcies for face i
            {
                vers_i.Add(triangles[fIdx_i + k]);
            }
            for (j = 0; j < spFaceIdxList.Count; j ++)
            {
                fIdx_j = spFaceIdxList[j];
                if (fIdx_i == fIdx_j)
                    continue;
                vers_j.Clear();
                for (k = 0; k < 3; k++) // vertices for face j
                {
                    vers_j.Add(triangles[fIdx_j + k]);
                }
                // check if share the same edge (count==2)
                count = 0;
                for (k = 0; k < 3; k++)
                {
                    if (vers_j.Contains(vers_i[k]))
                        count++;
                }
                if (count == 2) // add face j to list of face i
                {
                    fConnectMap[fIdx_i].Add(fIdx_j);
                }
            }
        }

        // determine the split layers corresponding to the split faces
        int v, layerIdx0, layerIdx1;
        //int[,] layers2split_mesh = new int [2, 2]; // outer: [0]: first, last, inner: [1]: first, last
        layers2split_mesh[0, 0] = 100; // outer layer
        layers2split_mesh[0, 1] = -1;
        layers2split_mesh[1, 0] = 100; // inner layer
        layers2split_mesh[1, 1] = -1;
        foreach (KeyValuePair<int, List<int>> kvp in fConnectMap)
        {
            fIdx_i = kvp.Key; // current face index
            for (k = 0; k < 3; k++) // vertcies for face i
            {
                v = triangles[fIdx_i + k];
                layerIdx0 = neighborSpheres[v][0][0];
                layerIdx1 = neighborSpheres[v][2][0];
                // update outer & inner layers individually
                if (triIOLayerInfo[fIdx_i / 3] == 1) // outer
                {
                    if (layerIdx0 < layers2split_mesh[0, 0])
                        layers2split_mesh[0, 0] = layerIdx0;
                    if (layerIdx1 > layers2split_mesh[0, 1])
                        layers2split_mesh[0, 1] = layerIdx1;
                }
                else if (triIOLayerInfo[fIdx_i / 3] == -1) // inner
                {
                    if (layerIdx0 < layers2split_mesh[1, 0])
                        layers2split_mesh[1, 0] = layerIdx0;
                    if (layerIdx1 > layers2split_mesh[1, 1])
                        layers2split_mesh[1, 1] = layerIdx1;
                }
            }
        }
        Debug.Log("split layers for outer colon mesh: " + layers2split_mesh[0, 0].ToString() + "," + layers2split_mesh[0, 1].ToString());
        Debug.Log("split layers for inner colon mesh: " + layers2split_mesh[1, 0].ToString() + "," + layers2split_mesh[1, 1].ToString());

        // examine face edge-connectivity map: fConnectMap
        bool bValidConnectivity = true;
        List<int> tmpList = new List<int>();
        // debug use
        bool bFoundEndPoint = false;
        List<int> tmpVerList = new List<int>();
        //
        foreach (KeyValuePair<int, List<int>> kvp in fConnectMap)
        {
            bFoundEndPoint = false;
            fIdx_i = kvp.Key; // current face index
            tmpList = kvp.Value; // other faces sharing same edges
            //if (tmpList.Count != 2)
            //    bValidConnectivity = false;
            
            if (tmpList.Count <= 0) // isolate face
            {
                Debug.Log("Error in checkSplitFaceConnectivity: isolate face found");
                bValidConnectivity = false;
            }
            else if (tmpList.Count == 1) // one face connectivity
            {
                // check if end face
                for (k = 0; k < 3; k++) // vertcies for face i
                {
                    v = triangles[fIdx_i + k];
                    layerIdx0 = neighborSpheres[v][0][0];
                    layerIdx1 = neighborSpheres[v][2][0];

                    if (triIOLayerInfo[fIdx_i/3] == 1) // outer layer
                    {
                        //Debug.Log("checkSplitFaceConnectivity: found end point triangle " + fIdx_i.ToString() + "neibhorf:" + tmpList[0].ToString());
                        if (layerIdx1 == layers2split_mesh[0, 1])
                            bFoundEndPoint = true;
                    }
                    else if (triIOLayerInfo[fIdx_i / 3] == -1) // inner layer
                    {
                        //Debug.Log("checkSplitFaceConnectivity: found end point triangle " + fIdx_i.ToString() + "neibhorf:" + tmpList[0].ToString());
                        if (layerIdx1 == layers2split_mesh[1, 1])
                            bFoundEndPoint = true;
                    }
                    //if ((layerIdx1 == layers2split[1] || layerIdx0 == layers2split[1]) && (layers2split[1] < (numLayers-1))) // end point & not the last layer of the model
                    //{
                    //    //Debug.Log("checkSplitFaceConnectivity: found end point triangle " + fIdx_i.ToString() + "neibhorf:" + tmpList[0].ToString());
                    //    bFoundEndPoint = true;
                    //}
                }
                if (!bFoundEndPoint) // all 3 vertices are within the last layer
                {
                    Debug.Log("Error in checkSplitFaceConnectivity: only 1 connected face found");
                    bValidConnectivity = false;
                }
                else
                {
                    Debug.Log("Partial split triangle found: " + fIdx_i.ToString());
                    bPartialSplit = true;
                    // debug use: add the vertices for display
                    //for (k = 0; k < 3; k++)
                    //{
                    //    tmpVerList.Add(triangles[fIdx_i + k]);
                    //}
                    //
                }
            }
            else if (tmpList.Count > 2) // more than 2 connected faces
            {
                Debug.Log("Error in checkSplitFaceConnectivity: more than 2 connected faces found");
                bValidConnectivity = false;
            }
        }
        // debug use only
        //splitVertexList = tmpVerList.Distinct().ToList();
        //
        if (bValidConnectivity == false)
        {
            Debug.Log("Error in checkSplitFaceConnectivity: invalid connectivity of split faces");
            return false;
        }

        return true;
    }

    // Procedure:
    //  Further examine the connected split faces: identify the first and last triangles for
    //      for both outer and inner layers of the colon model
    //  Note: triangles of section layer are included into the outer layer
    // Return:  
    //      startEndSplitFaces: "start_outer"/"end_outer"/"start_inner"/"end_inner": face index (within 'triangles')
    private bool sortSplitFaceAlongLayers(int[,] layers2split_mesh, Dictionary<int, List<int>> fConnectMap, bool bPartialSplit, ref Dictionary<string, int> startEndSplitFaces)
    {
        if (triIOLayerInfo.Count <= 0)
        {
            Debug.Log("Error1 in 'sortSplitFaceAlongLayers': no triIOLayerInfo!");
            return false;
        }

        // obtain the list of split face indices
        List<int> spFaceIdxList = new List<int>(splitFaceList.Keys);

        // identify the first and end split triangles sharing edges with sections triangles
        //  in the first and last splitting layers for both inner and outer layers
        bool bFirst = false, bEnd = false;
        int i, j, k, layerIdx0, layerIdx1, verIdx, fIdx_i, triIOtag;
        int[] sharedTriIOtags = { 0, 0 };
        List<int> sharedFList = new List<int>();
        for (i = 0; i < spFaceIdxList.Count; i++)
        {
            bFirst = false;
            bEnd = false;
            fIdx_i = spFaceIdxList[i];
            triIOtag = triIOLayerInfo[fIdx_i / 3];
            sharedFList = fConnectMap[fIdx_i]; // get the faces sharing the same edges with the current face

            // check if the triangle has at least one vertex belonging to the first/end split layers
            for (k = 0; k < 3; k++) // vertcies for face i
            {
                verIdx = triangles[fIdx_i + k];
                layerIdx0 = neighborSpheres[verIdx][0][0];
                layerIdx1 = neighborSpheres[verIdx][2][0];
                
                if (triIOtag == 0 || triIOtag == 1) // outer & section -> outer
                {
                    if (layerIdx0 == layers2split_mesh[0, 0]) // first split layer
                        bFirst = true;
                    if (layerIdx1 == layers2split_mesh[0, 1]) // end split layer
                        bEnd = true;
                }
                else if (triIOtag == -1) // inner
                {
                    if (layerIdx0 == layers2split_mesh[1, 0]) // first split layer
                        bFirst = true;
                    if (layerIdx1 == layers2split_mesh[1, 1]) // end split layer
                        bEnd = true;
                }
                /*if (layerIdx0 == layers2split[0]) // first split layer
                    bFirst = true;
                if (layerIdx0 == layers2split[1] || layerIdx1 == layers2split[1]) // end split layer
                    bEnd = true;*/
            }
            if (bFirst && bEnd)
            {
                Debug.Log("Error2 in 'sortSplitFaceAlongLayers");
                return false;
            }
            // check if the triangle is the first of outer or inner layer
            if (bFirst) // at least one vertex within the first layer
            {
                if (sharedFList.Count != 2)
                {
                    Debug.Log("Error3 in 'sortSplitFaceAlongLayers'");
                    return false;
                }
                // get the inner/outer info for these shared triangles
                sharedTriIOtags[0] = triIOLayerInfo[sharedFList[0]/3];
                sharedTriIOtags[1] = triIOLayerInfo[sharedFList[1]/3];
                if (triIOtag == 0) // outer layer
                {
                    // check if shared with one outer one section
                    if ((sharedTriIOtags[0] == 0 && sharedTriIOtags[1] == -1) ||
                        (sharedTriIOtags[0] == -1 && sharedTriIOtags[1] == 0))
                        startEndSplitFaces.Add("start_outer", fIdx_i); // the first triangle of outer layer is a section triangle
                }
                else if (triIOtag == -1) // inner layer
                {
                    // check if shared with one inner one section
                    if ((sharedTriIOtags[0] == 0 && sharedTriIOtags[1] == -1) ||
                        (sharedTriIOtags[0] == -1 && sharedTriIOtags[1] == 0))
                        startEndSplitFaces.Add("start_inner", fIdx_i);
                }
            }
            // check if the triangle is the last of outer or inner layer
            if (bEnd)
            {
                if (bPartialSplit) // the triangles having only one shared triangle are end ones
                {
                    if (sharedFList.Count == 1) 
                    {
                        if (triIOtag == 1)// outer layer
                            startEndSplitFaces.Add("end_outer", fIdx_i); // the end triangle of outer layer is a outer triangle
                        else if (triIOtag == -1) // inner layer
                            startEndSplitFaces.Add("end_inner", fIdx_i); // the end triangle of inner layer is a inner triangle
                    }
                }
                else // full split
                {
                    if (sharedFList.Count != 2)
                    {
                        Debug.Log("Error4 in 'sortSplitFaceAlongLayers'");
                        return false;
                    }
                    // get the inner/outer info for these shared triangles
                    sharedTriIOtags[0] = triIOLayerInfo[sharedFList[0] / 3];
                    sharedTriIOtags[1] = triIOLayerInfo[sharedFList[1] / 3];
                    if (triIOtag == 0) // outer layer
                    {
                        // check if shared with one outer one section
                        if ((sharedTriIOtags[0] == 0 && sharedTriIOtags[1] == -1) ||
                            (sharedTriIOtags[0] == -1 && sharedTriIOtags[1] == 0))
                            startEndSplitFaces.Add("end_outer", fIdx_i); // the end triangle of outer layer is a section triangle
                    }
                    else if (triIOtag == -1) // inner layer
                    {
                        // check if shared with one inner one section
                        if ((sharedTriIOtags[0] == 0 && sharedTriIOtags[1] == -1) ||
                            (sharedTriIOtags[0] == -1 && sharedTriIOtags[1] == 0))
                            startEndSplitFaces.Add("end_inner", fIdx_i);
                    }
                }
            }
            // break if the first triangles for both inner/outer layers are found
            if (startEndSplitFaces.ContainsKey("start_outer") && startEndSplitFaces.ContainsKey("start_inner") &&
                startEndSplitFaces.ContainsKey("end_outer") && startEndSplitFaces.ContainsKey("end_inner"))
            {
                Debug.Log("The first triangle for outer layer: " + startEndSplitFaces["start_outer"].ToString());
                Debug.Log("The end triangle for outer layer: " + startEndSplitFaces["end_outer"].ToString());
                Debug.Log("The first triangle for inner layer: " + startEndSplitFaces["start_inner"].ToString());
                Debug.Log("The end triangle for inner layer: " + startEndSplitFaces["end_inner"].ToString());
                break;
            }
        }
        if (!startEndSplitFaces.ContainsKey("start_outer") || !startEndSplitFaces.ContainsKey("start_inner") ||
            !startEndSplitFaces.ContainsKey("end_outer") || !startEndSplitFaces.ContainsKey("end_inner"))
        {
            Debug.Log("Error5 in 'sortSplitFaceAlongLayers': cannot find the first/end triangle for outer layer or inner layer!");
            return false;
        }

        /// check connectivity of the first/end split triangles for outer and inner layers
        //      Start: the first triangles of inner and outer should share the same edge
        List<int> verList = new List<int>();
        for (k = 0; k < 3; k++) // vertcies for the first of outer
        {
            verIdx = triangles[startEndSplitFaces["start_outer"] + k];
            verList.Add(verIdx);
        }
        int count = 0;
        for (k = 0; k < 3; k++)
        {
            verIdx = triangles[startEndSplitFaces["start_inner"] + k];
            for (j = 0; j < 3; j++)
            {
                if (verIdx == verList[j])
                    count++;
            }
        }
        if (count != 2)
        {
            Debug.Log("Error6 in 'sortSplitFaceAlongLayers: first triangles of outer and inner are not connect!");
            return false;
        }
        //      End: for full split, the end triangles of inner and outer should share the same edge
        if (!bPartialSplit)
        {
            verList.Clear();
            for (k = 0; k < 3; k++) // vertcies for the end of outer
            {
                verIdx = triangles[startEndSplitFaces["end_outer"] + k];
                verList.Add(verIdx);
            }
            count = 0;
            for (k = 0; k < 3; k++)
            {
                verIdx = triangles[startEndSplitFaces["end_inner"] + k];
                for (j = 0; j < 3; j++)
                {
                    if (verIdx == verList[j])
                        count++;
                }
            }
            if (count != 2)
            {
                Debug.Log("Error7 in 'sortSplitFaceAlongLayers: end triangles of outer and inner are not connect!");
                return false;
            }
        }
        return true;
    }

    // get split face list: triangles interesecting with the split line
    //  Output 
    //      splitFaceList: <triID, List<tags of v1,v2,v3>>
    //      fConnectMap: connecivity info of split triangles: {triID, List<edge-shared triIDs>}
    //      startEndSplitFaces: "start_outer"/"end_outer"/"start_inner"/"end_inner": face index (within 'triangles')
    //  Return
    //      true/false: whether or not find sequences of edge-connected split triangles
    private bool getSplitFaceList1(int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel, Dictionary<int, List<int>> map,
                                        ref Dictionary<int, List<int>> fConnectMap, ref Dictionary<string, int> startEndSplitFaces)
    {
        // get all faces sharing the vertices in the map
        int i, j, k, v, layerIdx, count;
        List<int> tempList = new List<int>();
        List<int> verList = new List<int>();
        List<int> sharedFaceList = new List<int>();
        foreach (KeyValuePair<int, List<int>> kvp in map)
        {
            //verList.Clear();
            verList = kvp.Value;
            for (i = 0; i < verList.Count; i++)//traverse all vertices within each quad
            {
                v = verList[i];
                for (j = 0; j < triangles.Length; j += 3)
                {
                    tempList.Clear();
                    for (k = 0; k < 3; k++)
                    {
                        tempList.Add(triangles[j + k]);
                    }
                    if (tempList.Contains(v))
                    {
                        // check if all the triangle vertices are within the split quad
                        count = 0;
                        for (k = 0; k < 3; k++)
                        {
                            layerIdx = neighborSpheres[tempList[k]][2][0];
                            if (layerIdx >= layers2split[0] && layerIdx <= layers2split[1])
                                count++;
                        }
                        if (count == 3)
                            sharedFaceList.Add(j);
                    }
                }
            }
        }
        if (sharedFaceList.Count <= 0)
        {
            Debug.Log("Error 1 in getSplitFaceList1: no split faces found");
            return false;
        }
        // remove repeated face indices
        List<int> distSharedFaceList = sharedFaceList.Distinct().ToList();

        // traverse face list, add the ones interesting with the split line (t=0.5) to split face list
        int numSpheres = sphereJointModel.m_numSpheres;
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= numSpheres)
            sphereIdxB = sphereIdxB - numSpheres;
        int tag, nSSphIdxA, nSSphIdxB;
        List<int> tags = new List<int>();
        foreach(int f in distSharedFaceList)
        {
            tags.Clear();
            for (j = 0; j < 3; j++)
            {
                v = triangles[f + j];
                nSSphIdxA = neighborSpheres[v][0][1];
                nSSphIdxB = neighborSpheres[v][1][1];
                tag = aboveOrBelow(v, sphereIdxA, sphereIdxB, nSSphIdxA, nSSphIdxB, numSpheres); //1: above/ -1: below
                if (tag != 0)
                {
                    tags.Add(tag);
                }
                else
                    Debug.Log("Error face index: " + f.ToString());
            }
            if (tags.Count() != 3)
                return false;

            // determine if the face has opposite vertex tags -> intersect
            if (Math.Abs(tags[0] + tags[1] + tags[2]) != 3)
            {
                splitFaceList.Add(f, new List<int>());
                splitFaceList[f].Add(tags[0]);
                splitFaceList[f].Add(tags[1]);
                splitFaceList[f].Add(tags[2]);
            }
                //splitFaceList1.Add(f);
        }

        if (splitFaceList.Count <= 0)
        {
            Debug.Log("Error in getSplitFaceList1: spliting info empty!");
            return false;
        }

        // examine split face connectivity
        int numLayers = sphereJointModel.m_numLayers;
        bool bPartialSplit = false;
        int[,] layers2split_mesh = new int[2, 2]; // outer: [0]: first, last, inner: [1]: first, last
        if (!checkSplitFaceConnectivity(layers2split, numLayers, ref fConnectMap, ref bPartialSplit, ref layers2split_mesh))
            return false;

        // check if the splitting layers along meshes are the same as the input split layers
        if (layers2split_mesh[0, 0] != layers2split[0] || layers2split_mesh[0, 1] != layers2split[1] ||
            layers2split_mesh[1, 0] != layers2split[0] || layers2split_mesh[1, 1] != layers2split[1])
        {
            Debug.Log("Error in getSplitFaceList1: invalid mesh splitting layers");
            return false;
        }

        // find the first triangle for both inner and outer layers
        if (!sortSplitFaceAlongLayers(layers2split_mesh, fConnectMap, bPartialSplit, ref startEndSplitFaces))
            return false;

        return true;
    }

    // Identify split vertex from the given triangle
    // Input
    //      preTriIdx: previous split triangle index
    //      curTriIdx: current split triangle index (pre and cur triangles are connected)
    //      splitFaceList (member vairable): <triID, List<tags of v1,v2,v3>>; tag: 1:above/-1:below
    // Procedure
    //      split vertex selection criteria:
    //          - the only vertex with the opposite below/above tag amoung the current triangle
    //          - not forming a triangle with the previous two split vertices in the list
    //      make sure current split vertex connects to the previous one in the list
    // Output
    //      splitVerList_layer <- current split vertex from the current triangle
    // Return 
    //      true: find the vertex; false: not find
    private bool getSplitVertexFromTriangle(int preTriIdx, int curTriIdx, ref List<int> splitVerList_layer)
    {
        int potentialSplitVerIdx = -1, splitVerIdx = -1;
        int i, j, k, count = 0;
        List<int> curTriVers = new List<int>();
        List<int> curTriVerTags = new List<int>();
        List<int> preTriVers = new List<int>();

        for (k = 0; k < 3; k++)
        {
            curTriVers.Add(triangles[curTriIdx + k]);
            curTriVerTags.Add(splitFaceList[curTriIdx][k]);
            preTriVers.Add(triangles[preTriIdx + k]);
        }

        // check if the current triangle contains the last split vertex (make sure split vertex connectivity)
        if (splitVerList_layer.Count > 0)
        {
            if (!curTriVers.Contains(splitVerList_layer[splitVerList_layer.Count - 1]))
            {
                Debug.Log("Error1 in 'getSplitVertexFromTriangle': vertex connectivity issue");
                return false;
            }
        }

        // examine the vertices of the current triangle, and find the only vertex with the opposite below/above tag
        for (k = 0; k < 3; k++)
        {
            i = (k == 0) ? 2 : (k - 1);
            j = (k == 2) ? 0 : (k + 1);
            if (curTriVerTags[k] != curTriVerTags[i] && curTriVerTags[k] != curTriVerTags[j])
                potentialSplitVerIdx = curTriVers[k];
        }
        if (potentialSplitVerIdx < 0)
        {
            Debug.Log("Error2 in 'getSplitVertexFromTriangle': invalid potentialSplitVerIdx");
            return false;
        }

        // identify split vertex
        int preVerIdx0, preVerIdx1;
        List<int> sharedEdgeVers = new List<int>();
        if (splitVerList_layer.Count <= 0) // the current triangle is start_inner or start_outer 
        {
            // the split vertex has to be one of the two vertices shared by start_inner and start_outer triangels
            for (k = 0; k < 3; k++)
            {
                if (curTriVers.Contains(preTriVers[k])) // shared vertex
                    sharedEdgeVers.Add(preTriVers[k]);
            }
            if (sharedEdgeVers.Count != 2)
            {
                Debug.Log("Error3 in 'getSplitVertexFromTriangle'");
                return false;
            }
            // check if the vertex with opposite tag is on the shared edge
            if (sharedEdgeVers.Contains(potentialSplitVerIdx))
                splitVerIdx = potentialSplitVerIdx;
            else // pick either of the two shared edge vertices
                splitVerIdx = sharedEdgeVers[0];
        }
        else // the current triangle is not the start
        {
            if (splitVerList_layer.Count < 2)
                splitVerIdx = potentialSplitVerIdx;
            else
            {
                preVerIdx0 = splitVerList_layer[splitVerList_layer.Count - 1];
                preVerIdx1 = splitVerList_layer[splitVerList_layer.Count - 2];
                if (preVerIdx0 != potentialSplitVerIdx) // the vertex not appear before
                {
                    // check if the vertex form previous triangle with the previous split vertices
                    if (preTriVers.Contains(potentialSplitVerIdx) && preTriVers.Contains(preVerIdx0) && preTriVers.Contains(preVerIdx1))
                    {
                        // mark the 3rd vertex of the current triangle as split vertex instead
                        for (k = 0; k < 3; k++)
                        {
                            if (curTriVers[k] != potentialSplitVerIdx && curTriVers[k] != preVerIdx0)
                            {
                                splitVerIdx = curTriVers[k];
                                break;
                            }
                        }
                    }
                    else
                    {
                        splitVerIdx = potentialSplitVerIdx;
                    }
                }
                else
                    splitVerIdx = potentialSplitVerIdx;
            }
            
        }

        if (splitVerIdx <= 0)
        {
            Debug.Log("Error4 in 'getSplitVertexFromTriangle': cannot identify split vertex from triangle " + curTriIdx.ToString());
            return false;
        }

        // check if the potential split vertex already exists in the list
        //    if not, add the split vertex into the list
        if (!splitVerList_layer.Contains(splitVerIdx))
            splitVerList_layer.Add(splitVerIdx);

        return true;
    }

    // Identify split vertex from the given triangle, find the ones closest to the mideline
    // Input
    //      bEndFace: true/false the current split triangle (curTriIdx) is an 'END' split face
    //      preTriIdx: previous split triangle index
    //      curTriIdx: current split triangle index (pre and cur triangles are connected)
    //      layers2split & sphereIdx2split: gives the range of the sphere-squares to be split
    //      utCoords:  ut coordinates of each mesh vertex
    // Procedure
    //      split vertex selection criteria:
    //          From the vertices of the edge shared by pre and cur triangles, find
    //          - the vertex (of the current triangle) clostest to the midline (t=0.5) of to the splitting sphere-square  
    //          - not forming a triangle with the previous two split vertices in the list
    //      make sure current split vertex connects to the previous one in the list
    //      If bEndFace==true, make sure split vertex identified from the the face 'curTriIdx' is within the last layer 'layers2split[1]'
    // Output
    //      splitVerList_layer <- current split vertex from the current triangle
    // Return 
    //      true: find the vertex; false: not find
    private bool getSplitVertexFromTriangle1(bool bEndFace, int preTriIdx, int curTriIdx, int[] layers2split, int[] sphereIdx2split, ref List<int> splitVerList_layer)
    {
        int potentialSplitVerIdx = -1, splitVerIdx = -1;
        int k;
        List<int> curTriVers = new List<int>();
        List<int> preTriVers = new List<int>();

        // get the vertices of the edge shared by pre and current triangles
        List<int> sharedEdgeVers = new List<int>();
        for (k = 0; k < 3; k++)
        {
            curTriVers.Add(triangles[curTriIdx + k]);
            preTriVers.Add(triangles[preTriIdx + k]);
        }
        for (k = 0; k < 3; k++)
        {
            if (curTriVers.Contains(preTriVers[k])) // shared vertex
                sharedEdgeVers.Add(preTriVers[k]);
        }
        if (sharedEdgeVers.Count != 2)
        {
            Debug.Log("Error1 in 'getSplitVertexFromTriangle1'");
            return false;
        }

        // identify potential split vertex: among the 2 shared vertices  
        //    find the vertex closest to the mideline of the splitting sphere-square
        int verIdx, sphereIdx_ver;
        float t_ver, dist_ver, minDist = 10.0f; // min distantce from the vertex to the midline along t
        for (k = 0; k < 2; k++)
        {
            verIdx = sharedEdgeVers[k];
            t_ver = utCoords[verIdx][1];
            sphereIdx_ver = neighborSpheres[verIdx][0][1];
            if (sphereIdx_ver < sphereIdx2split[0]) // vertex within the sphere-square left
                dist_ver = 1.5f - t_ver;
            else if (sphereIdx_ver > sphereIdx2split[1]) // vertex within the sphere-square right
                dist_ver = 0.5f + t_ver;
            else // vertex wtihin the split sphere-square
                dist_ver = Mathf.Abs(t_ver - 0.5f);
            if (dist_ver < minDist)
            {
                minDist = dist_ver;
                potentialSplitVerIdx = verIdx;
            }
        }
        if (potentialSplitVerIdx < 0)
        {
            Debug.Log("Error2 in 'getSplitVertexFromTriangle1': invalid potentialSplitVerIdx");
            return false;
        }

        // identify split vertex: check if the potential split vertex form a triangle
        //      with the last two vertices in the split vertex list
        int preVerIdx0, preVerIdx1;
        if (splitVerList_layer.Count < 2)
            splitVerIdx = potentialSplitVerIdx;
        else // split vertex list has more than 2 vertices already
        {
            preVerIdx0 = splitVerList_layer[splitVerList_layer.Count - 1];
            preVerIdx1 = splitVerList_layer[splitVerList_layer.Count - 2];
            if (preVerIdx0 != potentialSplitVerIdx) // the vertex not appear before
            {
                // check if the vertex form previous triangle with the previous split vertices
                if (preTriVers.Contains(potentialSplitVerIdx) && preTriVers.Contains(preVerIdx0) && preTriVers.Contains(preVerIdx1))
                {
                    // mark the other shared vertex as split vertex instead
                    for (k = 0; k < 2; k++)
                    {
                        verIdx = sharedEdgeVers[k];
                        if (verIdx != potentialSplitVerIdx)
                        {
                            splitVerIdx = verIdx;
                            break;
                        }
                    }
                }
                else
                {
                    splitVerIdx = potentialSplitVerIdx;
                }
            }
            else
                splitVerIdx = potentialSplitVerIdx;
        }

        if (splitVerIdx < 0)
        {
            Debug.Log("Error3 in 'getSplitVertexFromTriangle1': cannot identify split vertex from triangle " + curTriIdx.ToString());
            return false;
        }

        // check if the potential split vertex already exists in the list
        //    if not, add the split vertex into the list
        if (!splitVerList_layer.Contains(splitVerIdx))
            splitVerList_layer.Add(splitVerIdx);

        // When bEndFace==true, make sure the last added split vertex is within the last layer
        //  if not, add another split vertex from curTriidx that is within the last layer
        if (bEndFace)
        {
            preVerIdx0 = splitVerList_layer[splitVerList_layer.Count - 1];
            preVerIdx1 = splitVerList_layer[splitVerList_layer.Count - 2];
            int curLastSplitVerLayerIdx = neighborSpheres[preVerIdx0][2][0];
            int testLastSplitVer, lastSplitVer = -1;
            if (curLastSplitVerLayerIdx < layers2split[1]) // split line of the mesh is too short 
            {
                // add another vertex from the curTriIdx that within the last layer to the split vertex list
                for (k = 0; k < 3; k++)
                {
                    testLastSplitVer = curTriVers[k];
                    if (testLastSplitVer == preVerIdx0)
                        continue;
                    if (neighborSpheres[testLastSplitVer][2][0] == layers2split[1])
                    {
                        if (!splitVerList_layer.Contains(testLastSplitVer)) // not appear before
                        {
                            // not forming a triangle with previous added split vertices
                            if (preTriVers.Contains(testLastSplitVer) && preTriVers.Contains(preVerIdx0) && preTriVers.Contains(preVerIdx1))
                                continue;
                            lastSplitVer = testLastSplitVer;
                            break;
                        }
                    }
                }
                if (lastSplitVer == -1)
                {
                    Debug.Log("Error4 in 'getSplitVertexFromTriangle1': cannot identify the last split vertex from triangle " + curTriIdx.ToString());
                    return false;
                }
                splitVerList_layer.Add(lastSplitVer);
            }
            else if (curLastSplitVerLayerIdx > layers2split[1])
            {
                Debug.Log("Error5 in 'getSplitVertexFromTriangle1': last split vertex exceeds the last split layer");
                return false;
            }
        }

        return true;
    }

    /// Identify split vertices based on split faces for each layer; those vertices should connect to each other
    // Input
    //      splitFaceList (member vairable): <triID, List<tags of v1,v2,v3>>; tag: 1:above/-1:below
    //      fConnectMap: connectivity info of split triangles: {triID, List<edge-shared triIDs>}
    //      startEndSplitFaces: "start_outer"/"end_outer"/"start_inner"/"end_inner": face index (within 'triangles')
    //      layers2split & sphereIdx2split: gives the range of the sphere-squares to be split
    // Procedure
    //      Identify vertices from start_inner/outer to end_inner/outer
    // Output
    //      splitVertexList: list of vertex indices from start_inner/outer to end_inner/outer
    // Return
    //      true: sucess / false: fail
    private bool getSplitVerticesFromLayer (string whichLayer, Dictionary<int, List<int>> fConnectMap, Dictionary<string, int> startEndSplitFaces,
                                                int[] layers2split, int[] sphereIdx2split, ref List<int> splitVerList_layer)
    {
        string startFace = "start_" + whichLayer, endFace = "end_" + whichLayer, startFaceOther = "start_";
        startFaceOther += (whichLayer == "outer") ? "inner" : "outer";
        int curTriIdx = startEndSplitFaces[startFace], preTriIdx = startEndSplitFaces[startFaceOther], nexTriIdx = -1;
        bool bEndFace = false;
        int i;
        List<int> sharedFaceList = new List<int>();
        while (preTriIdx != startEndSplitFaces[endFace])
        {
            // find the next connected face: nexTriIdx
            nexTriIdx = -1;
            sharedFaceList = fConnectMap[curTriIdx];
            for (i = 0; i < sharedFaceList.Count; i++)
            {
                if (sharedFaceList[i] != preTriIdx)
                    nexTriIdx = sharedFaceList[i];
            }
            if (sharedFaceList.Count <= 0 || nexTriIdx == -1) // cannot find the next connected split face
            {
                if (curTriIdx != startEndSplitFaces[endFace]) // not the end split face either
                {
                    Debug.Log("Error1 in 'getSplitVerticesFromLayer': invalid fConnectMap info for triID: " + curTriIdx.ToString());
                    return false;
                }
            }

            // identify the split vertex for the current split triangle
            bEndFace = (curTriIdx == startEndSplitFaces[endFace]);
            if (!getSplitVertexFromTriangle1(bEndFace, preTriIdx, curTriIdx, layers2split, sphereIdx2split, ref splitVerList_layer))
            //if (!getSplitVertexFromTriangle(preTriIdx, curTriIdx, ref splitVerList_layer))
                return false;

            preTriIdx = curTriIdx;
            curTriIdx = nexTriIdx;
        }

        if (splitVerList_layer.Count <= 0)
        {
            Debug.Log("Error2 in 'getSplitVerticesFromLayer': empty splitVerList_layer");
            return false;
        }

        return true;
    }

    /// Identify split vertices based on split faces to be mapped to the split line
    // Input
    //      splitFaceList (member vairable): <triID, List<tags of v1,v2,v3>>; tag: 1:above/-1:below
    //      fConnectMap: connectivity info of split triangles: {triID, List<edge-shared triIDs>}
    //      startEndSplitFaces: "start_outer"/"end_outer"/"start_inner"/"end_inner": face index (within 'triangles')
    // Procedure
    //      1) innder layer: Identify vertices from start_inner to end_inner 
    //      2) outer layer: Identify vertiices from start_outer to end_outer; make sure both start from the same vertex
    //      3) if full-split, make sure above 2 sequences ending up with the same vertex to form a loop
    // Output
    //      bFullSplit: true: full-split / false: partial-split
    //      splitVertexList: list of vertex indices that forms the split line in the order:
    //           start_outer -> end_outer -> end_inner -> start_inner
    //      startEndSplitVers: "start_outer"/"end_outer"/"start_inner"/"end_inner": vertex index (within 'vertices')
    // Return
    //      true: sucess / false: fail
    private bool getSplitVertexList1(int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel, Dictionary<int, List<int>> fConnectMap, 
                                            Dictionary<string, int> startEndSplitFaces, ref bool bFullSplit, ref Dictionary<string, int> startEndSplitVers)
    {
        if (splitFaceList.Count <= 0 || fConnectMap.Count <= 0)
        {
            Debug.Log("Error1 in getSplitVertexList1: spliting info empty!");
            return false;
        }
        if (!startEndSplitFaces.ContainsKey("start_outer") || !startEndSplitFaces.ContainsKey("start_inner")||
            !startEndSplitFaces.ContainsKey("end_outer") || !startEndSplitFaces.ContainsKey("end_inner"))
        {
            Debug.Log("Error2 in 'getSplitVertexList1: no startEndSplitFaces info!");
            return false;
        }

        /// Identify connected split vertices from split faces respectively for outer and inner layers
        // obtain the index range of the sphere-quads
        int numSpheres = sphereJointModel.m_numSpheres;
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= numSpheres)
            sphereIdxB = sphereIdxB - numSpheres;
        int[] sphereIdx2split = { sphereIdxA, sphereIdxB };
        // outer layer: start_outer -> end_outer
        string whichLayer = "outer";
        List<int> splitVerList_outer = new List<int>();
        if (!getSplitVerticesFromLayer(whichLayer, fConnectMap, startEndSplitFaces, layers2split, sphereIdx2split, ref splitVerList_outer))
            return false;

        // inner layer: start_inner -> end_inner
        whichLayer = "inner";
        List<int> splitVerList_inner = new List<int>();
        // make sure the lists of both layers start from the same split vertex
        splitVerList_inner.Add(splitVerList_outer[1]);
        splitVerList_inner.Add(splitVerList_outer[0]);
        if (!getSplitVerticesFromLayer(whichLayer, fConnectMap, startEndSplitFaces, layers2split, sphereIdx2split, ref splitVerList_inner))
            return false;

        // if full-split, check if the lists are connected at the end split vertices
        if (sphereJointModel.m_numLayers <= (Math.Abs(layers2split[0]-layers2split[1]) + 1))
        {
            // find the vertices of the edge shared by both end_inner and end_outer
            int k, ver;
            List<int> vers = new List<int>();
            List<int> sharedVers = new List<int>();
            for (k = 0; k < 3; k++)
            {
                ver = triangles[startEndSplitFaces["end_outer"] + k];
                vers.Add(ver);
            }
            for (k = 0; k < 3; k++)
            {
                ver = triangles[startEndSplitFaces["end_inner"] + k];
                if (vers.Contains(ver))
                    sharedVers.Add(ver);
            }
            if (sharedVers.Count != 2)
            {
                Debug.Log("Error3 in getSplitVertex1: loop checking");
                return false;
            }
            if (sharedVers.Contains(splitVerList_inner[splitVerList_inner.Count - 1]) ||
                sharedVers.Contains(splitVerList_outer[splitVerList_outer.Count - 1]))
            {
                Debug.Log("Full-split: split vertices of outer & inner layers form a loop");
            }
            else // end split vertices not connect
            {
                // add one of the shared vertices into the inner split vertex list
                splitVerList_inner.Add(sharedVers[0]);
                Debug.Log("Full-split: add an extra vertex to close to loop");
            }
            bFullSplit = true;
        }

        /// combine splitVerList_inner & outer to be splitVerList
        int numSplitVer_inner = splitVerList_inner.Count, numSplitVer_outer = splitVerList_outer.Count;
        //Debug.Log("#split Vertices: inner - " + numSplitVer_inner.ToString() + ", outer - " + numSplitVer_outer.ToString());
        
        // remove the first two vertices from the inner list, as those are from the outer layer
        splitVerList_inner.RemoveRange(0, 2);
        numSplitVer_inner = splitVerList_inner.Count;
        startEndSplitVers.Add("start_outer", splitVerList_outer[0]);
        startEndSplitVers.Add("end_outer", splitVerList_outer[numSplitVer_outer - 1]);
        startEndSplitVers.Add("start_inner", splitVerList_inner[0]);
        startEndSplitVers.Add("end_inner", splitVerList_inner[numSplitVer_inner - 1]);
        Debug.Log("#split Vertices: inner - " + numSplitVer_inner.ToString() + ", outer - " + numSplitVer_outer.ToString());
        Debug.Log(" - outer layer: start: " + splitVerList_outer[0].ToString() + ", end: " + splitVerList_outer[numSplitVer_outer-1].ToString());
        Debug.Log(" - inner layer: start: " + splitVerList_inner[0].ToString() + ", end: " + splitVerList_inner[numSplitVer_inner-1].ToString());

        // combine outer layer with reversed inner layer to be: start_outer->end_outer->end_inner->start_inner
        splitVerList_inner.Reverse();
        Debug.Log(" - inner layer (reversed): start: " + splitVerList_inner[0].ToString() + ", end: " + splitVerList_inner[numSplitVer_inner - 1].ToString());
        splitVertexList = new List<int>(splitVerList_outer);
        splitVertexList.AddRange(splitVerList_inner);
        int numSplitVer_combined = splitVertexList.Count;
        Debug.Log("#combined split vertices: " + numSplitVer_combined.ToString() + ": start: " + splitVertexList[0].ToString() + ", end: " + splitVertexList[numSplitVer_combined - 1].ToString());
        // remove duplicates in the split vertex list
        splitVertexList = splitVertexList.Distinct().ToList();
        numSplitVer_combined = splitVertexList.Count;
        Debug.Log("#distinct split vertices: " + numSplitVer_combined.ToString() + ": start: " + splitVertexList[0].ToString() + ", end: " + splitVertexList[numSplitVer_combined - 1].ToString());

        if (splitVertexList.Count <= 0)
        {
            Debug.Log("Error4 in getSplitVertex1: no split vertex found");
            return false;
        }

        if (!bFullSplit && splitVertexList.Count <= 2)
        {
            Debug.Log("Error5 in getSplitVertex1: only 2 split vertices detected in a partial-split, can't continue");
            return false;
        }

        return true;
    }

    /// Project split vertices on to the mideline of the split sphere-quads & update the neighboring 
    ///   spheres of the split vertices initially outside the split sphere-quads to closes sphere-quads
    // Procedure:
    //      1. traverse each split vertex, mark the vertices outside split sphere-squares as outliers
    //      2. map each outlier split vertex to the closest sphere-square
    //      3. re-position each split vertex to the midline of its corresponding sphere-square
    // Input:
    //      split sphere-squares range: layers2split, sphereIdx
    //      splitVertexList
    // Output:
    //      updated neighborSpheres, vertices
    private bool projSplitVer2Midline1(int objIdx, int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel)
    {
        // obtain the index range of the sphere-quads
        int numSpheres = sphereJointModel.m_numSpheres;
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= numSpheres)
            sphereIdxB = sphereIdxB - numSpheres;
        
        // travese split vertices to check if they all belong to split sphere-quads
        int i, j, count;
        int vIdx, layerIdx, sIdx;
        List<int> outlierSplitVers = new List<int>();
        for (i = 0; i < splitVertexList.Count; i++)
        {
            vIdx = splitVertexList[i];
            count = 0;
            for (j = 0; j < 4; j++)
            {
                layerIdx = neighborSpheres[vIdx][j][0];
                sIdx = neighborSpheres[vIdx][j][1];
                // check if all the neighborspheres are split
                if ((layerIdx >= layers2split[0] && layerIdx <= layers2split[1]) &&
                    (sIdx == sphereIdxA || sIdx == sphereIdxB))
                {
                    count++;
                }
            }
            if (count != 4)
            {
                outlierSplitVers.Add(vIdx);
            }
        }

        // update neighbor spheres for each outlier split vertex to be within the split sphere-squares
        if (outlierSplitVers.Count > 0)
        {
            Debug.Log("#outlier sphere vertices: " + outlierSplitVers.Count.ToString());
            for (i = 0; i < outlierSplitVers.Count; i++)
            {
                vIdx = outlierSplitVers[i];
                Debug.Log("outlier split vertex: " + vIdx.ToString() + " layerIdx: " + neighborSpheres[vIdx][0][0].ToString() +
                        " sphereIdx: " + neighborSpheres[vIdx][0][1].ToString());
                neighborSpheres[vIdx][0][1] = sphereIdxA;
                neighborSpheres[vIdx][1][1] = sphereIdxB;
                neighborSpheres[vIdx][2][1] = sphereIdxA;
                neighborSpheres[vIdx][3][1] = sphereIdxB;
                Debug.Log("outlier split vertex: " + vIdx.ToString() + " layerIdx: " + neighborSpheres[vIdx][0][0].ToString() +
                        " sphereIdx: " + neighborSpheres[vIdx][0][1].ToString());
            }
        }

        // update t coord of each split vertex to be 0.5 (midline) and update their position
        Vector3 newPosWorld, newPosLocal;
        Vector3[] spherePos = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f) };
        GameObject sphere;
        for (i = 0; i < splitVertexList.Count; i++)
        {
            vIdx = splitVertexList[i];
            for (j = 0; j < 4; j++)
            {
                layerIdx = neighborSpheres[vIdx][j][0];
                sIdx = neighborSpheres[vIdx][j][1];
                sphere = GameObject.Find("sphere_" + objIdx.ToString() + "_" + layerIdx.ToString() + "_" + sIdx.ToString());
                if (sphere)
                    spherePos[j] = sphere.transform.position;
                else
                {
                    Debug.Log("Error1 in projSplitVer2Midline1: cannot find sphere_" + objIdx.ToString() + "_" + layerIdx.ToString() + "_" + sIdx.ToString());
                    return false;
                }
            }
            // update each split vertex's t coord to be 0.5 (midlien)
            utCoords[vIdx][1] = 0.5f;

            newPosWorld = spherePos[0] + utCoords[vIdx][1] * (spherePos[1] - spherePos[0]) + utCoords[vIdx][0] * (spherePos[2] - spherePos[0]);
            newPosLocal = this.transform.InverseTransformPoint(newPosWorld);
            vertices[vIdx] = newPosLocal;
        }

        // clear temp data
        outlierSplitVers.Clear();

        // update clone mesh and normals
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();

        Debug.Log("projSplitVer2Midline is done!");
        return true;
    }

    /// Search all the faces sharing the split vertices (including those faces that are within splitFacelist)
    ///     tag each face as above (+1) and below (-1) based on its non-split vertex
    // Procedure:
    //      1. tag all the faces to be split
    //      2. record all the faces sharing the split vertices
    //      3. update affectedVertexList to reflect all the non-split vertices within splitting sphere-squares (except the end split faces with tag==2)
    // Input:
    //      splitVertexList
    //      splitFaceList
    //      fConnectMap: connecivity info of split triangles: {triID, List<edge-shared triIDs>}
    //      startEndSplitFaces: "start_outer"/"end_outer"/"start_inner"/"end_inner": face index (within 'triangles')
    //      startEndSplitVers: "start_outer"/"end_outer"/"start_inner"/"end_inner": vertex index(within 'vertices')
    //      affectedVertexList: List<vertex index> all the mesh vertices(unique) within sphere-squares to be split
    // Output:
    //      shareSplitVertexFaceList: dictionary<face Indices,List<split vertices indices>>
    //      shareSplitFaceTags: List<Tags> for above face list
    //      affectedVertexList: List<vertex index> all the non-split vertices within splitting sphere-squares
    private bool tagFacesSharingSplitVertices(int sphereIdxA, sphereJointModel sphereJointModel, Dictionary<int, List<int>> fConnectMap, 
                    Dictionary<string, int> startEndSplitFaces, Dictionary<string, int> startEndSplitVers,
                    ref Dictionary<int, List<int>> shareSplitVertexFaceList, ref List<int> shareSplitFaceTags)
    {
        // obtain the sphere on the other side of spliting line
        int numSpheres = sphereJointModel.m_numSpheres;
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= numSpheres)
            sphereIdxB = sphereIdxB - numSpheres;

        // traverse each split vertex in the list and record all its sharing faces
        int i, j;
        int ver;
        for (i = 0; i < triangles.Length; i += 3)
        {
            for (j = 0; j < 3; j++)
            {
                ver = triangles[i + j];
                if (splitVertexList.Contains(ver))
                {
                    if (!shareSplitVertexFaceList.ContainsKey(i))
                        shareSplitVertexFaceList.Add(i, new List<int>());
                    shareSplitVertexFaceList[i].Add(ver);
                }
            }
        }

        // tag the faces sharing split vertices based on tags of their non-split vertices
        //  if all 3 vertices of a face are split vertices, tag such face at last based on its neighbor face
        int count = 0;
        int numFacesWith3SplitVers = 0, numFacesWithAmbiguity = 0;
        int numFacesSharingEndSplitVer = 0;
        int tag, nSSphIdxA, nSSphIdxB;
        List<int> verList = new List<int>();
        List<int> tags = new List<int>();
        foreach (KeyValuePair<int, List<int>> kvp in shareSplitVertexFaceList)
        {
            tags.Clear();
            verList = kvp.Value;
            shareSplitFaceTags.Add(0);
            if (verList.Count <= 0)
            {
                Debug.Log("Error1 in 'tagFacesSharingSplitVertices'!");
                return false;
            }
            else if (verList.Count == 3)
            {
                numFacesWith3SplitVers++;
            }
            else // #split vertices contained: 1 or 2
            {
                // tag the face based on non-split vertex
                for(j = 0; j < 3; j++)
                {
                    if (!verList.Contains(triangles[kvp.Key + j])) // found non-split vertex
                    {
                        ver = triangles[kvp.Key + j];
                        nSSphIdxA = neighborSpheres[ver][0][1];
                        nSSphIdxB = neighborSpheres[ver][1][1];
                        tag = aboveOrBelow(ver, sphereIdxA, sphereIdxB, nSSphIdxA, nSSphIdxB, numSpheres); //1: above/ -1: below
                        tags.Add(tag);
                        //affectedVertexList.Add(ver); 
                    }
                }
                // tag the face
                if (tags.Count == 1) // only one non-split vertex
                    shareSplitFaceTags[count] = tags[0];
                else if (tags.Count == 2) // two non-split vertices
                {
                    if (tags[0] != tags[1])
                    {
                        if (verList[0] == startEndSplitVers["end_inner"] || verList[0] == startEndSplitVers["end_outer"])
                        {
                            numFacesSharingEndSplitVer++;
                            shareSplitFaceTags[count] = 2;
                        }
                        else
                            numFacesWithAmbiguity++;
                    }
                    else
                        shareSplitFaceTags[count] = tags[0];
                    // remove those two non-split vertices that just added from 'affectedVertexList'
                    //affectedVertexList.RemoveRange(affectedVertexList.Count - 2, 2);
                }
            }
            count++;
        }
        
        // process affectedVertexList: remove all split vertices
        if (affectedVertexList.Count > 0)
        {
            // remove all split vertices
            affectedVertexList = affectedVertexList.Except(splitVertexList).ToList();
            affectedVertexList = affectedVertexList.Distinct().ToList();
            Debug.Log("tagFacesSharingSplitVertices: #affectedVertices: " + affectedVertexList.Count.ToString());
        }

        if (numFacesSharingEndSplitVer > 0)
            Debug.Log("tagFacesSharingSplitVertices: #faces sharing end split vertices: " + numFacesSharingEndSplitVer.ToString());
        if (numFacesWithAmbiguity > 0)
        {
            Debug.Log("Error1 in 'tagFacesSharingSplitVertices': found " + numFacesWithAmbiguity.ToString() + " faces with ambiuity!");
            return false;
        }

        // tag the split faces with all 3 split vertices or with ambiguity 
        int index = 0, count1 = 0, totUnhandledVerNum = numFacesWith3SplitVers, iterCount = 0;
        int maxIterNum = totUnhandledVerNum + 1;
        List<int> sharedSplitFaces = new List<int>();
        while(totUnhandledVerNum > 0)
        //if (numFacesWith3SplitVers > 0 || numEndFacesWithAmbiguity > 0)
        {
            count = 0;
            foreach (KeyValuePair<int, List<int>> kvp in shareSplitVertexFaceList)
            {
                if (shareSplitFaceTags[count] == 0)
                {
                    verList = kvp.Value;
                    // find another shareSplitVertex face sharing the same edge and already tagged
                    sharedSplitFaces = fConnectMap[kvp.Key];
                    if (sharedSplitFaces.Count < 1)
                    {
                        Debug.Log("Error3 in 'tagFacesSharingSplitVertices'!");
                        return false;
                    }
                    for (i = 0; i < sharedSplitFaces.Count; i++)
                    {
                        index = Array.IndexOf(shareSplitVertexFaceList.Keys.ToArray(), sharedSplitFaces[i]);
                        if (index < 0)
                        {
                            Debug.Log("Error4 in 'tagFacesSharingSplitVertices'!");
                            return false;
                        }
                        if (shareSplitFaceTags[index] != 0)
                        {
                            shareSplitFaceTags[count] = shareSplitFaceTags[index];
                            totUnhandledVerNum -= 1;
                            break;
                        }
                    }
                    /*count1 = 0;
                    foreach (KeyValuePair<int, List<int>> kvp1 in shareSplitVertexFaceList)
                    {
                        if (kvp.Key != kvp1.Key && shareSplitFaceTags[count1] != 0 && kvp1.Value.Contains(verList[0]))
                        {
                            shareSplitFaceTags[count] = shareSplitFaceTags[count1];
                            totUnhandledVerNum -= 1;
                        }
                        if (shareSplitFaceTags[count] != 0)
                            break;
                        count1++;
                    }*/
                    if (shareSplitFaceTags[count] == 0) // didn't find the neighbor shareSplitVertex face
                    {
                        Debug.Log("Error5 in 'tagFacesSharingSplitVertices'!");
                        return false;
                    }
                }
                count++;
            }
            iterCount++;

            if (totUnhandledVerNum == 0)
            {
                Debug.Log("tagFacesSharingSplitVertices: tagged all of " + numFacesWith3SplitVers.ToString() + " faces with 3 split vertices");
                //Debug.Log("tagFacesSharingSplitVertices: tagged all of " + numFacesWithAmbiguity.ToString() + " end faces with ambiguity");
            }
            if (iterCount >= maxIterNum)
            {
                if (totUnhandledVerNum > 0)
                {
                    Debug.Log("Error6 in 'tagFacesSharingSplitVertices'!");
                    return false;
                }
                break;
            }
        }

        // debug use: display all faces with the same tags: +1
        //count = 0;
        //Vector3 vertex;
        //List<int> verList_d = new List<int>();
        //foreach (KeyValuePair<int, List<int>> kvp in shareSplitVertexFaceList)
        //{
        //    if (shareSplitFaceTags[count] == 1)
        //    {
        //        for(j = 0; j < 3; j++)
        //        {
        //            verList_d.Add(triangles[kvp.Key + j]);
        //        }
        //    }
        //    count++;
        //}
        //verList_d = verList_d.Distinct().ToList();
        //for (i = 0; i < verList_d.Count; i++)
        //{
        //    seleVertexList.indices.Add(verList_d[i]);
        //    vertex = this.transform.TransformPoint(vertices[verList_d[i]]); //convert vertex's local position into world space
        //    seleVertexList.positions.Add(vertex);
        //}
        //

        Debug.Log("Tag all faces sharing the split vertices is done!");

        return true;
    }

    /// split all the faces sharing the split vertices along those vertices based face tags
    // Procedure 
    //      1. Duplicate each split vertex and add duplicates into 'vertices'
    //      2. Update the neirboring spheres for both split vertices and their duplicates
    //      3. Split faces, for each face in 'shareSplitVertexFaceList':
    //          - If face tag == -1, assign duplicate vertices to this face
    //      4. Update vertices, triangles, cloneMesh
    // Input
    //      bFullSplit: true: full-split / false: partial-split
    //      layers2split: first and end layers to be split
    //      shareSplitVertexFaceList: dictionary<face Indices,List<split vertices indices>>
    //      shareSplitFaceTags: List<Tags> for the above face list
    //                              +1: above; -1: below; 2: faces sharing the last split vertices in partial split
    //      startEndSplitVers: "start_outer"/"end_outer"/"start_inner"/"end_inner": vertex index (within 'vertices')
    //      affectedVertexList: List<vertex index> non-split vertices of the faces sharing split vertices (except those faces with tag==2)
    // Output
    //      updated vertices, triangles, cloneMesh
    private bool splitFaces1(bool bFullSplit, int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel, Dictionary<int, List<int>> shareSplitVertexFaceList, 
                                List<int> shareSplitFaceTags, Dictionary<string, int> startEndSplitVers)
    {
        if (splitVertexList.Count <= 0 || shareSplitVertexFaceList.Count <= 0 || shareSplitFaceTags.Count <= 0 || startEndSplitVers.Count <= 0)
        {
            Debug.Log("Error1 in splitFaces1: spliting info empty!");
            return false;
        }

        // obtain the sphere on the other side of spliting line
        int numSpheres = sphereJointModel.m_numSpheres;
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= numSpheres)
            sphereIdxB = sphereIdxB - numSpheres;

        /// update vertices & attributes by splitting each split vertex into two
        int oriVerticeNum = vertices.Length;
        int newVerticeNum = splitVertexList.Count;
        // if partial-split, remove inner_end and outer_end split vertices from duplication
        if (!bFullSplit)
            newVerticeNum -= 2;
        Vector3[] normals;
        Vector2[] uvs;
        normals = clonedMesh.normals;
        uvs = clonedMesh.uv;
        Debug.Log("original vertices num: " + oriVerticeNum.ToString());

        // resize arrays
        Array.Resize(ref vertices, oriVerticeNum + newVerticeNum);
        Array.Resize(ref normals, oriVerticeNum + newVerticeNum);
        Array.Resize(ref uvs, oriVerticeNum + newVerticeNum);

        // adding new vertices and copy attibutes
        int dupVerCount = 0;
        Vector3 vertexL, vertex;
        var oriNewSplitVerPairs = new List<KeyValuePair<int, int>>();
        for (int i = 0; i < splitVertexList.Count; i++)
        {
            if (!bFullSplit && (splitVertexList[i] == startEndSplitVers["end_outer"] || splitVertexList[i] == startEndSplitVers["end_inner"]))
            {
                oriNewSplitVerPairs.Add(new KeyValuePair<int, int>(splitVertexList[i], splitVertexList[i])); // not replicate
            }
            else // replicate the vertex
            {
                // vertices
                vertexL = vertices[splitVertexList[i]];
                // debug use
                vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
                if (sphereJointObjIdx == 0)
                    vertex.x += 0.2f;
                else if (sphereJointObjIdx == 1)
                    vertex.x -= 0.2f;
                vertexL = this.transform.InverseTransformPoint(vertex);
                //
                vertices[oriVerticeNum + dupVerCount] = vertexL;
                oriNewSplitVerPairs.Add(new KeyValuePair<int, int>(splitVertexList[i], oriVerticeNum + dupVerCount));
                // normals
                normals[oriVerticeNum + dupVerCount] = normals[splitVertexList[i]];
                // uvs
                uvs[oriVerticeNum + dupVerCount] = uvs[splitVertexList[i]];
                // neighborSpheres
                //neighborSpheres.Add(new List<List<int>>(neighborSpheres[splitVertexList[i]]));
                neighborSpheres.Add(new List<List<int>>());
                neighborSpheres[oriVerticeNum + dupVerCount].Add(new List<int> { neighborSpheres[splitVertexList[i]][0][0], neighborSpheres[splitVertexList[i]][0][1], neighborSpheres[splitVertexList[i]][0][2] });
                neighborSpheres[oriVerticeNum + dupVerCount].Add(new List<int> { neighborSpheres[splitVertexList[i]][1][0], neighborSpheres[splitVertexList[i]][1][1], neighborSpheres[splitVertexList[i]][1][2] });
                neighborSpheres[oriVerticeNum + dupVerCount].Add(new List<int> { neighborSpheres[splitVertexList[i]][2][0], neighborSpheres[splitVertexList[i]][2][1], neighborSpheres[splitVertexList[i]][2][2] });
                neighborSpheres[oriVerticeNum + dupVerCount].Add(new List<int> { neighborSpheres[splitVertexList[i]][3][0], neighborSpheres[splitVertexList[i]][3][1], neighborSpheres[splitVertexList[i]][3][2] });
                // update split vertex's neighboring spheres
                neighborSpheres[splitVertexList[i]][0][0] = neighborSpheres[splitVertexList[i]][1][0]; // = -1
                neighborSpheres[splitVertexList[i]][0][1] = neighborSpheres[splitVertexList[i]][1][1]; // = -1
                neighborSpheres[splitVertexList[i]][2][0] = neighborSpheres[splitVertexList[i]][3][0]; // = -1
                neighborSpheres[splitVertexList[i]][2][1] = neighborSpheres[splitVertexList[i]][3][1]; // = -1
                // update split vertex-replica's neighboring spheres
                neighborSpheres[oriVerticeNum + dupVerCount][1][0] = neighborSpheres[oriVerticeNum + dupVerCount][0][0]; // = -1;
                neighborSpheres[oriVerticeNum + dupVerCount][1][1] = neighborSpheres[oriVerticeNum + dupVerCount][0][1]; // = -1;
                neighborSpheres[oriVerticeNum + dupVerCount][3][0] = neighborSpheres[oriVerticeNum + dupVerCount][2][0]; // = -1;
                neighborSpheres[oriVerticeNum + dupVerCount][3][1] = neighborSpheres[oriVerticeNum + dupVerCount][2][1]; //= -1;
                // utCoords
                utCoords.Add(new List<float>(utCoords[splitVertexList[i]]));

                dupVerCount++;
            }
        }

        // update affected vertices' neighborhood 
        int afVerIdx = 0;
        int tag, nSSphIdxA, nSSphIdxB;
        for (int i = 0; i < affectedVertexList.Count; i++)
        {
            afVerIdx = affectedVertexList[i];
            nSSphIdxA = neighborSpheres[afVerIdx][0][1];
            nSSphIdxB = neighborSpheres[afVerIdx][1][1];
            tag = aboveOrBelow(afVerIdx, sphereIdxA, sphereIdxB, nSSphIdxA, nSSphIdxB, numSpheres); //1: above/ -1: below
            affectedVerTagList.Add(tag);
            if (neighborSpheres[afVerIdx][2][0] < layers2split[1]) // only update neighborSpheres of affected vertices within fully split sphere-quads
            {
                if (tag == 1/*utCoords[afVerIdx][1] > 0.5*/) //above split line
                {
                    neighborSpheres[afVerIdx][0][0] = neighborSpheres[afVerIdx][1][0];
                    neighborSpheres[afVerIdx][0][1] = neighborSpheres[afVerIdx][1][1];
                    neighborSpheres[afVerIdx][2][0] = neighborSpheres[afVerIdx][3][0];
                    neighborSpheres[afVerIdx][2][1] = neighborSpheres[afVerIdx][3][1];
                }
                else if (tag == -1) // below split line
                {
                    neighborSpheres[afVerIdx][1][0] = neighborSpheres[afVerIdx][0][0];
                    neighborSpheres[afVerIdx][1][1] = neighborSpheres[afVerIdx][0][1];
                    neighborSpheres[afVerIdx][3][0] = neighborSpheres[afVerIdx][2][0];
                    neighborSpheres[afVerIdx][3][1] = neighborSpheres[afVerIdx][2][1];
                }
                else
                {
                    Debug.Log("Error2 in splitFaces1: invalid tag of affected vertex");
                    return false;
                }
            }
            else
                Debug.Log("splitFaces1: outer" + sphereJointObjIdx.ToString() + "find affected vertices within partial-split sphere-quads");
        }

        Debug.Log("resized vertices num: " + vertices.Length.ToString());
        //Array.Resize(ref vertices, oriVerticeNum);
        //Debug.Log("reversed vertices num: " + vertices.Length.ToString());

        /// update triangles
        bool bReplace = false;
        int triID, fverIdx, verIdx, v, j;
        int splitCount = 0;
        foreach (KeyValuePair<int, List<int>> kvp in shareSplitVertexFaceList)
        {
            if (shareSplitFaceTags[splitCount] == -1) // split faces below only
            {
                verIdx = -1;
                triID = kvp.Key;
                for (v = 0; v < 3; v++) // replace all split vertices in the triangle
                {
                    fverIdx = triID + v;
                    verIdx = triangles[fverIdx];
                    bReplace = false;
                    for (j = 0; j < kvp.Value.Count; j++)
                    {
                        if (kvp.Value[j] == verIdx) // find split vertex
                        {
                            foreach (var pair in oriNewSplitVerPairs)
                            {
                                if (pair.Key == verIdx)
                                {
                                    triangles[fverIdx] = pair.Value;
                                    bReplace = true;
                                    break;
                                }
                            }
                        }
                        if (bReplace)
                            break;
                    }
                }
            }
            splitCount++;
        }

        /// update cloned mesh: vertices, normals, triangles..
        clonedMesh.vertices = vertices;
        clonedMesh.triangles = triangles;
        clonedMesh.normals = normals;
        clonedMesh.uv = uvs;
        clonedMesh.RecalculateNormals();

        Debug.Log("splitFaces1 is done!");

        return true;
    }

    // split the mesh based on the given splitting layers of the underlying physics model
    public bool split1(int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel)
    {
        if (sphereJointObjIdx < 0)
        {
            Debug.Log("Error in split: invalid sphereJointObjIdx");
            return false;
        }

        if (neighborSpheres.Count <= 0 || utCoords.Count <= 0)
        {
            Debug.Log("Error in split: neighborspheres or utCoords is empty!");
            return false;
        }

        if (splitVertexList.Count > 0 || affectedVertexList.Count > 0 ||
            splitFaceList.Count > 0 || splitFaceTagList.Count > 0 || startEndSplitVers.Count > 0)
        {
            Debug.Log("Error in split: mesh is alrady split.");
            return false;
        }

        // construct a dictionary of all mesh vertices wrt. all sphere-quads 
        // within the given layers and sphereIdices
        Dictionary<int, List<int>> map = new Dictionary<int, List<int>>(); // key: quad-index (sphereNumPerLayer*layerIdx + sphereIdx); value: vertex index
        if (!buildMap1(layers2split, sphereIdxA, sphereJointModel, ref map))
            return false;

        // get split face list -> splitFaceList<faceIdx, 3 vertex tags>
        Dictionary<int, List<int>> fConnectMap = new Dictionary<int, List<int>>();
        var startEndSplitFaces = new Dictionary<string, int>(); // "start_outer"/"end_outer"/"start_inner"/"end_inner": face index (within 'triangles')
        if (!getSplitFaceList1(layers2split, sphereIdxA, sphereJointModel, map, ref fConnectMap, ref startEndSplitFaces))
            return false;

        // get split vertex list -> splitVertexList<verIdx>
        bFullSplit = false;
        //var startEndSplitVers = new Dictionary<string, int>(); // "start_outer"/"end_outer"/"start_inner"/"end_inner": vertex index (within 'vertices')
        if (!getSplitVertexList1(layers2split, sphereIdxA, sphereJointModel, fConnectMap, startEndSplitFaces, ref bFullSplit, ref startEndSplitVers))
            return false;

        // project split vertices to the midline of sphere-squares to be split
        if (!projSplitVer2Midline1(sphereJointObjIdx, layers2split, sphereIdxA, sphereJointModel))
            return false;

        // tag all the faces sharing the split vertices as above/below
        Dictionary<int, List<int>> shareSplitVertexFaceList = new Dictionary<int, List<int>>();
        List<int> shareSplitFaceTags = new List<int>();
        if (!tagFacesSharingSplitVertices(sphereIdxA, sphereJointModel,fConnectMap,startEndSplitFaces,startEndSplitVers,
                                            ref shareSplitVertexFaceList, ref shareSplitFaceTags))
            return false;

        // split the faces sharing the split vertices along those vertices
        if (!splitFaces1(bFullSplit, layers2split, sphereIdxA, sphereJointModel, shareSplitVertexFaceList, shareSplitFaceTags, startEndSplitVers))
            return false;

        // debug use only
        Vector3 vertex;
        for (int count = 0; count < splitVertexList.Count; count++)
        {
            seleVertexList.indices.Add(splitVertexList[count]);
            vertex = this.transform.TransformPoint(vertices[splitVertexList[count]]); //convert vertex's local position into world space
            seleVertexList.positions.Add(vertex);
        }
        //

        return true;
    }

    /// <summary>
    /// Update all split vertices and affectedVertices' neighboring spheres to join with
    ///     the spheres' of another sphereJointModel (objIdx2Join)
    /// Note: 
    ///     1. Assume the splitting layer indices are the same for both models 
    ///     2. not considering partial case, i.e. startEndSplitVers, for now
    /// </summary>
    /// <returns></returns>
    public bool updateNeighboringSpheres4Join(int objIdx2Join, int[] layers2split, int sphereIdxA, int sphereIdxB)
    {
        if (sphereJointObjIdx < 0)
        {
            Debug.Log("Error(updateNeighboringSphers4Join): invalid sphereJointObjIdx");
            return false;
        }

        if (objIdx2Join < 0)
        {
            Debug.Log("Error(updateNeighboringSphers4Join): invalid objIdx2Join");
            return false;
        }

        if (neighborSpheres.Count <= 0)
        {
            Debug.Log("Error(updateNeighboringSphers4Join): neighborspheres is empty!");
            return false;
        }

        if (splitVertexList.Count <= 0 || affectedVertexList.Count <= 0 ||
            affectedVerTagList.Count <= 0)
        {
            Debug.Log("Error(updateNeighboringSphers4Join): mesh has no split info.");
            return false;
        }

        if (affectedVertexList.Count != affectedVerTagList.Count)
        {
            Debug.Log("Error(updateNeighboringSphers4Join): inconsistent info between affectedVerteList and affectedVerTagList");
            return false;
        }

        // update neighboring spheres for split vertices and their duplicates
        //      split vertices are considered "above": update neighboringSpheres[0],[2]
        //      their duplicates are "below": update neighboringSpheres[1],[3]
        int numSplitVertices = splitVertexList.Count;
        int dupVerStart = verticesNum;//vertices.Length - numSplitVertices;
        int numDupVersCount = 0;
        for (int i = 0; i < numSplitVertices; i++)
        {
            // update neighborSpheres for non-partial split vertices
            if (neighborSpheres[splitVertexList[i]][0][1] == neighborSpheres[splitVertexList[i]][1][1] &&
                neighborSpheres[splitVertexList[i]][2][1] == neighborSpheres[splitVertexList[i]][3][1])
            {
                // split vertices (above)
                neighborSpheres[splitVertexList[i]][0][1] = sphereIdxA;
                neighborSpheres[splitVertexList[i]][0][2] = objIdx2Join;
                neighborSpheres[splitVertexList[i]][2][1] = sphereIdxA;
                neighborSpheres[splitVertexList[i]][2][2] = objIdx2Join;

                // duplicates of split vertices (below)
                neighborSpheres[dupVerStart + numDupVersCount][1][1] = sphereIdxB;
                neighborSpheres[dupVerStart + numDupVersCount][1][2] = objIdx2Join;
                neighborSpheres[dupVerStart + numDupVersCount][3][1] = sphereIdxB;
                neighborSpheres[dupVerStart + numDupVersCount][3][2] = objIdx2Join;

                numDupVersCount++;
            }
        }
        if (numDupVersCount != (vertices.Length - verticesNum))
        {
            Debug.Log("Error(updateNeighboringSphers4Join): incorrect handling of startEndSplitVers!");
            return false;
        }

        // udpate affected vertices' neigbhorhood based on their tags:
        //  "above": update neighboringSpheres[0],[2]
        //  "below": update neighboringSpheres[1],[3]
        for (int i = 0; i < affectedVertexList.Count; i++)
        {
            // update the ones within non-partial split sphere-quads
            if (neighborSpheres[affectedVertexList[i]][0][1] == neighborSpheres[affectedVertexList[i]][1][1] &&
                neighborSpheres[affectedVertexList[i]][2][1] == neighborSpheres[affectedVertexList[i]][3][1])
            {
                if (affectedVerTagList[i] == 1) // above
                {
                    neighborSpheres[affectedVertexList[i]][0][1] = sphereIdxA;
                    neighborSpheres[affectedVertexList[i]][0][2] = objIdx2Join;
                    neighborSpheres[affectedVertexList[i]][2][1] = sphereIdxA;
                    neighborSpheres[affectedVertexList[i]][2][2] = objIdx2Join;
                }
                else if (affectedVerTagList[i] == -1) // below
                {
                    neighborSpheres[affectedVertexList[i]][1][1] = sphereIdxB;
                    neighborSpheres[affectedVertexList[i]][1][2] = objIdx2Join;
                    neighborSpheres[affectedVertexList[i]][3][1] = sphereIdxB;
                    neighborSpheres[affectedVertexList[i]][3][2] = objIdx2Join;
                }
                else
                {
                    Debug.Log("Error(updateNeighboringSphers4Join): invalid affectedVerTagList info!");
                    return false;
                }
            }
            else
                Debug.Log("updateNeighboringSpheres4Join: outer" + sphereJointObjIdx.ToString() + "find affected vertices within partial-split sphere-quads");
        }

        sphereJointObjIdx2Join = objIdx2Join;

        Debug.Log("'updateNeighboringSphers4Join' for colon mesh of " + sphereJointObjName + " succeed!");
        return true;
    }


    // Deform colon mesh based on sphereJointModel
    private bool projSplitVer2Midline(int objIdx)
    {
        if (neighborSpheres.Count <= 0 || utCoords.Count <= 0)
        {
            Debug.Log("Error in projSplitVer2Midline: neighborspheres or utCoords is empty!");
            return false;
        }

        if (splitVertexList.Count <= 0)
        {
            Debug.Log("Error in projSplitVer2Midline: no split vertices found!");
            return false;
        }

        int count1 = 0;
        int layerIdx, sIdx;
        Vector3 newPosWorld, newPosLocal;
        Vector3[] spherePos = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f) };
        GameObject sphere;
        foreach (int count in splitVertexList)
        {
            //vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
            for (count1 = 0; count1 < 4; count1++)
            {
                layerIdx = neighborSpheres[count][count1][0];
                sIdx = neighborSpheres[count][count1][1];
                sphere = GameObject.Find("sphere_" + objIdx.ToString() + "_" + layerIdx.ToString() + "_" + sIdx.ToString());
                if (sphere)
                    spherePos[count1] = sphere.transform.position;
                else
                {
                    Debug.Log("Error in projSplitVer2Midline: cannot find sphere_" + objIdx.ToString() + "_" + layerIdx.ToString() + "_" + sIdx.ToString());
                    Debug.Log("func terminated...");
                    return false;
                }
            }
            // update each split vertex's t coord to be 0.5 (midlien)
            utCoords[count][1] = 0.5f;

            newPosWorld = spherePos[0] + utCoords[count][1] * (spherePos[1] - spherePos[0]) + utCoords[count][0] * (spherePos[2] - spherePos[0]);
            newPosLocal = this.transform.InverseTransformPoint(newPosWorld);
            vertices[count] = newPosLocal;
        }

        // update clone mesh and normals
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();

        Debug.Log("projSplitVer2Midline is done!");
        return true;
    }

    /// <summary>
    /// Classify faces sharing split vertices 
    ///  to be either Above or Below the split line
    /// </summary>
    /// Return: splitFaceTagList
    private bool tagSplitFaces(int sphereIdxA, sphereJointModel sphereJointModel)
    {
        /// find one non-split vertex of one split triangle for side-classification of the split line
        bool bFound = false;
        int triID, fverIdx, verIdx, v;
        List<int> nonSplitVerList = new List<int>();
        foreach (KeyValuePair<int, List<int>> kvp in splitFaceList)
        {
            bFound = false;
            verIdx = -1;
            triID = kvp.Key;
            for (v = 0; v < 3; v++)
            {
                fverIdx = 3 * triID + v;
                verIdx = triangles[fverIdx];
                if (!kvp.Value.Contains(verIdx))
                {
                    bFound = true;
                    break;
                }
            }
            if (bFound && verIdx >= 0 && verIdx < vertices.Length)
                nonSplitVerList.Add(verIdx);
            else
                Debug.Log("Error in tagSplitFaces: cannot find non-split vertex!");
        }
        Debug.Log("# non-split vertices: " + nonSplitVerList.Count.ToString());

        if (splitFaceList.Count != nonSplitVerList.Count)
        {
            Debug.Log("Error in tagSplitFaces: invalid nonSplitVerList!");
            return false;
        }

        /// evaluate which side the non-split vertices are w.r.t splitting line
        ///   based on the indices of the neighboring spheres being split: 
        ///     above: sum > (sphereIdxA + sphereIdxB) 
        ///     below: sum < (sphereIdxA + sphereIdxB)
        // obtain the sphere on the other side of spliting line
        int numSpheres = sphereJointModel.m_numSpheres;
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= numSpheres)
            sphereIdxB = sphereIdxB - numSpheres;
        int layerIdxA, layerIdxB;
        int nSLayerIdxA, nSLayerIdxB, nSSphIdxA, nSSphIdxB; // non-split vertex's neighboring sphere info.
        bool B_LEE_nA = false; // true: sphereIdxB <= nSSphIdxA / false: sphereIdxB > nSSphIdxA
        bool A_LAE_nB = false; // true: sphereIdxA >= nSSpnIdxB / false: sphereIdxA < nSSpnIdxB
        for (int count = 0; count < nonSplitVerList.Count; count++)
        {
            nSLayerIdxA = neighborSpheres[nonSplitVerList[count]][0][0];
            nSLayerIdxB = neighborSpheres[nonSplitVerList[count]][2][0];
            nSSphIdxA = neighborSpheres[nonSplitVerList[count]][0][1];
            nSSphIdxB = neighborSpheres[nonSplitVerList[count]][1][1];
            if (nSSphIdxA == sphereIdxA && nSSphIdxB == sphereIdxB) // within the same split neighbor sphere range
            {
                // get all split vertices of the same face
                foreach (int vidx in splitFaceList.Values.ElementAt(count))
                {
                    layerIdxA = neighborSpheres[vidx][0][0];
                    layerIdxB = neighborSpheres[vidx][2][0];
                    //debug use
                    if (utCoords[nonSplitVerList[count]][1] == utCoords[vidx][1])
                        Debug.Log("Error in tagSplitFaces");
                    //
                    if (utCoords[nonSplitVerList[count]][1] > utCoords[vidx][1]) // above
                    {
                        splitFaceTagList.Add(1);
                        break;
                    }
                    else // below
                    {
                        splitFaceTagList.Add(-1);
                        break;
                    }
                }
            }
            else
            {
                // B_LEE_nA: compare sphereIdxB with nSSphIdxA
                if (Math.Abs(sphereIdxB - nSSphIdxA) <= (numSpheres/2))
                {
                    if (sphereIdxB <= nSSphIdxA)
                        B_LEE_nA = true;
                    else
                        B_LEE_nA = false;
                }
                else
                {
                    if (sphereIdxB > nSSphIdxA)
                        B_LEE_nA = true;
                    else
                        B_LEE_nA = false;
                }
                // A_LAE_nB: compare sphereIdxA with nSSphIdxB
                if (Math.Abs(sphereIdxA - nSSphIdxB) <= (numSpheres / 2))
                {
                    if (sphereIdxA >= nSSphIdxB)
                        A_LAE_nB = true;
                    else
                        A_LAE_nB = false;
                }
                else
                {
                    if (sphereIdxA < nSSphIdxB)
                        A_LAE_nB = true;
                    else
                        A_LAE_nB = false;
                }
                // tag split faces based on B_LEE_nA and A_LAE_nB
                if (B_LEE_nA) // above
                {
                    splitFaceTagList.Add(1);
                }
                else
                {
                    if (A_LAE_nB) // below
                        splitFaceTagList.Add(-1);
                    else
                    {
                        Debug.Log("Error: in tagSplitFaces: evaluate above/below side of non-split vertices");
                        return false;
                    }
                }
            }
            /*
            if ((nSSphIdxA + nSSphIdxB) > (sphereIdxA + sphereIdxB)) // above
                splitFaceTagList.Add(1);
            else if ((nSSphIdxA + nSSphIdxB) < (sphereIdxA + sphereIdxB)) // below
                splitFaceTagList.Add(-1);
            else // within the same split neighbor sphere range
            {
                // get all split vertices of the same face
                foreach (int vidx in splitFaceList.Values.ElementAt(count))
                {
                    layerIdxA = neighborSpheres[vidx][0][0];
                    layerIdxB = neighborSpheres[vidx][2][0];
                    //debug use
                    if (utCoords[nonSplitVerList[count]][1] == utCoords[vidx][1])
                        Debug.Log("Error in tagSplitFaces");
                    //
                    if (utCoords[nonSplitVerList[count]][1] > utCoords[vidx][1]) // above
                    {
                        splitFaceTagList.Add(1);
                        break;
                    }
                    else // below
                    {
                        splitFaceTagList.Add(-1);
                        break;
                    }
                }
            }*/
        }

        if (splitFaceTagList.Count != splitFaceList.Count)
        {
            Debug.Log("Error in tagSplitFaces: unequal num split faces and tags!");
            return false;
        }

        // debug use only
        //Vector3 vertex;
        //for (int count1 = 0; count1 < nonSplitVerList.Count; count1++)
        //{
        //    if (splitFaceTagList[count1] <= 0)
        //        continue;
        //    seleVertexList.indices.Add(nonSplitVerList[count1]);
        //    vertex = this.transform.TransformPoint(vertices[nonSplitVerList[count1]]); //convert vertex's local position into world space
        //    seleVertexList.positions.Add(vertex);
        //}
        //

        return true;
    }

    /// <summary>
    /// Split the cloned mesh along the split line
    ///   by updating vertices, triangles, normals
    /// </summary>
    /// <returns></returns>
    private bool splitFaces()
    {
        if (splitVertexList.Count <= 0 || splitFaceList.Count <= 0 || splitFaceTagList.Count <= 0)
        {
            Debug.Log("Error in splitFaces: spliting info empty!");
            return false;
        }

        /// update vertices & attributes by splitting each split vertex into two
        int oriVerticeNum = vertices.Length;
        int newVerticeNum = splitVertexList.Count;
        Vector3[] normals;
        Vector2[] uvs;
        normals = clonedMesh.normals;
        uvs = clonedMesh.uv;
        Debug.Log("original vertices num: " + oriVerticeNum.ToString());
        
        // resize arrays
        Array.Resize(ref vertices, oriVerticeNum + newVerticeNum);
        Array.Resize(ref normals, oriVerticeNum + newVerticeNum);
        Array.Resize(ref uvs, oriVerticeNum + newVerticeNum);

        // adding new vertices and copy attibutes
        Vector3 vertexL, vertex;
        var oriNewSplitVerPairs = new List<KeyValuePair<int, int>>();
        for (int i = 0; i < newVerticeNum; i++)
        {
            // vertices
            vertexL = vertices[splitVertexList[i]];
            //vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
            //if (sphereJointObjIdx == 0)
            //    vertex.y -= 0.1f;
            //else if (sphereJointObjIdx == 1)
            //    vertex.y += 0.1f;
            //vertexL = this.transform.InverseTransformPoint(vertex);
            vertices[oriVerticeNum + i] = vertexL;
            oriNewSplitVerPairs.Add(new KeyValuePair<int, int>(splitVertexList[i], oriVerticeNum + i));
            // normals
            normals[oriVerticeNum + i] = normals[splitVertexList[i]];
            // uvs
            uvs[oriVerticeNum + i] = uvs[splitVertexList[i]];
            // neighborSpheres
            //neighborSpheres.Add(new List<List<int>>(neighborSpheres[splitVertexList[i]]));
            neighborSpheres.Add(new List<List<int>>());
            neighborSpheres[oriVerticeNum + i].Add(new List<int> { neighborSpheres[splitVertexList[i]][0][0], neighborSpheres[splitVertexList[i]][0][1]});
            neighborSpheres[oriVerticeNum + i].Add(new List<int> { neighborSpheres[splitVertexList[i]][1][0], neighborSpheres[splitVertexList[i]][1][1]});
            neighborSpheres[oriVerticeNum + i].Add(new List<int> { neighborSpheres[splitVertexList[i]][2][0], neighborSpheres[splitVertexList[i]][2][1]});
            neighborSpheres[oriVerticeNum + i].Add(new List<int> { neighborSpheres[splitVertexList[i]][3][0], neighborSpheres[splitVertexList[i]][3][1]});
            // update split vertex's neighboring spheres
            neighborSpheres[splitVertexList[i]][0][0] = neighborSpheres[splitVertexList[i]][1][0]; // = -1
            neighborSpheres[splitVertexList[i]][0][1] = neighborSpheres[splitVertexList[i]][1][1]; // = -1
            neighborSpheres[splitVertexList[i]][2][0] = neighborSpheres[splitVertexList[i]][3][0]; // = -1
            neighborSpheres[splitVertexList[i]][2][1] = neighborSpheres[splitVertexList[i]][3][1]; // = -1
            // update split vertex-replica's neighboring spheres
            neighborSpheres[oriVerticeNum + i][1][0] = neighborSpheres[oriVerticeNum + i][0][0]; // = -1;
            neighborSpheres[oriVerticeNum + i][1][1] = neighborSpheres[oriVerticeNum + i][0][1]; // = -1;
            neighborSpheres[oriVerticeNum + i][3][0] = neighborSpheres[oriVerticeNum + i][2][0]; // = -1;
            neighborSpheres[oriVerticeNum + i][3][1] = neighborSpheres[oriVerticeNum + i][2][1]; //= -1;
            // utCoords
            utCoords.Add(new List<float>(utCoords[splitVertexList[i]]));
        }

        // update affected vertices' neighborhood 
        int afVerIdx = 0;
        for (int i = 0; i < affectedVertexList.Count; i ++)
        {
            afVerIdx = affectedVertexList[i];
            if (utCoords[afVerIdx][1] > 0.5) //above split line
            {
                neighborSpheres[afVerIdx][0][0] = neighborSpheres[afVerIdx][1][0];
                neighborSpheres[afVerIdx][0][1] = neighborSpheres[afVerIdx][1][1]; 
                neighborSpheres[afVerIdx][2][0] = neighborSpheres[afVerIdx][3][0]; 
                neighborSpheres[afVerIdx][2][1] = neighborSpheres[afVerIdx][3][1]; 
            }
            else // below split line
            {
                neighborSpheres[afVerIdx][1][0] = neighborSpheres[afVerIdx][0][0];
                neighborSpheres[afVerIdx][1][1] = neighborSpheres[afVerIdx][0][1];
                neighborSpheres[afVerIdx][3][0] = neighborSpheres[afVerIdx][2][0];
                neighborSpheres[afVerIdx][3][1] = neighborSpheres[afVerIdx][2][1];
            }
        }

        Debug.Log("resized vertices num: " + vertices.Length.ToString());
        //Array.Resize(ref vertices, oriVerticeNum);
        //Debug.Log("reversed vertices num: " + vertices.Length.ToString());

        /// update triangles

        /*int ck = 0;
        for (int k = 0; k < triangles.Length;)
        {
            if (triangles[k] == 294 || triangles[k + 1] == 294 || triangles[k + 2] == 294)
            {
                Debug.Log("found triangle " + ck.ToString());
            }
            k += 3;
            ck++;
        }*/

        bool bReplace = false;
        int triID, fverIdx, verIdx, v, j;
        int splitCount = 0;
        foreach (KeyValuePair<int, List<int>> kvp in splitFaceList)
        {
            if (splitFaceTagList[splitCount] < 0) // split faces below only
            {
                verIdx = -1;
                triID = kvp.Key;
                for (v = 0; v < 3; v++) // replace all split vertices in the triangle
                {
                    fverIdx = 3 * triID + v;
                    verIdx = triangles[fverIdx];
                    bReplace = false;
                    for (j = 0; j < kvp.Value.Count; j++)
                    {
                        if (kvp.Value[j] == verIdx) // find split vertex
                        {
                            foreach (var pair in oriNewSplitVerPairs)
                            {
                                if (pair.Key == verIdx)
                                {
                                    triangles[fverIdx] = pair.Value;
                                    bReplace = true;
                                    break;
                                }
                            }
                        }
                        if (bReplace)
                            break;
                    }
                }
            }
            splitCount++;
        }

        //// debug use
        //triID = 0;
        //int v1, v2, v3;
        //for (int i = 0; i < triangles.Length;)
        //{
        //    v1 = triangles[3 * triID + 0];
        //    v2 = triangles[3 * triID + 1];
        //    v3 = triangles[3 * triID + 2];
        //    if ((v1 == 1200 || v2 == 1200 || v3 == 1200) &&
        //        (v1 == 1197 || v2 == 1197 || v3 == 1197))
        //        Debug.Log("find " + triID.ToString());
        //    triID++;
        //    i += 3;
        //}
        ////

        /// update cloned mesh: vertices, normals, triangles..
        clonedMesh.vertices = vertices;
        clonedMesh.triangles = triangles;
        clonedMesh.normals = normals;
        clonedMesh.uv = uvs;
        clonedMesh.RecalculateNormals();

        return true;
    }

    // split the mesh based on the given splitting layers of the underlying physics model
    public bool split(int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel)
    {
        if (sphereJointObjIdx < 0)
        {
            Debug.Log("Error in split: invalid sphereJointObjIdx");
            return false;
        }

        if (neighborSpheres.Count <= 0 || utCoords.Count <= 0)
        {
            Debug.Log("Error in split: neighborspheres or utCoords is empty!");
            return false;
        }

        if (splitVertexList.Count > 0 || affectedVertexList.Count > 0 ||
            splitFaceList.Count > 0 || splitFaceTagList.Count > 0)
        {
            Debug.Log("Error in split: mesh is alrady split.");
            return false;
        }

        if (!getSplitVertexList(layers2split, sphereIdxA, sphereJointModel))
            return false;
/*
        // get split faces
        if (!getSplitFaceList())
            return false;

        // tag split faces to be above or below spliting line
        if (!tagSplitFaces(sphereIdxA, sphereJointModel))
            return false;

        // project split vertices to the midline of the sphere-quads to be split
        if (!projSplitVer2Midline(sphereJointObjIdx))
            return false;

        // split vertices and faces
        splitFaces();
*/
        // debug use only
        Vector3 vertex;
        for (int count = 0; count < splitVertexList.Count; count++)
        {
            seleVertexList.indices.Add(splitVertexList[count]);
            vertex = this.transform.TransformPoint(vertices[splitVertexList[count]]); //convert vertex's local position into world space
            seleVertexList.positions.Add(vertex);
        }
        //

        return true;
    }

    private void Update()
    {
        // About to enter playmode
        if (!Application.isPlaying && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // same as "Reset" in MeshInspector
            Reset();
            Debug.Log("Mesh is reset!");
            // clear all member variables
            ClearAllData();
            Debug.Log("Mesh member variables are cleared!");
            // clear duplicate tag
            if (bRemoveDuplicate)
            {
                Debug.Log("Mesh duplicate handling is reversed!");
                bRemoveDuplicate = false;
            }
            // clear neighbors (same to 'unbind sphereJointModel')
            if (isBound || neighborSpheres.Count > 0)
            {
                clearNeighbors();
                unbindSphereJointModel();
                Debug.Log("Mesh is unbind!");
            }
            /*if (isBound || neighborSpheres.Count > 0)
            {
                clearNeighbors();
                unbindSphereJointModel();
                Debug.Log("Remove binding before playmode!");
            }
            ClearAllData();*/
        }

        // deform the colon mesh during playmode
        if (Application.isPlaying)
        {
            if (isBound && sphereJointObjIdx >= 0)
            {
                //deform_parallel();
                //deform(sphereJointObjIdx);
                if (physicsModel)
                {
                    if (sphereJointObjIdx2Join >= 0 && physicsModel2Join)
                        deform(physicsModel.m_numSpheres, physicsModel.m_spherePos, physicsModel2Join.m_numSpheres, physicsModel2Join.m_spherePos);
                    else
                        deform(physicsModel.m_numSpheres, physicsModel.m_spherePos, 0, physicsModel.m_spherePos);
                }
            }
        }
    }

    ///// Mesh Processing Functions
    /// Examine mesh face connectivity: 
    //  each face should be shared by three other faces by edges
    public bool examineMeshFaceConnectivity(int layerNum)
    {
        // traverse each face and identify other faces share the same edges
        int i, j, k, count = 0;
        List<int> vers_i = new List<int>();
        List<int> vers_j = new List<int>();
        Dictionary<int, List<int>> fConnectMap = new Dictionary<int, List<int>>();
        for (i = 0; i < triangles.Length; i+=3)
            fConnectMap.Add(i, new List<int>());

        for (i = 0; i < triangles.Length; i +=3)
        {
            vers_i.Clear();
            for (k = 0; k < 3; k++) // vertcies for face i
            {
                vers_i.Add(triangles[i + k]);
            }
            for (j = i + 3; j < triangles.Length; j += 3)
            {
                if (j >= triangles.Length)
                    break;
                vers_j.Clear();
                for (k = 0; k < 3; k++) // vertices for face j
                {
                    vers_j.Add(triangles[j + k]);
                }
                // check if share the same edge (count==2)
                count = 0;
                for (k = 0; k < 3; k++)
                {
                    if (vers_j.Contains(vers_i[k]))
                        count++;
                }
                if (count == 2) // add face j to list of face i
                {
                    fConnectMap[i].Add(j);
                    fConnectMap[j].Add(i);
                }
            }
        }

        // examine fConnectMap: make sure every triangle has 3 edge-shared triangles
        bool bFoundEndPoint = false, bValidConnectivity = true;
        int fIdx, v, layerIdx0, layerIdx1;
        string log;
        List<int> tmpList = new List<int>();
        count = 0;
        foreach (KeyValuePair<int, List<int>> kvp in fConnectMap)
        {
            fIdx = kvp.Key; // current face index
            tmpList = kvp.Value; // other faces sharing same edges
            // check if end face
            //bFoundEndPoint = false;
            //if (tmpList.Count != 3)
            //{
            //    bValidConnectivity = false;
            //    for (k = 0; k < 3; k++) // vertcies for face i
            //    {
            //        v = triangles[fIdx + k];
            //        layerIdx0 = neighborSpheres[v][0][0];
            //        layerIdx1 = neighborSpheres[v][2][0];
            //        if (layerIdx0 == 0 || layerIdx1 == (layerNum-1)) // end point
            //        {
            //            bFoundEndPoint = true;
            //            break;
            //        }
            //    }
            //    if (!bFoundEndPoint) // not end point, just bad connecivity
            //    {
            //        log = "face idx " + fIdx.ToString() + ": ";
            //        for (k = 0; k < tmpList.Count; k++)
            //            log += tmpList[k] + ",";
            //        Debug.Log(log);
            //    }
            //}
            // print all face indices with invalid connecivity
            if (tmpList.Count != 3)
            {
                //log = "face idx " + fIdx.ToString() + ": ";
                //for (k = 0; k < tmpList.Count; k++)
                //    log += tmpList[k] + ",";
                //Debug.Log(log);
                count++;
                bValidConnectivity = false;
            }
        }
        Debug.Log("#faces with invalid connecivity: " + count.ToString());
        Debug.Log("#total faces: " + (triangles.Length / 3).ToString() + "#total vertices: " + vertices.Length.ToString());
        if (!bValidConnectivity)
            return false;
        return true;
    }

    /// Examine duplicated vertices in the mesh
    //   - Check each vertex and record the vertices that share the same position
    //   - Create a mapping between a unique vertex and its duplicates: verDupMap
    public bool checkDuplicateVertices()
    {
        int i, j;
        float sqrLen = 0.0f, closeThreshod = 1.0e-5f;
        Vector3 ver_i, ver_j, offset;
        Dictionary<int, List<int>> verMap = new Dictionary<int, List<int>>(); // duplication info for every mesh vertex
        List<bool> bChecked = new List<bool>(); // flag for each vertex if it's checked
        bDuplicate.Clear();

        // build verMap: mapping between each vertex and other vertices sharing the same position
        for (i = 0; i < vertices.Length; i++)
        {
            verMap.Add(i, new List<int>());
            bChecked.Add(false);
            bDuplicate.Add(false);
        }
        for (i = 0; i < vertices.Length; i++)
        {
            ver_i = vertices[i];
            for (j = i+1; j < vertices.Length; j++)
            {
                if (i == j)
                    continue;
                ver_j = vertices[j];
                offset = ver_i - ver_j;
                sqrLen = offset.sqrMagnitude;
                // duplicate vertex if the distance is smaller than the thredshold
                if (sqrLen < closeThreshod * closeThreshod)
                {
                    // build verMap, for all vertices
                    verMap[i].Add(j);
                    verMap[j].Add(i);
                }
            }
        }
        // build verDupMap: mapping between each unique vertex with their duplicates
        for (i = 0; i < vertices.Length; i++)
        {
            if (bChecked[i] || verMap[i].Count == 0)
                continue;
            verDupMap.Add(i, new List<int>());
            ver_i = vertices[i];
            for (j = i + 1; j < vertices.Length; j++)
            {
                if (i == j)
                    continue;
                ver_j = vertices[j];
                offset = ver_i - ver_j;
                sqrLen = offset.sqrMagnitude;
                // duplicate vertex if the distance is smaller than the thredshold
                if (sqrLen < closeThreshod * closeThreshod)
                {
                    verDupMap[i].Add(j);
                    bChecked[j] = true;
                    bDuplicate[j] = true; // tag the duplicate vertices
                }
            }
            bChecked[i] = true;
        }

        // examine #duplicated vertices that have duplicates
        int verIdx, count = 0;
        List<int> tmpList = new List<int>();
        List<int> dupVerList = new List<int>();// debug use
        foreach (KeyValuePair<int, List<int>> kvp in verMap)
        {
            verIdx = kvp.Key; // current vertex index
            tmpList = kvp.Value; // other vertices sharing same position
            if (tmpList.Count > 0)
            {
                count++;
                // debug use
                //dupVerList.Add(verIdx);
                //
            }
        }
        // debug use - display duplicated vertices
        //Vector3 vertex;
        //for (count1 = 0; count1 < dupVerList.Count; count1++)
        //{
        //    seleVertexList.indices.Add(dupVerList[count1]);
        //    vertex = this.transform.TransformPoint(vertices[dupVerList[count1]]); //convert vertex's local position into world space
        //    seleVertexList.positions.Add(vertex);
        //}
        //

        // check #duplicated vertices to be removed from triangles
        tmpList.Clear();
        int count1 = 0;
        foreach (KeyValuePair<int, List<int>> kvp in verDupMap)
        {
            verIdx = kvp.Key;
            tmpList = kvp.Value;
            count1 += tmpList.Count; // #duplicated vertices to be removed
        }
        Debug.Log("#rep vertices: " + verDupMap.Count.ToString() + ", #dup vertices to remove: " + count1.ToString());

        // examine bDuplicate tags
        int count2 = 0;
        for (i = 0; i < bDuplicate.Count; i++)
        {
            if (bDuplicate[i])
                count2++;
        }
        Debug.Log("#bDuplicate: " + count2.ToString()); //should be same as #dup vertices to remove

        // clear
        verMap.Clear();
        dupVerList.Clear();

        if (count > 0)
        {
            Debug.Log("#duplicated vertices: " + count.ToString());
            return false;
        }
        Debug.Log("mesh has no duplicated vertices!");
        return true;
    }

    /// Remove vertex duplicates from triangles
    //    For each traingle, traverse all vertex duplicates (values) in verDupMap
    //      & replace the duplicate with the original vertex (key) in the triangle
    public void removeDuplicateVertices()
    {
        if (verDupMap.Count <= 0)
        {
            Debug.Log("removeDuplicateVertices not complete: verDupMap empty!");
            return;
        }
        // traverse each triangle in triangles
        bool bFoundMatch = false;
        int i, j, k;
        int verIdx_tri, verIdx_key, verIdx_dup;
        List<int> dupVerList = new List<int>();
        List<int> faceList = new List<int>();
        for (i = 0; i < triangles.Length; i += 3)
        {
            for (j = 0; j < 3; j++)
            {
                verIdx_tri = triangles[i + j];
                // traverse duplicated vertices in the map
                bFoundMatch = false;
                foreach (KeyValuePair<int, List<int>> kvp in verDupMap)
                {
                    verIdx_key = kvp.Key;
                    dupVerList = kvp.Value;
                    for (k=0; k < dupVerList.Count; k++)
                    {
                        verIdx_dup = dupVerList[k];
                        // check if the triangle has the dup vertex
                        if (verIdx_tri == verIdx_dup)
                        {
                            // record such triangles
                            faceList.Add(i); 
                            // replace duplicated vertex with the original one in trianlge
                            triangles[i + j] = verIdx_key;
                            bFoundMatch = true;
                            break;
                        }
                    }
                    if (bFoundMatch)
                        break;
                }
            }
        }
        // summarize #faces use duplciated vertices
        List<int> distFaceList = faceList.Distinct().ToList();
        Debug.Log("# faces sharing duplicated vertices: " + distFaceList.Count.ToString());

        // update cloned mesh
        if (distFaceList.Count > 0)
            clonedMesh.triangles = triangles;

        // clear data
        faceList.Clear();
        distFaceList.Clear();
        dupVerList.Clear();
    }

    /// Evaluate each triangle belonging to inner/outer/section layer of the colon model (before binding)
    //   - Identify triangles on sections: all 3 vertices have similar z coordinates
    //   - Identify triangles in inner and outer layers based on their normals
    //   - Return: int[] triIOLayerInfo: length: #triangles, 0: section; 1: outer; -1: inner
    public void evaluateTriIOLayerInfo()
    {
        triIOLayerInfo.Clear();

        // Identify each "section triangle"
        //  whose 3 vertices: maxDeltaZ < thresholdZ
        float maxDeltaZ = 0.0f, thresholdZ = 0.01f;
        int i, j, k;
        float[] deltaZ = { 0.0f, 0.0f, 0.0f };
        Vector3 vertexL, vertex;
        List<Vector3> vers = new List<Vector3>();
        for (i = 0; i < triangles.Length; i += 3)
        {
            vers.Clear();
            for (k = 0; k < 3; k++) // vertcies for face i
            {
                vertexL = vertices[triangles[i + k]];
                vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
                vers.Add(vertex);
            }
            deltaZ[0] = Mathf.Abs(vers[0].z - vers[1].z);
            deltaZ[1] = Mathf.Abs(vers[1].z - vers[2].z);
            deltaZ[2] = Mathf.Abs(vers[0].z - vers[2].z);
            maxDeltaZ = Mathf.Max(deltaZ);
            if (maxDeltaZ < thresholdZ)
                triIOLayerInfo.Add(0); // section triangle
            else
                triIOLayerInfo.Add(-1); // inner/outer layer
        }
        // Identify inner/outer layer
        Bounds bbox = clonedMesh.bounds;
        Vector3 layerCenterL = bbox.center;
        Vector3 layerCenter = this.transform.TransformPoint(layerCenterL); //convert vertex's local position into world space
        Vector3 normal, center2Vertex;
        //Debug.Log("layerCenterL: " + layerCenterL.x.ToString() + "," + layerCenterL.y.ToString() + "," + layerCenterL.z.ToString());
        //Debug.Log("layerCenter: " + layerCenter.x.ToString() + "," + layerCenter.y.ToString() + "," + layerCenter.z.ToString());
        for (i = 0; i < triangles.Length; i += 3)
        {
            j = i / 3;
            if (triIOLayerInfo[j] == 0) // skip section triangle
                continue;
            vertexL = vertices[triangles[i + 0]];
            vertex = this.transform.TransformPoint(vertexL); //convert vertex's local position into world space
            layerCenter.z = vertex.z; // project the model center to the layer center along z
            center2Vertex = vertex - layerCenter;
            normal = this.transform.TransformPoint(clonedMesh.normals[triangles[i + 0]]); // convert vertex's local normal into world space
            //normal.Normalize();
            // examine the angle between center2Vertex and normal
            if (Vector3.Dot(center2Vertex, normal) > 0.0f) // outer layer
                triIOLayerInfo[j] = 1;
        }

        // examine #triangles of section, inner, outer layers
        int num_section = 0, num_inner = 0, num_outer = 0;
        for (i = 0; i < triIOLayerInfo.Count; i++)
        {
            if (triIOLayerInfo[i] == 0)
                num_section++;
            else if (triIOLayerInfo[i] == 1)
                num_outer++;
            else if (triIOLayerInfo[i] == -1)
                num_inner++;
        }
        Debug.Log("#triangles: " + triIOLayerInfo.Count.ToString() + ", " +
                    "#num_sec: " + num_section.ToString() + ", " +
                    "#num_inner: " + num_inner.ToString() + ", " +
                    "#num_outer: " + num_outer.ToString());

        /// debug use only: display section triangles using seleVertexList
        /*int count = 0;
        List<int> sectionTriVerList = new List<int>();
        for (count = 0; count < triIOLayerInfo.Count; count++)
        {
            if (triIOLayerInfo[count] >= 0)
                continue;
            for (k = 0; k < 3; k++)
            {
                sectionTriVerList.Add(triangles[3*count + k]);
            }
        }
        // remove repeated face indices
        List<int> distSectionTriVerList = new List<int>();
        distSectionTriVerList = sectionTriVerList.Distinct().ToList();

        // display
        for (count = 0; count < distSectionTriVerList.Count; count++)
        {
            seleVertexList.indices.Add(distSectionTriVerList[count]);
            vertex = this.transform.TransformPoint(vertices[distSectionTriVerList[count]]); //convert vertex's local position into world space
            seleVertexList.positions.Add(vertex);
        }*/
        ///
    }

    /// test vertex augment and recover
    private void testArrayEnlargeAndShrink()
    {
        int[] originVers = { 0, 1, 2 };
        Debug.Log("OriginVers: ");
        foreach (int i in originVers)
            Debug.Log(i.ToString() + ",");
        int[] clonedVers;
        clonedVers = originVers;
        Debug.Log("ClonedVers: ");
        foreach (int i in clonedVers)
            Debug.Log(i.ToString() + ",");
        int[] newVers = { 3, 4 };
        Array.Resize(ref clonedVers, clonedVers.Length + newVers.Length);
        Debug.Log("Enlarged ClonedVers: ");
        for (int count = 0; count < clonedVers.Length; count++)
        {
            if (count >= originVers.Length)
                clonedVers[count] = newVers[count - originVers.Length];
            Debug.Log(clonedVers[count].ToString() + ",");
        }
        Array.Resize(ref clonedVers, originVers.Length);
        Debug.Log("Reversed ClonedVers: ");
        foreach (int i in clonedVers)
            Debug.Log(i.ToString() + ",");
    }


    /// ========= Prior version of finding split vertices and split faces (before 7/22) ======== ///
    // build a map between sphere-squares to be splited and mesh vertices inside
    private bool buildMap(int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel, ref Dictionary<int, List<int>> map)
    {
        // obtain the sphere on the other side of spliting line
        int numSpheres = sphereJointModel.m_numSpheres;
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= numSpheres)
            sphereIdxB = sphereIdxB - numSpheres;

        // iterate mesh vertices to find the ones within the sphere quads to be split
        int count1, count2;
        int layerIdx, sIdx;
        Vector3 vertex;
        for (int count = 0; count < vertices.Length; count++)
        {
            count2 = 0;
            vertex = this.transform.TransformPoint(vertices[count]); //convert vertex's local position into world space
            for (count1 = 0; count1 < 4; count1++)
            {
                layerIdx = neighborSpheres[count][count1][0];
                sIdx = neighborSpheres[count][count1][1];
                // check if all the neighborspheres are split
                if ((layerIdx >= layers2split[0] && layerIdx <= layers2split[1]) &&
                    (sIdx == sphereIdxA || sIdx == sphereIdxB))
                {
                    count2++;
                }
            }
            // highlight the vertices within split sphere quads
            if (count2 == 4)
            {
                // save split mesh vertices by the first sphere layers
                layerIdx = neighborSpheres[count][0][0];
                // add the vertex to the map
                if (!map.ContainsKey(layerIdx))
                    map.Add(layerIdx, new List<int>());
                map[layerIdx].Add(count);
                //splitVertexList.Add(count);
            }
        }

        if (map.Count <= 0)
        {
            Debug.Log("Error in split: no vertices to split!");
            return false;
        }

        return true;
    }

    // process the map to make sure every sphere-quad got a mesh vertex to be split
    private bool processMap(int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel, ref Dictionary<int, List<int>> map)
    {
        // sort the map by the keys (layer index) in ascending order
        map = map.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        // traverse the map to identify the layers without any vertex
        //  layer indices in map should be continuous, offset == 1
        int lastLayer = layers2split[0] - 1;
        int layerIdx, i, offset;
        List<int> layerlist = new List<int>();
        foreach (KeyValuePair<int, List<int>> kvp in map)
        {
            layerIdx = kvp.Key;
            offset = layerIdx - lastLayer;
            if ((offset) > 1)
            {
                for (i = 1; i < offset; i ++)
                {
                    layerlist.Add(lastLayer + i);
                }
            }
            lastLayer = layerIdx;
        }

        // make sure each quad got a least one vertex
        //  connecting to the vertices in the neighboring quads
        List<int> tempList = new List<int>();
        foreach (KeyValuePair<int, List<int>> kvp in map)
        {
            layerIdx = kvp.Key;
            tempList.Clear();
            tempList = kvp.Value;
        }

        return true;
    }

    // check if two vertices are from the same triangle
    private bool bInSameTriangle(int verA, int verB)
    {
        // traverse the entire triangle list
        int v;
        List<int> list = new List<int>();
        for (int t = 0; t < triangles.Length; t += 3)
        {
            list.Clear();
            for (v = 0; v < 3; v++)
            {
                list.Add(triangles[t + v]);
            }
            if (list.Contains(verA) && list.Contains(verB))
                return true;
        }
        return false;
    }

    private void filterSplitVertexList(List<int> potenSplitVerList, ref List<bool> bKeep)
    {
        int i, j;
        int verA, verB;
        float u_delta, t_delta, t_delA, t_delB;
        List<float> utA = new List<float>();
        List<float> utB = new List<float>();

        for (i = 0; i < (potenSplitVerList.Count - 1); i++)
        {
            if (!bKeep[i])
                continue;
            verA = potenSplitVerList[i];
            for (j = i + 1; j < potenSplitVerList.Count; j++)
            {
                if (!bKeep[j])
                    continue;
                verB = potenSplitVerList[j];
                if (bInSameTriangle(verA, verB))
                {
                    utA = utCoords[verA];
                    utB = utCoords[verB];
                    u_delta = Mathf.Abs(utA[0] - utB[0]);
                    t_delta = Mathf.Abs(utA[1] - utB[1]);
                    if (t_delta > u_delta) // found the vertical edge
                    {
                        t_delA = Mathf.Abs(utA[1] - 0.5f);
                        t_delB = Mathf.Abs(utB[1] - 0.5f);
                        if (t_delA < t_delB) // remove the one further from the midline
                            bKeep[j] = false; // remove verB
                        else
                            bKeep[i] = false; // remove verA
                    }
                }
            }
        }
    }
    private bool getSplitVertexList(int[] layers2split, int sphereIdxA, sphereJointModel sphereJointModel)
    {
        // construct a dictionary of all mesh vertices wrt. all sphere-quads 
        // within the given layers and sphereIdices
        Dictionary<int, List<int>> map = new Dictionary<int, List<int>>(); // key: quad-index (sphereNumPerLayer*layerIdx + sphereIdx); value: vertex index

        if (!buildMap(layers2split, sphereIdxA, sphereJointModel, ref map))
            return false;

        // Iterate the map and add single vertex of each sphere-quad into split vertex list
        // and 2) obtain a list of multiple vertices within the same sphere-quad
        int count;
        List<int> tempList = new List<int>();
        foreach (KeyValuePair<int, List<int>> kvp in map)
        {
            tempList.Clear();
            tempList = kvp.Value;
            List<bool> bKeep = new List<bool>();
            for (count = 0; count < tempList.Count; count++)
                bKeep.Add(true);

            if (tempList.Count == 1)
            {
                splitVertexList.Add(tempList[0]);
                continue;
            }
            // filter tempList
            filterSplitVertexList(tempList, ref bKeep);

            for (count = 0; count < bKeep.Count; count++)
            {
                if (bKeep[count])
                    splitVertexList.Add(tempList[count]);
                else
                {
                    affectedVertexList.Add(tempList[count]);
                    //Debug.Log("Remove vertex: " + tempList[count].ToString());
                }
            }
        }

        // clear all map and lists
        map.Clear();

        if (splitVertexList.Count <= 0)
        {
            Debug.Log("Error in getSplitVertexList: no split vertices found!");
            return false;
        }

        Debug.Log("# split vertices: " + splitVertexList.Count.ToString());
        Debug.Log("# affected vertices: " + affectedVertexList.Count.ToString());

        return true;
    }

    // get all the triangles that share the splitVertexList
    private bool getSplitFaceList()
    {
        if (neighborSpheres.Count <= 0 || utCoords.Count <= 0)
        {
            Debug.Log("Error in getSplitFaceList: neighborspheres or utCoords is empty!");
            return false;
        }

        if (splitVertexList.Count <= 0)
        {
            Debug.Log("Error in getSplitFaceList: no split vertices found!");
            return false;
        }

        // construct a dictionary between triangleIDs (keys) to split-vertices (values)
        //  to find all triangles sharing the split-vertices
        int t, v, count;
        List<int> list = new List<int>();
        foreach (int s in splitVertexList)
        {
            count = 0;
            for (t = 0; t < triangles.Length; t += 3)
            {
                list.Clear();
                for (v = 0; v < 3; v++)
                {
                    list.Add(triangles[t + v]);
                }

                if (list.Contains(s))
                {
                    if (!splitFaceList.ContainsKey(count))
                        splitFaceList.Add(count, new List<int>());
                    splitFaceList[count].Add(s);
                }
                count++;
            }
        }
        if (splitFaceList.Count <= 0)
        {
            Debug.Log("Error in getSplitFaceList: no split triangles found!");
            return false;
        }
        Debug.Log("# split triangles: " + splitFaceList.Count.ToString());      

        return true;
    }
}
