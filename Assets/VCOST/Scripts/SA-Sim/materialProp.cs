using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class materialProp : MonoBehaviour
{
    // Material properties
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
