using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;

[ExecuteInEditMode] //allow start() to run in both edit/play mode

public class sphereJointModel : MonoBehaviour
{
    // member variables
    public bool m_bShow = true;
    private bool m_bJointInfo = false;
    public bool m_bFindColonMesh = false;
    public bool m_bCloseUp = false;
    public int m_objIndex;
    public int m_numLayers = 20;
    public int m_numSpheres = 20;// per layer
    public int m_numJoints = 4; // initial #in-layer joints per sphere, will not be udpated
    public int m_numJoints_firstLayer = 2; // #in-lalyer joints in the first layer (for colon-end close-up)
    public int m_numLayerJoints = 2;// initial #cross-layer joints per sphere, will not be updated
    public float m_inLayerSpring = 90.0f;
    public float m_inLayerDamper = 90.0f;
    public float m_inLayerLimit = 0.002f;
    public float m_croLayerSpring = 90.0f;
    public float m_croLayerDamper = 90.0f;
    public float m_croLayerLimit = 0.002f;
    public float m_circleRadius = 1.5f;
    public float m_sphereRadius;
    public float m_layerThickness;
    public float m_startX;
    private Color m_startColor;
    [HideInInspector]
    public Vector3[] m_spherePos = new Vector3[1];
    public GameObject[,] m_sphereGameObjects;

    public GameObject m_sphereJointModel;
    [HideInInspector]
    public Bounds[] m_layerBoundingBox; //[m_numLayers], Bounds: Vector3 min,max
    public float maxSphereVelocity;

    public Vector3 getOriginalSpherePos(int i, int j)
    {
        float theta = 360.0f / m_numSpheres;
        float thetaR = 2.0f * Mathf.PI * theta / 360.0f;
        Vector3 startPos = new Vector3(m_startX, m_sphereRadius, i * 2.2f * m_sphereRadius);// 2.2f, -5.0f
        Vector3 curPos = new Vector3(m_startX, m_sphereRadius, i * 2.2f * m_sphereRadius);// 2.2f, -5.0f
        curPos.x = startPos.x + 2.0f * m_circleRadius * Mathf.Sin(0.5f * j * thetaR) * Mathf.Cos(0.5f * j * thetaR);
        curPos.y = startPos.y + 2.0f * m_circleRadius * Mathf.Sin(0.5f * j * thetaR) * Mathf.Sin(0.5f * j * thetaR);

        return curPos;
    }
    public void initialize(int objIdx, float startX, ref GameObject sphereJointGO) //buildSphereSkeleton()
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
            //startPos += globalOffset;
            Vector3 curPos = new Vector3(m_startX, m_sphereRadius, i * 2.2f * m_sphereRadius);
            //curPos += globalOffset;
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
            int otherObjIdx = 0;
            GameObject tmpObj;
            GameObject[] otherObjects = new GameObject[m_numJoints];
            GameObject[] preLayerObjs = new GameObject[m_numLayerJoints];
            for (int j = 0; j < m_numSpheres; j++)
            {
                tmpObj = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + j.ToString());

                // in-layer joints
                for (int k = 1; k <= m_numJoints; k++)
                {
                    // First layer: only add the limited number of configurable joints 
                    if (i == 0 && k > m_numJoints_firstLayer)
                        continue;

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

    /// <summary>
    /// Add fixed joints to connect spheres of the opposite positions in the same layer 
    ///    to close up the layer
    /// Input:
    ///     layerIdx: index of the layer to close up (defalt: first layer)
    /// 
    public bool closeupLayers(int layerIdx)
    {
        int oppositeObjIdx = 0;
        float distBottom2Top = 0.0f;
        Vector3 vecBottom2Top;
        GameObject tmpObj, oppositeObj;
        sphereJointsInfo objJointsInfo;

        for (int j = 0; j < m_numSpheres; j++)
        {
            tmpObj = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + layerIdx.ToString() + "_" + j.ToString());
            objJointsInfo = tmpObj.GetComponent<sphereJointsInfo>();

            // in-layer fixed joints to connect opposite sphere of the first layer (ADD DURING RUNTIME, AFTER BINDING IS DONE!!)
            if (j >= 6 && j <= 13) // spheres on the top
            {
                oppositeObjIdx = 9 - j; // spheres on the bottom
                if (oppositeObjIdx < 0)
                    oppositeObjIdx += m_numSpheres;
                oppositeObj = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + layerIdx.ToString() + "_" + oppositeObjIdx.ToString());
                if (tmpObj && oppositeObj)
                {
                    // move the two spheres to the middle point
                    vecBottom2Top = tmpObj.transform.position - oppositeObj.transform.position; // bottom -> top
                    vecBottom2Top.Normalize();
                    distBottom2Top = Vector3.Distance(tmpObj.transform.position, oppositeObj.transform.position);
                    tmpObj.transform.position = tmpObj.transform.position - 0.5f * distBottom2Top * vecBottom2Top;
                    oppositeObj.transform.position = oppositeObj.transform.position + 0.5f * distBottom2Top * vecBottom2Top;
                    // add fixed joint
                    FixedJoint joint = tmpObj.AddComponent<FixedJoint>();
                    joint.connectedBody = oppositeObj.GetComponent<Rigidbody>();
                    // fill out joint info.
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 0] = objJointsInfo.m_inLayerJointNum;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 1] = m_objIndex;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 2] = layerIdx;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 3] = oppositeObjIdx;
                    objJointsInfo.m_inLayerJointNum += 1;
                }
                else
                {
                    Debug.Log("Error in 'closeupLayers'");
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Add fixed joints to connect spheres of the opposite positions in the same layer 
    ///    to close up the layer; allow half close-up
    /// Input:
    ///     layerIdx: index of the layer to close up (defalt: first layer)
    /// 
    public bool closeupLayers(int layerIdx, bool bHalfCloseup = false)
    {
        int oppositeObjIdx = 0;
        float distBottom2Top = 0.0f;
        Vector3 vecBottom2Top;
        GameObject tmpObj, oppositeObj;
        sphereJointsInfo objJointsInfo;

        for (int j = 0; j < m_numSpheres; j++)
        {
            tmpObj = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + layerIdx.ToString() + "_" + j.ToString());
            objJointsInfo = tmpObj.GetComponent<sphereJointsInfo>();

            // in-layer fixed joints to connect opposite sphere of the first layer (ADD DURING RUNTIME, AFTER BINDING IS DONE!!)
            if (j >= 6 && j <= 13) // spheres on the top
            {
                if (bHalfCloseup == true && j >= 10)
                    continue;

                oppositeObjIdx = 9 - j; // spheres on the bottom
                if (oppositeObjIdx < 0)
                    oppositeObjIdx += m_numSpheres;
                oppositeObj = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + layerIdx.ToString() + "_" + oppositeObjIdx.ToString());
                if (tmpObj && oppositeObj)
                {
                    // move the two spheres to the middle point
                    vecBottom2Top = tmpObj.transform.position - oppositeObj.transform.position; // bottom -> top
                    vecBottom2Top.Normalize();
                    distBottom2Top = Vector3.Distance(tmpObj.transform.position, oppositeObj.transform.position);
                    tmpObj.transform.position = tmpObj.transform.position - 0.5f * distBottom2Top * vecBottom2Top;
                    oppositeObj.transform.position = oppositeObj.transform.position + 0.5f * distBottom2Top * vecBottom2Top;
                    // add fixed joint
                    FixedJoint joint = tmpObj.AddComponent<FixedJoint>();
                    joint.connectedBody = oppositeObj.GetComponent<Rigidbody>();
                    // fill out joint info.
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 0] = objJointsInfo.m_inLayerJointNum;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 1] = m_objIndex;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 2] = layerIdx;
                    objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 3] = oppositeObjIdx;
                    objJointsInfo.m_inLayerJointNum += 1;
                }
                else
                {
                    Debug.Log("Error in 'closeupLayers'");
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Add fixed joints to connect one specific pair of spheres of the given layer 
    /// Input:
    ///     layerIdx: index of the layer to close up (defalt: first layer)
    ///     topSphereIdx/botSphereIdx: indices of the top and bottom spheres to be connected
    /// 
    public bool closeupSpherePair(int layerIdx, int topSphereIdx, int botSphereIdx)
    {
        // parameters checking
        if (layerIdx < 0 || layerIdx >= m_numLayers)
        {
            Debug.Log("Error(closeupSpherePair): invalid layerIdx");
            return false;
        }
        if (topSphereIdx < 0 || topSphereIdx >= m_numSpheres)
        {
            Debug.Log("Error(closeupSpherePair): invalid topSphereIdx");
            return false;
        }
        if (botSphereIdx < 0 || botSphereIdx >= m_numSpheres)
        {
            Debug.Log("Error(closeupSpherePair): invalid botSphereIdx");
            return false;
        }

        float distBottom2Top = 0.0f;
        Vector3 vecBottom2Top;
        sphereJointsInfo objJointsInfo;
        GameObject topObj = m_sphereGameObjects[layerIdx, topSphereIdx];
        GameObject botObj = m_sphereGameObjects[layerIdx, botSphereIdx];
        if (topObj && botObj)
        {
            // move both spheres to the mid point
            vecBottom2Top = topObj.transform.position - botObj.transform.position; // bottom -> top
            vecBottom2Top.Normalize();
            distBottom2Top = Vector3.Distance(topObj.transform.position, botObj.transform.position);
            topObj.transform.position = topObj.transform.position - 0.3f * distBottom2Top * vecBottom2Top;
            botObj.transform.position = botObj.transform.position + 0.3f * distBottom2Top * vecBottom2Top;
            // add fixed joint
            FixedJoint joint = topObj.AddComponent<FixedJoint>();
            joint.connectedBody = botObj.GetComponent<Rigidbody>();
            // fill out joint info.
            objJointsInfo = topObj.GetComponent<sphereJointsInfo>();
            if (objJointsInfo)
            {
                objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 0] = objJointsInfo.m_inLayerJointNum;
                objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 1] = m_objIndex;
                objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 2] = layerIdx;
                objJointsInfo.m_inLayerJointList[objJointsInfo.m_inLayerJointNum, 3] = botSphereIdx;
                objJointsInfo.m_inLayerJointNum += 1;
            }
            else
            {
                Debug.Log("Error(closeupSpherePair): invalid objJointInfo for sphereIdx " + topSphereIdx.ToString() + "and " + botSphereIdx.ToString());
                return false;
            }
        }
        else
        {
            Debug.Log("Error(closeupSpherePair): invalid GameObjects for sphereIdx " + topSphereIdx.ToString() + "and " + botSphereIdx.ToString());
            return false;
        }

        //Debug.Log("CloseupSpherePair done for sphereIdx " + topSphereIdx.ToString() + "and " + botSphereIdx.ToString());

        return true;
    }

    /// <summary>
    /// Cut the corner of the colon by removing fixed-joints between the spheres of the given positions 
    /// Input: 
    ///     layerIdx: index of the layer to close up (defalt: first layer)
    ///     cutCornerSphereIndices: int[], indices of the top-corner spheres whose fixed-joints to be removed
    ///         - [6, 7, 8,...] (bottom counter-parts are [3, 2, 1,...]) OR
    ///         - [13, 12, 11,..] (bottom counter-parts are [16, 17, 18,...])
    public bool cornerCut(int layerIdx, int[] cutCornerSphereIndices)
    {
        GameObject tmpObj;
        FixedJoint fixedjoint;
        sphereJointsInfo jointsInfo;
        foreach (int sphereIdx in cutCornerSphereIndices)
        {
            tmpObj = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + layerIdx.ToString() + "_" + sphereIdx.ToString());
            fixedjoint = tmpObj.GetComponent<FixedJoint>();
            jointsInfo = tmpObj.GetComponent<sphereJointsInfo>();
            if (fixedjoint && jointsInfo)
            {
                // destroy the joint
                DestroyImmediate(fixedjoint);
                // update joints info.
                jointsInfo.m_inLayerJointList[jointsInfo.m_inLayerJointNum - 1, 0] = -1;
                jointsInfo.m_inLayerJointList[jointsInfo.m_inLayerJointNum - 1, 1] = -1;
                jointsInfo.m_inLayerJointList[jointsInfo.m_inLayerJointNum - 1, 2] = -1;
                jointsInfo.m_inLayerJointList[jointsInfo.m_inLayerJointNum - 1, 3] = -1;
                jointsInfo.m_inLayerJointNum -= 1;
            }
        }

        Debug.Log("cornerCut: Done!!");
        return true;
    }

    /// build joint info for each sphere of sphere-skeleton model
    private void buildJointInfo()
    {
        int numSpheres, numJoints;
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
                if (i > 0)
                    numJoints = m_numJoints;
                else
                    numJoints = m_numJoints_firstLayer;
                for (int k = 1; k <= numJoints; k++)
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
                for (int j = 0; j < oriInLayerNum; j++)
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
    private void highlight(int l, int s)
    {
        GameObject sphere = GameObject.Find("sphere_" + m_objIndex.ToString() + "_" + l.ToString() + "_" + s.ToString());
        if (sphere)
            sphere.GetComponent<Renderer>().material.color = Color.red;
    }

    // highlight a set of spheres
    public void highlight(List<List<int>> neighborSpheres)
    {
        for (int i = 0; i < neighborSpheres.Count; i++)
        {
            highlight(neighborSpheres[i][0], neighborSpheres[i][1]);
        }
    }

    // unhighlight a specific sphere
    private void unhighlight(int l, int s)
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

        // close-up the layers, after its bind with colon mesh
        if (!m_bCloseUp)
        {
            if (m_bFindColonMesh)
            {
                GameObject colonMeshObj = GameObject.Find("outer" + m_objIndex.ToString());
                if (colonMeshObj)
                {
                    colonMesh mesh = colonMeshObj.GetComponent<colonMesh>();
                    if (mesh.isBound)
                    {
                        if (closeupLayers(0))
                            m_bCloseUp = true;

                    }
                }
            }
        }

        if (m_spherePos.Length <= 1)
            m_spherePos = new Vector3[m_numLayers * m_numSpheres];

        // Update data druing playmode
        if (Application.isPlaying)
        {
            Vector3 layerBboxMin, layerBboxMax;
            for (int l = 0; l < m_numLayers; l++)
            {
                layerBboxMin = Vector3.positiveInfinity;
                layerBboxMax = Vector3.negativeInfinity;
                for (int s = 0; s < m_numSpheres; s++)
                {
                    GameObject sphere = m_sphereGameObjects[l, s];
                    if (sphere)
                    {
                        //hide/unhide the rendering of all spheres
                        if (m_bShow)
                            sphere.GetComponent<Renderer>().enabled = true;
                        else
                            sphere.GetComponent<Renderer>().enabled = false;
                        // fill in m_spherePos
                        m_spherePos[l * m_numSpheres + s] = sphere.transform.position;
                        // update min/max layerBbox
                        layerBboxMin = Vector3.Min(layerBboxMin, sphere.transform.position);
                        layerBboxMax = Vector3.Max(layerBboxMax, sphere.transform.position);
                    }
                }
                // update layer bounding box
                if (m_layerBoundingBox.Length > 0)
                {
                    // enlarge the bbox by half
                    Vector3 min2Max = layerBboxMax - layerBboxMin;
                    m_layerBoundingBox[l].min = layerBboxMin - 0.5f * min2Max;
                    m_layerBoundingBox[l].max = layerBboxMax + 0.5f * min2Max;
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

    private void FixedUpdate()
    {
        // Limit colon joint sphere velocity
        for (int l = 0; l < m_numLayers; l++)
        {
            for (int s = 0; s < m_numSpheres; s++)
            {
                m_sphereGameObjects[l, s].GetComponent<Rigidbody>().velocity = m_sphereGameObjects[l, s].GetComponent<Rigidbody>().velocity.normalized *
                    Mathf.Clamp(m_sphereGameObjects[l, s].GetComponent<Rigidbody>().velocity.magnitude, 0, maxSphereVelocity);
            }
        }
    }

    private void Start()
    {
        // Initialize sphere joint GameObject array
        m_sphereGameObjects = new GameObject[m_numLayers, m_numSpheres];

        for (int l = 0; l < m_numLayers; l++)
        {
            for (int s = 0; s < m_numSpheres; s++)
            {
                GameObject sphere = transform.FindDeepChild("sphere_" + m_objIndex.ToString() + "_" + l.ToString() + "_" + s.ToString()).gameObject;
                if (sphere)
                {
                    m_sphereGameObjects[l, s] = sphere;
                }
            }
        }

        // Initialize layer bounding box
        m_layerBoundingBox = new Bounds[m_numLayers];
    }

    [ShowInInspector]
    public void UpdatephereJointModelSimple()
    {
        gameObject.GetComponentsInChildren<ConfigurableJoint>().ToList().ForEach(
            cj => cj.linearLimitSpring = new SoftJointLimitSpring()
            {
                spring = m_inLayerSpring,
                damper = m_inLayerDamper
            });
        return;

        // layers
        for (int i = 0; i < m_numLayers; i++)
        {
            // joints
            int otherObjIdx = 0;
            Transform tmpObj;
            Transform[] otherObjects = new Transform[m_numJoints];
            Transform[] preLayerObjs = new Transform[m_numLayerJoints];
            for (int j = 0; j < m_numSpheres; j++)
            {
                tmpObj = transform.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + j.ToString());

                // in-layer joints
                for (int k = 1; k <= m_numJoints; k++)
                {
                    // First layer: only add the limited number of configurable joints 
                    if (i == 0 && k > m_numJoints_firstLayer)
                        continue;

                    if ((j + k) >= m_numSpheres)
                        otherObjIdx = j + k - m_numSpheres;
                    else
                        otherObjIdx = j + k;
                    otherObjects[k - 1] = transform.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + otherObjIdx.ToString());
                    // joint k
                    if (tmpObj && otherObjects[k - 1])
                    {
                        ConfigurableJoint joint = tmpObj.gameObject.AddComponent<ConfigurableJoint>();
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
                        preLayerObjs[b - 1] = transform.Find("sphere_" + m_objIndex.ToString() + "_" + (i - b).ToString() + "_" + j.ToString());
                    if (tmpObj && preLayerObjs[b - 1])
                    {
                        ConfigurableJoint jointL = tmpObj.gameObject.AddComponent<ConfigurableJoint>();
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

    //[ShowInInspector]
    public void UpdatephereJointModel(ref GameObject sphereJointGO)
    {
        // layers
        for (int i = 0; i < m_numLayers; i++)
        {
            //Transform layer_i = transform.Find("Layer" + m_objIndex.ToString() + "_" + i.ToString());

            //for (int j = 0; j < m_numSpheres; j++)
            //{
            //    Transform sphere_j = transform.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + j.ToString());
            //}
            //layer_i.transform.parent = m_sphereJointModel.transform;

            // joints
            int otherObjIdx = 0;
            Transform tmpObj;
            Transform[] otherObjects = new Transform[m_numJoints];
            Transform[] preLayerObjs = new Transform[m_numLayerJoints];
            for (int j = 0; j < m_numSpheres; j++)
            {
                tmpObj = transform.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + j.ToString());

                // in-layer joints
                for (int k = 1; k <= m_numJoints; k++)
                {
                    // First layer: only add the limited number of configurable joints 
                    if (i == 0 && k > m_numJoints_firstLayer)
                        continue;

                    if ((j + k) >= m_numSpheres)
                        otherObjIdx = j + k - m_numSpheres;
                    else
                        otherObjIdx = j + k;
                    otherObjects[k - 1] = transform.Find("sphere_" + m_objIndex.ToString() + "_" + i.ToString() + "_" + otherObjIdx.ToString());
                    // joint k
                    if (tmpObj && otherObjects[k - 1])
                    {
                        ConfigurableJoint joint = tmpObj.gameObject.AddComponent<ConfigurableJoint>();
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
                        preLayerObjs[b - 1] = transform.Find("sphere_" + m_objIndex.ToString() + "_" + (i - b).ToString() + "_" + j.ToString());
                    if (tmpObj && preLayerObjs[b - 1])
                    {
                        ConfigurableJoint jointL = tmpObj.gameObject.AddComponent<ConfigurableJoint>();
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
}
