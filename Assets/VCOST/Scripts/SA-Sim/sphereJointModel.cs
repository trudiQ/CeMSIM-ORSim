using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode] //allow start() to run in both edit/play mode

public class sphereJointModel : MonoBehaviour
{
    // member variables
    public bool m_bShow = true;
    private bool m_bJointInfo = false;
    public bool m_bFindColonMesh = false;
    public int m_objIndex;
    public int m_numLayers = 20;
    public int m_numSpheres = 20;// per layer
    public int m_numJoints = 4; // initial #in-layer joints per sphere, will not be udpated
    public int m_numLayerJoints = 2;// initial #cross-layer joints per sphere, will not be updated
    public float m_inLayerSpring = 90.0f;
    public float m_inLayerDamper = 90.0f;
    public float m_inLayerLimit = 0.002f;
    public float m_croLayerSpring = 90.0f;
    public float m_croLayerDamper = 90.0f;
    public float m_croLayerLimit = 0.002f;
    public float m_circleRadius = 0.025f;
    public float m_sphereRadius;
    public float m_layerThickness;
    public float m_startX;
    private Color m_startColor;
    [HideInInspector]
    public Vector3[] m_spherePos = new Vector3[1];

    public GameObject m_sphereJointModel;

    public Vector3 getOriginalSpherePos (int i, int j)
    {
        float theta = 360.0f / m_numSpheres;
        float thetaR = 2.0f * Mathf.PI * theta / 360.0f;
        Vector3 startPos = new Vector3(m_startX, m_sphereRadius, i * 2.2f * m_sphereRadius);// 2.2f, -5.0f
        Vector3 curPos = new Vector3(m_startX, m_sphereRadius, i * 2.2f * m_sphereRadius);// 2.2f, -5.0f
        curPos.x = startPos.x + 2.0f * m_circleRadius * Mathf.Sin(0.5f * j * thetaR) * Mathf.Cos(0.5f * j * thetaR);
        curPos.y = startPos.y + 2.0f * m_circleRadius * Mathf.Sin(0.5f * j * thetaR) * Mathf.Sin(0.5f * j * thetaR);

        return curPos;
    }
    public void initialize (int objIdx, float startX, ref GameObject sphereJointGO) //buildSphereSkeleton()
    {
        m_objIndex = objIdx;
        m_startX = startX;
        // create model
        buildSphereJointModel(ref sphereJointGO);
        // restore joint info for each sphere
        //buildJointInfo();
        m_bJointInfo = false;
    }

    private void buildSphereJointModel(ref GameObject sphereJointGO)
    {
        /// create layer: spheres, joints
        m_sphereJointModel = sphereJointGO;
        //m_sphereJointModel = new GameObject("SphereJoint" + m_objIndex.ToString());
        m_sphereJointModel.transform.position = new Vector3(0, 0, 0);
        m_sphereJointModel.transform.rotation = Quaternion.identity;
        m_sphereJointModel.transform.localScale = new Vector3(1, 1, 1);

        // global offset sphereJointModel
        Vector3 globalOffset = new Vector3(0.3266f, 0.805f, -1.0563f);//-1.054 
        // calculate sphere radius
        float theta = 360.0f / m_numSpheres;
        float thetaR = 2.0f * Mathf.PI * theta / 360.0f;
        m_sphereRadius = m_circleRadius * Mathf.Sin(thetaR / 2.0f);
        // layer thickness
        m_layerThickness = 2.2f * m_sphereRadius;

        // layers
        for (int i = 0; i < m_numLayers; i++)
        {
            GameObject layer_i = new GameObject("Layer" + m_objIndex.ToString() + "_" + i.ToString());
            layer_i.transform.position = new Vector3(0, 0, 0);
            layer_i.transform.rotation = Quaternion.identity;
            layer_i.transform.localScale = new Vector3(1, 1, 1);
            // spheres
            Vector3 startPos = new Vector3(m_startX, m_sphereRadius, i * 2.2f * m_sphereRadius);
            startPos += globalOffset;
            Vector3 curPos = new Vector3(m_startX, m_sphereRadius, i * 2.2f * m_sphereRadius);
            curPos += globalOffset;
            for (int j = 0; j < m_numSpheres; j++)
            {
                GameObject sphere_j = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere_j.name = "sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + j.ToString();
                if (i == 0 && j == 0)
                    m_startColor = sphere_j.GetComponent<Renderer>().sharedMaterial.color;
                Rigidbody rigid = sphere_j.AddComponent<Rigidbody>();
                rigid.mass = 0.01f;//0.05
                rigid.angularDrag = 0.05f;
                rigid.useGravity = true;
                // pos
                curPos.x = startPos.x + 2.0f * m_circleRadius * Mathf.Sin(0.5f * j * thetaR) * Mathf.Cos(0.5f * j * thetaR);
                curPos.y = startPos.y + 2.0f * m_circleRadius * Mathf.Sin(0.5f * j * thetaR) * Mathf.Sin(0.5f * j * thetaR);
                sphere_j.transform.position = curPos;//new Vector3(0, 0.25f, -0.4f);
                sphere_j.transform.rotation = Quaternion.identity;
                sphere_j.transform.localScale = new Vector3(2.0f * m_sphereRadius, 2.0f * m_sphereRadius, 2.0f * m_sphereRadius);

                sphere_j.transform.parent = layer_i.transform;

                // joint info
                sphere_j.AddComponent<sphereJointsInfo>();
            }
            layer_i.transform.parent = m_sphereJointModel.transform;

            // joints
            GameObject tmpObj;
            GameObject[] otherObjects = new GameObject[m_numJoints];
            GameObject[] preLayerObjs = new GameObject[m_numLayerJoints];
            for (int j = 0; j < m_numSpheres; j++)
            {
                tmpObj = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + j.ToString());

                // in-layer joints
                for (int k = 1; k <= m_numJoints; k++)
                {
                    int otherObjIdx = 0;
                    if ((j + k) >= m_numSpheres)
                        otherObjIdx = j + k - m_numSpheres;
                    else
                        otherObjIdx = j + k;
                    otherObjects[k - 1] = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + otherObjIdx.ToString());
                    // joint k
                    if (tmpObj && otherObjects[k - 1])
                    {
                        ConfigurableJoint joint = tmpObj.AddComponent<ConfigurableJoint>();
                        joint.connectedBody = otherObjects[k - 1].GetComponent<Rigidbody>();
                        // configure
                        joint.xMotion = ConfigurableJointMotion.Limited;
                        joint.yMotion = ConfigurableJointMotion.Limited;
                        joint.zMotion = ConfigurableJointMotion.Limited;
                        joint.angularXMotion = ConfigurableJointMotion.Locked;
                        joint.angularYMotion = ConfigurableJointMotion.Locked;
                        joint.angularZMotion = ConfigurableJointMotion.Locked;
                        joint.linearLimitSpring = new SoftJointLimitSpring()
                        {
                            spring = m_inLayerSpring,
                            damper = m_inLayerDamper
                        };
                        joint.linearLimit = new SoftJointLimit() { limit = m_inLayerLimit, contactDistance = 0.1f };
                    }
                }

                //// cross-layer joints
                for (int b = 1; b <= m_numLayerJoints; b++)
                {
                    if (i >= b)
                        preLayerObjs[b - 1] = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + (i - b).ToString() + "_" + j.ToString());
                    if (tmpObj && preLayerObjs[b - 1])
                    {
                        ConfigurableJoint jointL = tmpObj.AddComponent<ConfigurableJoint>();
                        jointL.connectedBody = preLayerObjs[b - 1].GetComponent<Rigidbody>();
                        // configure
                        jointL.xMotion = ConfigurableJointMotion.Limited;
                        jointL.yMotion = ConfigurableJointMotion.Limited;
                        jointL.zMotion = ConfigurableJointMotion.Limited;
                        jointL.angularXMotion = ConfigurableJointMotion.Locked;
                        jointL.angularYMotion = ConfigurableJointMotion.Locked;
                        jointL.angularZMotion = ConfigurableJointMotion.Locked;
                        jointL.linearLimitSpring = new SoftJointLimitSpring()
                        {
                            spring = m_croLayerSpring,
                            damper = m_croLayerDamper
                        };
                        jointL.linearLimit = new SoftJointLimit() { limit = m_croLayerLimit, contactDistance = 0.1f };
                    }
                }
            }
        }
    }

    /// build joint info for each sphere of sphere-skeleton model
    private void buildJointInfo()
    {
        int numSpheres;
        GameObject layer, tmpObj;
        GameObject[] preLayerObjs = new GameObject[m_numLayerJoints];
        sphereJointsInfo objJointsInfo;

        for (int i = 0; i < m_numLayers; i++)
        {
            layer = GameObject.Find("Layer" + m_objIndex.ToString() + "_" + i.ToString());
            numSpheres = layer.transform.childCount;
            for (int j = 0; j < numSpheres; j++)
            {
                tmpObj = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + j.ToString());
                objJointsInfo = tmpObj.GetComponent<sphereJointsInfo>();
                objJointsInfo.m_inLayerJointNum = 0;
                objJointsInfo.m_crossLayerJointNum = 0;

                // in-layer joints
                for (int k = 1; k <= m_numJoints; k++)
                {
                    int otherObjIdx = 0;
                    if ((j + k) >= numSpheres)
                        otherObjIdx = j + k - numSpheres;
                    else
                        otherObjIdx = j + k;

                    // fill out joint info for each sphere
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 0] = objJointsInfo.m_inLayerJointNum;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 1] = m_objIndex;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 2] = i;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 3] = otherObjIdx;
                    objJointsInfo.m_inLayerJointNum += 1;
                }

                // cross-layer joints
                for (int b = 1; b <= m_numLayerJoints; b++)
                {
                    // fill out join info for each sphere
                    if (i >= b)
                    {
                        objJointsInfo.m_crossLayerJointList[objJointsInfo.m_crossLayerJointNum, 0] = objJointsInfo.m_inLayerJointNum + objJointsInfo.m_crossLayerJointNum;
                        objJointsInfo.m_crossLayerJointList[objJointsInfo.m_crossLayerJointNum, 1] = m_objIndex;
                        objJointsInfo.m_crossLayerJointList[objJointsInfo.m_crossLayerJointNum, 2] = i - b;
                        objJointsInfo.m_crossLayerJointList[objJointsInfo.m_crossLayerJointNum, 3] = j;
                        objJointsInfo.m_crossLayerJointNum += 1;
                    }
                }
            }
        }
    }

    /// Split operation: split in-layer joints of a single model
        // split the sphere model between sphereIdxA and sphereIdxB (i.e., sphereIdxA+1) 
        //  along layers of layers2split[]
    public void splitInlayerJoints_sigle(ref int[] layers2split, int sphereIdxA)
    {
        // make sure the input split layers are valid
        if (layers2split[0] >= m_numLayers || layers2split[0] < 0 ||
            layers2split[1] >= m_numLayers || layers2split[1] < 0)
        {
            Debug.Log("Error in 'splitInLayerJoints_single: invalid layer numbers!");
            return;
        }
        // obtain #layers to split
        int numSplitLayers = layers2split[1] - layers2split[0] + 1;
        if (numSplitLayers == 0 || numSplitLayers > m_numLayers)
        {
            Debug.Log("Error in 'splitInLayerJoints_single: invalid layer range!");
            return;
        }
        else if (numSplitLayers < 0) // swap these two values
        {
            int temp = layers2split[0];
            layers2split[0] = layers2split[1];
            layers2split[1] = temp;
            numSplitLayers = Math.Abs(numSplitLayers);
        }

        // obtain the sphere on the other side of spliting line
        int sphereIdxB = sphereIdxA + 1;
        if (sphereIdxB >= m_numSpheres)
            sphereIdxB = sphereIdxB - m_numSpheres;

        // traverse each layer to be split
        bool bDestroy = false;
        int sphereIdx, sphereIdxj, oriInLayerNum;
        GameObject layer, sphere;
        ConfigurableJoint[] joints;
        sphereJointsInfo jointsInfo;
        for (int l = 0; l < numSplitLayers; l++)
        {
            layer = GameObject.Find("Layer" + m_objIndex.ToString() + "_" + (layers2split[0] + l).ToString());
            // traverse spheres in backward (decreasing index)
            for (int s = 0; s < m_numJoints; s++)
            {
                sphereIdx = sphereIdxA - s;
                if (sphereIdx < 0)
                    sphereIdx = sphereIdx + m_numSpheres;
                sphere = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + (layers2split[0] + l).ToString() + "_" + sphereIdx.ToString());
                joints = sphere.GetComponents<ConfigurableJoint>();
                jointsInfo = sphere.GetComponent<sphereJointsInfo>();
                oriInLayerNum = jointsInfo.m_inLayerJointNum;
                // traverse all its in-layer joints and disconnect all the spheres whose index >= sphereIdxB
                for (int j = 0; j < oriInLayerNum; j ++)
                {
                    bDestroy = false;
                    sphereIdxj = jointsInfo.m_inLayerJointList[j, 3];
                    if (sphereIdxB == (m_numSpheres - 1))
                    {
                        if (sphereIdx < sphereIdxj)
                        {
                            if (sphereIdxj >= sphereIdxB)
                                bDestroy = true;
                        }
                        else if (sphereIdx > sphereIdxj)
                        {
                            sphereIdxj += m_numSpheres;
                            if (sphereIdxj >= sphereIdxB)
                                bDestroy = true;
                        }
                    }
                    else
                    {
                        if (sphereIdxj >= sphereIdxB)
                            bDestroy = true;
                    }
                    if (bDestroy)
                    {
                        // destroy the joint
                        DestroyImmediate(joints[jointsInfo.m_inLayerJointList[j, 0]]);
                        // update joints info 
                        jointsInfo.m_inLayerJointList[j, 0] = -1;
                        jointsInfo.m_inLayerJointList[j, 1] = -1;
                        jointsInfo.m_inLayerJointList[j, 2] = -1;
                        jointsInfo.m_inLayerJointList[j, 3] = -1;
                        jointsInfo.m_inLayerJointNum -= 1;
                    }
                    /*
                    if (jointsInfo.m_inLayerJointList[j, 3] >= sphereIdxB)
                    {
                        // destroy the joint
                        DestroyImmediate(joints[jointsInfo.m_inLayerJointList[j, 0]]);
                        // update joints info 
                        jointsInfo.m_inLayerJointList[j, 0] = -1;
                        jointsInfo.m_inLayerJointList[j, 1] = -1;
                        jointsInfo.m_inLayerJointList[j, 2] = -1;
                        jointsInfo.m_inLayerJointList[j, 3] = -1;
                        jointsInfo.m_inLayerJointNum -= 1;
                    } */
                }
            }
        }
        Debug.Log("Split in-layer joints done!");
    }

    // Associate to colon mesh
    public bool findColonMesh()
    {
        GameObject colonMeshObj = GameObject.Find("outer" + m_objIndex.ToString());
        if (colonMeshObj)
        {
            colonMesh mesh = colonMeshObj.GetComponent<colonMesh>();
            if (mesh)
            {
                mesh.sphereJointObjName = "sphereJointGameObj" + m_objIndex.ToString();
                mesh.sphereJointObjIdx = m_objIndex;
                return true;
            }
            else
            {
                Debug.Log("sphereJointModel: cannot find colonMesh");
            }
        }
        else
        {
            Debug.Log("sphereJointModel: cannot find colonMesh game object!");
        }
        return false;
    }

    // Reset transformations of the model
    public void Reset()
    {
        if (m_sphereJointModel == null)
        {
            Debug.Log("Cannot Reset: no m_sphereJointModel!");
            return;
        }
        // reset model tranformations
        m_sphereJointModel.transform.position = new Vector3(0, 0, 0);
        m_sphereJointModel.transform.rotation = Quaternion.identity;
        m_sphereJointModel.transform.localScale = new Vector3(1, 1, 1);

        // reset layers
        for (int i = 0; i < m_numLayers; i++)
        {
            GameObject layer_i = GameObject.Find("Layer" + m_objIndex.ToString() + "_" + i.ToString());
            layer_i.transform.position = new Vector3(0, 0, 0);
            layer_i.transform.rotation = Quaternion.identity;
            layer_i.transform.localScale = new Vector3(1, 1, 1);
            // reset spheres
            for (int j = 0; j < m_numSpheres; j++)
            {
                GameObject sphere_j = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + j.ToString());
                sphere_j.transform.position = getOriginalSpherePos(i, j);
                sphere_j.transform.rotation = Quaternion.identity;
                sphere_j.transform.localScale = new Vector3(2.0f * m_sphereRadius, 2.0f * m_sphereRadius, 2.0f * m_sphereRadius);

                sphere_j.transform.parent = layer_i.transform;
            }
            layer_i.transform.parent = m_sphereJointModel.transform;
        }
    }

    // highlight a specific sphere
    private void highlight (int l, int s)
    {
        GameObject sphere = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + l.ToString() + "_" + s.ToString());
        if (sphere)
           sphere.GetComponent<Renderer>().material.color = Color.red;
    }

    // highlight a set of spheres
    public void highlight (List<List<int>> neighborSpheres)
    {
        for (int i = 0; i < neighborSpheres.Count; i++)
        {
            highlight(neighborSpheres[i][0], neighborSpheres[i][1]);
        }
    }

    // unhighlight a specific sphere
    private void unhighlight (int l, int s)
    {
        GameObject sphere = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + l.ToString() + "_" + s.ToString());
        if (sphere)
            sphere.GetComponent<Renderer>().material.color = m_startColor;
    }

    // unhighlight a set of spheres
    public void unhighlight(List<List<int>> neighborSpheres)
    {
        for (int i = 0; i < neighborSpheres.Count; i++)
        {
            unhighlight(neighborSpheres[i][0], neighborSpheres[i][1]);
        }
    }

    private void Update()
    {
        if (!m_bJointInfo)
        {
            buildJointInfo();
            m_bJointInfo = true;
            Debug.Log("buildJointInfo!");
        }

        if (!m_bFindColonMesh)
        {
            if (findColonMesh())
            {
                m_bFindColonMesh = true;
                Debug.Log("sphereJointModel " + m_objIndex.ToString() + " find colon mesh!");
            }
        }

        if (m_spherePos.Length <= 1)
            m_spherePos = new Vector3[m_numLayers * m_numSpheres];

        // Druing playmode, hide the rendering of all spheres
        if (Application.isPlaying)
        {
            for (int l = 0; l < m_numLayers; l++)
            {
                for (int s = 0; s < m_numSpheres; s++)
                {
                    GameObject sphere = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + l.ToString() + "_" + s.ToString());
                    if (sphere)
                    {
                        if (m_bShow)
                            sphere.GetComponent<Renderer>().enabled = true;
                        else
                            sphere.GetComponent<Renderer>().enabled = false;
                        // fill in m_spherePos
                        m_spherePos[l * m_numSpheres + s] = sphere.transform.position;
                    }
                }
            }
        }

        //// About to exit from playmode
        //if (Application.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        //{
        //    for (int l = 0; l < m_numLayers; l++)
        //    {
        //        for (int s = 0; s < m_numSpheres; s++)
        //        {
        //            GameObject sphere = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + l.ToString() + "_" + s.ToString());
        //            if (sphere)
        //                sphere.GetComponent<Renderer>().enabled = true;
        //        }
        //    }
        //}
    }
}
