using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sphereJointsInfo : MonoBehaviour
{
    /// <summary>
    ///  Joint info for each sphere
    ///     should be updated during split & joit operations
    /// </summary>
    // in-layer joints
    public int m_inLayerJointNum = 0;
    public int[,] m_inLayerJointList = new int[5,4]; // 5: max num of in-layer joints; 3: [jointIdx,objIdx,layerIdx,sphIdx]

    // cross-layer joints
    public int m_crossLayerJointNum = 0;
    public int[,] m_crossLayerJointList = new int[5,4]; // 5: max num of cross-layer joints

    // Start is called before the first frame update
    void Start()
    {
        //m_crossLayerJointList[0, 0] = 100;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
