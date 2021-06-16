using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSMetricsScoring : MonoBehaviour
{
    bool m_bPass = false;
    float m_totalCompletionTime = 0.0f;
    float m_totalScore = 0.0f;
    /// Enterotomy
    float m_EnterotomyTime = 0.0f;
    float m_EnterotomyScore = 0.0f;
    Dictionary<string, float> m_EnterotomyMetricsScores =
                    new Dictionary<string, float>();
    string[] m_EnterotomyMetrics = { "SecureEnterotomyPoint",
                                        "OpenEnterotomyPoint" };
    // Variables used to determine scores (updated by external code)
    bool m_bSecureCornerCutPoint = false;
    bool m_bOpenAntiMesentCorner = false;

    // Start is called before the first frame update
    void Start()
    {
        /// Enterotomy
        // initialize metrics scores
        for (int i = 0; i < m_EnterotomyMetrics.Length; i++)
        {
            m_EnterotomyMetricsScores.Add(m_EnterotomyMetrics[i], 0.0f);
        }
    }

    void updateEnterotomyScores()
    {
        
        
        
        // Update total scores
        m_EnterotomyScore = 0.0f;
        foreach (KeyValuePair<string, float> ele in m_EnterotomyMetricsScores)
        {
            m_EnterotomyScore += ele.Value;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
