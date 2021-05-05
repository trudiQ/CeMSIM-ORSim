﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orignally derived from 'testSceneOperations.cs'
/// </summary>
public class globalOperators : MonoBehaviour
{
    // sphereJoint models
    public int m_numSphereModels = 0; // specified when creating sphereJointModels
    public GameObject[] m_sphereJointObjs; 
    public sphereJointModel[] m_sphereJointModels;

    // meshes
    public int m_numBindColonMeshes = 0; // #colon meshs bound to sphereJointModels
    public GameObject[] m_colonMeshObjs;
    public colonMesh[] m_colonMeshes;

    // opeartors related variables
    public bool[] m_bCornerCut = { false, false };
    public bool m_bSplit = false;
    public bool m_bJoin = false;
    public bool m_bFinalClosure = false;
    public int[] m_layers2Split = { -1, -1 }; // same for both models
    public int[] m_sphereIdx2Split = { -1, -1 }; // the first splitting sphere index for each model

    // Start is called before the first frame update
    void Start()
    {
        /// access sphereJointModels and colon meshes
        if (m_numSphereModels <= 0)
        {
            Debug.Log("Error, m_numSphereModels not specified!");
            return;
        }

        // ==> always assume models to be processed are sphereJointGameObj0 (outer0), sphereJointGameObj1 (outer1)
        m_sphereJointObjs = new GameObject[m_numSphereModels];
        m_sphereJointModels = new sphereJointModel[m_numSphereModels];
        m_colonMeshObjs = new GameObject[m_numSphereModels];
        m_colonMeshes = new colonMesh[m_numSphereModels];
        for(int i = 0; i < m_numSphereModels; i++)
        {
            m_sphereJointObjs[i] = GameObject.Find("sphereJointGameObj" + i.ToString());
            m_sphereJointModels[i] = m_sphereJointObjs[i].GetComponent<sphereJointModel>();

            if (!m_sphereJointObjs[i] || !m_sphereJointModels[i])
                return;
            
            // restore the bound colon mesh
            if (m_sphereJointModels[i].m_bFindColonMesh)
            {
                m_colonMeshObjs[i] = GameObject.Find("outer" + i.ToString());
                m_colonMeshes[i] = m_colonMeshObjs[i].GetComponent<colonMesh>();
                m_numBindColonMeshes += 1;
            }
        }

        /// initialize opeartor variables
        // split & joint
        m_layers2Split[0] = 0;
        m_layers2Split[1] = 10;//14
        m_sphereIdx2Split[0] = 3;//4 //5
        m_sphereIdx2Split[1] = 14;//16 //14
    }

    /// <summary>
    /// Split all colon models in the scene at once 
    ///     Note: need to identify which ones to split when more than 2 models
    /// Input: 
    ///     int[] m_layers2Split: assume the splitting layers for both models are the same
    ///     int[] m_sphereIdx2Split: first, second sphereIdx to split
    /// </summary>
    bool split()
    {
        if (m_numSphereModels <= 0)
        {
            Debug.Log("Error(split): no models to split!");
            return false;
        }
        if (m_numBindColonMeshes != m_numSphereModels)
        {
            Debug.Log("Error(split): unmatched colon mesh and sphereJoint models!");
            return false;
        }

        // split the mesh and sphereJointModel for each colon
        for(int i = 0; i < m_numSphereModels; i++)
        {
            // Processing colonMesh and bind it to sphereJointModel
            if (!Application.isPlaying) // Edit mode
            {
                // examine and handle duplicated vertices
                if (!m_colonMeshes[i].bRemoveDuplicate && m_colonMeshes[i].verDupMap.Count <= 0)
                {
                    if (!m_colonMeshes[i].checkDuplicateVertices())
                    {
                        m_colonMeshes[i].removeDuplicateVertices();
                        m_colonMeshes[i].bRemoveDuplicate = true;
                    }
                    Debug.Log("Duplicated already handled");
                }
                // check inner/outer layer info
                if (m_colonMeshes[i].triIOLayerInfo.Count <= 0)
                {
                    m_colonMeshes[i].evaluateTriIOLayerInfo();
                    Debug.Log("Done build inner/outer layer info");
                }
                // bind colon mesh to sphereJointModel
                if (!m_colonMeshes[i].isBound && m_colonMeshes[i].neighborSpheres.Count == 0)
                {
                    if (m_sphereJointModels[i])
                    {
                        if (m_colonMeshes[i].neighborSphereSearching1(m_sphereJointModels[i]))
                        {
                            m_colonMeshes[i].bindSphereJointModel();
                            m_colonMeshes[i].isBound = true;
                        }
                    }
                    Debug.Log("Binding sphereJointModel done!");
                }
            }

            // colonMesh
            if (m_colonMeshes[i].split1(m_layers2Split, m_sphereIdx2Split[i], m_sphereJointModels[i]))
                Debug.Log("Split done for outer " + i.ToString());
            else
            {
                Debug.Log("Split failed for outer " + i.ToString());
                return false;
            }

            // sphereJointModel
            m_sphereJointModels[i].splitInlayerJoints_sigle(ref m_layers2Split, m_sphereIdx2Split[i]);
        }

        m_bSplit = true;
        return true;
    }

    /// <summary>
    /// Description: Join two sphereJoint models that have been split 
    ///     Note: need to identify which ones to join when more than 2 models
    /// Method:
    ///     1. Move the spheres to be joined between two models to the mid point and contact with each other
    ///     2. Add only one joint between the spheres of the same layer of across the two models
    ///         m_sphereIdx2Split[0] -> m_sphereIdx2Split[1]+1
    ///         m_sphereIdx2Split[1] -> m_sphereIdx2Split[0]+1
    /// Input: 
    ///     int[] m_layers2Split: assume the layers for both models to be joined are the same
    ///     int[] m_sphereIdx2Split: the first sphereIdx of the first and second colons to join
    /// </summary>
    bool joinSphereJointModels(int numLayer2Join, int[] objIdx, int[] sphereIdxB, float[] sphereRadius)
    {
        // Adding two new joints between two models
        //      m_sphereIdx2Split[0] -> sphereIdxB[1] &
        //      m_sphereIdx2Split[1] -> sphereIdxB[0]
        int numJointsAdded = 0;
        float dist0A1B, dist1A0B; // initial distance between each pair of spheres to join
        Vector3 vec0A1B, vec1A0B; // unit vector between each pair of spheres to join
        for (int l = 0; l < numLayer2Join; l++)
        {
            // connect sphereA of object0 to sphereB of object1
            GameObject sphere0A = GameObject.Find("sphere_" + objIdx[0].ToString() + "_" + (m_layers2Split[0] + l).ToString() + "_" + m_sphereIdx2Split[0].ToString());
            GameObject sphere1B = GameObject.Find("sphere_" + objIdx[1].ToString() + "_" + (m_layers2Split[0] + l).ToString() + "_" + sphereIdxB[1].ToString());
            if (sphere0A && sphere1B)
            {
                // move the spheres to the middle
                vec0A1B = sphere1B.transform.position - sphere0A.transform.position; // 0A->1B
                vec0A1B.Normalize();
                dist0A1B = Vector3.Distance(sphere0A.transform.position, sphere1B.transform.position);
                sphere0A.transform.position = sphere0A.transform.position + (0.5f * dist0A1B - sphereRadius[0]) * vec0A1B;
                sphere1B.transform.position = sphere1B.transform.position - (0.5f * dist0A1B - sphereRadius[1]) * vec0A1B;
                // add a fixed joint
                FixedJoint joint0A1B = sphere0A.AddComponent<FixedJoint>();
                joint0A1B.connectedBody = sphere1B.GetComponent<Rigidbody>();
                // reconfigure the joint
                //joint0A1B.linearLimitSpring = new SoftJointLimitSpring() { spring = 50.0f, damper = 50.0f };
                //joint0A1B.linearLimit = new SoftJointLimit() { limit = 0.01f };
                //joint0A1B.autoConfigureConnectedAnchor = false;
                //joint0A1B.connectedAnchor = new Vector3(0, 0, 0);
                // update jointInfo of sphere0A
                sphereJointsInfo joint0A1BInfo = sphere0A.GetComponent<sphereJointsInfo>();
                joint0A1BInfo.m_inLayerJointList[joint0A1BInfo.m_inLayerJointNum, 0] = joint0A1BInfo.m_inLayerJointNum;
                joint0A1BInfo.m_inLayerJointList[joint0A1BInfo.m_inLayerJointNum, 1] = objIdx[1];
                joint0A1BInfo.m_inLayerJointList[joint0A1BInfo.m_inLayerJointNum, 2] = m_layers2Split[0] + l;
                joint0A1BInfo.m_inLayerJointList[joint0A1BInfo.m_inLayerJointNum, 3] = sphereIdxB[1];
                joint0A1BInfo.m_inLayerJointNum += 1;
                numJointsAdded += 1;
            }

            // connect sphereA of object1 to sphereB of object0
            GameObject sphere1A = GameObject.Find("sphere_" + objIdx[1].ToString() + "_" + (m_layers2Split[0] + l).ToString() + "_" + m_sphereIdx2Split[1].ToString());
            GameObject sphere0B = GameObject.Find("sphere_" + objIdx[0].ToString() + "_" + (m_layers2Split[0] + l).ToString() + "_" + sphereIdxB[0].ToString());
            if (sphere1A && sphere0B)
            {
                // move the spheres to the middle
                //vec1A0B = sphere0B.transform.position - sphere1A.transform.position; // 1A->0B
                vec1A0B = sphere1A.transform.position - sphere0B.transform.position; // 1A->0B
                vec1A0B.Normalize();
                dist1A0B = Vector3.Distance(sphere1A.transform.position, sphere0B.transform.position);
                //sphere1A.transform.position = sphere1A.transform.position + (0.5f * dist1A0B - sphereRadius[1]) * vec1A0B;
                //sphere0B.transform.position = sphere0B.transform.position - (0.5f * dist1A0B - sphereRadius[0]) * vec1A0B;
                // add a fixed joint
                FixedJoint joint1A0B = sphere1A.AddComponent<FixedJoint>();
                joint1A0B.connectedBody = sphere0B.GetComponent<Rigidbody>();
                // reconfigure the joint
                //joint1A0B.linearLimitSpring = new SoftJointLimitSpring() { spring = 50.0f, damper = 50.0f };
                //joint1A0B.linearLimit = new SoftJointLimit() { limit = 0.01f };
                //joint1A0B.autoConfigureConnectedAnchor = false;
                //joint1A0B.connectedAnchor = new Vector3(0, 0, 0);
                // update jointInfo of sphere0A
                sphereJointsInfo joint1A0BInfo = sphere1A.GetComponent<sphereJointsInfo>();
                joint1A0BInfo.m_inLayerJointList[joint1A0BInfo.m_inLayerJointNum, 0] = joint1A0BInfo.m_inLayerJointNum;
                joint1A0BInfo.m_inLayerJointList[joint1A0BInfo.m_inLayerJointNum, 1] = objIdx[0];
                joint1A0BInfo.m_inLayerJointList[joint1A0BInfo.m_inLayerJointNum, 2] = m_layers2Split[0] + l;
                joint1A0BInfo.m_inLayerJointList[joint1A0BInfo.m_inLayerJointNum, 3] = sphereIdxB[0];
                joint1A0BInfo.m_inLayerJointNum += 1;
                numJointsAdded += 1;
            }
        }

        if (numJointsAdded != 2 * numLayer2Join)
        {
            Debug.Log("Error(joinSphereJointModels): new joints of some layers were not added successfully!");
            Debug.Log("---- Should delete & re-create the sphereJointModels and re-do the operation again");
            return false;
        }

        Debug.Log("joinSphereJointModels: All layers of two sphereJointModels are joined!");
        return true;
    }

    /// <summary>
    /// Method:
    ///     1. join two sphereJointModels
    ///     2. update mesh vertices' neighboring spheres for joining with new sphereJointModels
    /// </summary>
    /// <returns></returns>
    bool join()
    {
        if (m_numSphereModels < 2)
        {
            Debug.Log("Error(join): < 2 colons to join!");
            return false;
        }

        /// join sphereJointModels
        // prepare for splitting layer and sphereIdx attributes
        int numLayer2Join = m_layers2Split[1] - m_layers2Split[0] + 1;
        int[] objIdx = new int[m_numSphereModels];
        int[] numSpheres = new int[m_numSphereModels];
        int[] sphereIdxB = new int[m_numSphereModels];
        float[] sphereRadius = new float[m_numSphereModels];
        for (int i = 0; i < m_numSphereModels; i++)
        {
            objIdx[i] = m_sphereJointModels[i].m_objIndex;
            numSpheres[i] = m_sphereJointModels[i].m_numSpheres;
            sphereIdxB[i] = m_sphereIdx2Split[i] + 1;
            if (sphereIdxB[i] >= numSpheres[i])
                sphereIdxB[i] = sphereIdxB[i] - numSpheres[i];
            sphereRadius[i] = m_sphereJointModels[i].m_sphereRadius;
        }

        // join
        if (!joinSphereJointModels(numLayer2Join, objIdx, sphereIdxB, sphereRadius))
            return false;

        // udpate the vertices' neighboring spheres for colon meshes
        bool[] bSucceed = new bool[m_numSphereModels];
        bSucceed[0] = m_colonMeshes[0].updateNeighboringSpheres4Join(objIdx[1], m_layers2Split, m_sphereIdx2Split[1], sphereIdxB[1]);
        bSucceed[1] = m_colonMeshes[1].updateNeighboringSpheres4Join(objIdx[0], m_layers2Split, m_sphereIdx2Split[0], sphereIdxB[0]);

        if (!bSucceed[0] || !bSucceed[1])
        {
            Debug.Log("Join failed");
            return false;
        }

        // update colon meshes' sphereJointModel2Join
        m_colonMeshes[0].physicsModel2Join = m_sphereJointModels[1];
        m_colonMeshes[1].physicsModel2Join = m_sphereJointModels[0];
        m_bJoin = true;

        Debug.Log("Join succeed!");
        return true;
    }

    bool cornerCut()
    {
        if (m_numSphereModels <= 0)
        {
            Debug.Log("Error(cornerCut): no models to conduct corner-cut!");
            return false;
        }

        if (m_bSplit || m_bJoin)
        {
            Debug.Log("Error(cornerCut): colons are already split or joined!");
            return false;
        }

        // cut both models at the same time for now
        int layerIdx = 0; // layer to cut-corner
        int[] cutCornerSphereIndices;
        for (int objIdx = 0; objIdx < m_numSphereModels; objIdx++)
        {
            if (objIdx == 0)
            {
                cutCornerSphereIndices = new int[] { 6, 7, 8 };
                if (!m_sphereJointModels[objIdx].cornerCut(layerIdx, cutCornerSphereIndices))
                    return false;
                m_bCornerCut[0] = true;
                Debug.Log("cornerCut done for sphereJointModel" + objIdx.ToString());
            }
            else if (objIdx == 1)
            {
                cutCornerSphereIndices = new int[] { 13, 12, 11 };
                if (!m_sphereJointModels[objIdx].cornerCut(layerIdx, cutCornerSphereIndices))
                    return false;
                m_bCornerCut[1] = true;
                Debug.Log("cornerCut done for sphereJointModel" + objIdx.ToString());
            }
        }

        return true;
    }

    bool finalClosure(int layerIdx)
    {
        if (m_numSphereModels <= 0)
        {
            Debug.Log("Error(finalClosure): no models to conduct finalClosure!");
            return false;
        }

        // conduct final-closure for both models at the same time
        List<bool> bFinalClosure = new List<bool>();
        for (int objIdx = 0; objIdx < m_numSphereModels; objIdx++)
        {
            bFinalClosure.Add(false);
            if (m_sphereJointModels[objIdx].closeupLayers(layerIdx))
                bFinalClosure[bFinalClosure.Count - 1] = true;
            else
            {
                Debug.Log("Error(finalClosure: failed on sphereJointModel" + objIdx.ToString());
                return false;
            }
        }

        m_bFinalClosure = true;
        Debug.Log("FinalClosure succeeds!");
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            // 'C': corner-cut both sphere-joint models
            if (Input.GetKeyDown(KeyCode.C))
            {
                cornerCut();
            }

            // 'S': split all the colons in the scene
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (m_bCornerCut[0] && m_bCornerCut[1])
                    split();
                else
                    Debug.Log("Error: Cannot split as either of the colons needs corner-cut!");
            }

            // 'J': join colons that were split
            if (Input.GetKeyDown(KeyCode.J))
            {

                if (m_bSplit)
                    join();
                else
                    Debug.Log("Error: Cannot join as colons have not been split yet!");
            }

            // handle partial-split join: pull end-split vertices for both colon meshes together
            if (m_bJoin && m_colonMeshes[0].endSplitVers2Join.Count <= 0 && m_colonMeshes[1].endSplitVers2Join.Count <= 0)
            {
                if (!m_colonMeshes[0].bFullSplit && !m_colonMeshes[1].bFullSplit)
                {
                    m_colonMeshes[0].endSplitVers2Join.Add("end_outer", m_colonMeshes[1].vertices[m_colonMeshes[1].startEndSplitVers["end_outer"]]);
                    m_colonMeshes[0].endSplitVers2Join.Add("end_inner", m_colonMeshes[1].vertices[m_colonMeshes[1].startEndSplitVers["end_inner"]]);
                    m_colonMeshes[1].endSplitVers2Join.Add("end_outer", m_colonMeshes[0].vertices[m_colonMeshes[0].startEndSplitVers["end_outer"]]);
                    m_colonMeshes[1].endSplitVers2Join.Add("end_inner", m_colonMeshes[0].vertices[m_colonMeshes[0].startEndSplitVers["end_inner"]]);
                }
            }

            // 'V': final close both colons using LS
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (!m_bJoin)
                    Debug.Log("Error: Cannot conduct finalClosure as the colons have not joined yet!");
                else if (m_bFinalClosure)
                    Debug.Log("Error: Cannot conduct finalClosure as that's already conducted!");
                else
                {
                    if (!finalClosure(1))
                        Debug.Log("Final Closure failed!");
                }
            }
        }
    }
}
