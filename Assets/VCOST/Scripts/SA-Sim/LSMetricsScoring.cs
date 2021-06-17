using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSMetricsScoring : MonoBehaviour
{
    bool m_bPass = false;
    float m_totalCompletionTime = 0.0f;
    float m_totalScore = 0.0f;
    /// Enterotomy
    // metrics scores
    float m_EnterotomyTime = 0.0f;
    float m_EnterotomyScore = 0.0f;
    string[] m_EnterotomyMetrics = { "OpenEnterotomyPoint", 
                                    "SecureEnterotomyPoint" };
    Dictionary<string, float> m_EnterotomyMetricsScores = new Dictionary<string, float>();
    // Variables used to determine scores (updated by external code)
    float m_openAntiMesentCorner = 1.0f; // 1.0: all cut-corners are anti-mesentery; 0.0: at least one cut-corner is mesentery
    int[] m_cornerCutSecured = { 0, 0 }; // for both colons, 0: not secured, 1: secured during cutting

    /// LS-Insertion
    // metrics scores
    float m_LSInsertionTime = 0.0f;
    float m_LSInsertionScore = 0.0f;
    string[] m_LSInsertionMetrics = { "CloseLS",
                                      "SecureInsertionOpening" };
    Dictionary<string, float> m_LSInsertionMetricsScores = new Dictionary<string, float>();
    // Variables used to determine scores (updated by external code)
    bool m_bLSInsertCloseEvaluated = false; // true: the 'CloseLS' metric already evaluated
    bool m_bLSInsertionClosed = false; // true: LS sticks close at the momemnt stapling
    int[] m_LSInsertionSecured = { 0, 0 }; // for both colons, 0: not secured, 1: secured during LS insertion

    /// Staple-Anastomosis 
    // metrics scores
    float m_StapledAnastTime = 0.0f;
    float m_StapledAnastScore = 0.0f;
    string[] m_StapledAnastMetrics = { "FullStapling",
                                       "LSOpenRemove" };
    Dictionary<string, float> m_StapledAnastMetricsScores = new Dictionary<string, float>();
    // Variables used to determine scores
    bool m_bSAFullStaplingEvaluated = false;
    bool m_bLSOpenRemoveEvaluated = false;

    // Start is called before the first frame update
    void Start()
    {
        // initialize metrics scores
        for (int i = 0; i < m_EnterotomyMetrics.Length; i++)
        {
            m_EnterotomyMetricsScores.Add(m_EnterotomyMetrics[i], 0.0f);
        }
        for (int i = 0; i < m_LSInsertionMetrics.Length; i++)
        {
            m_LSInsertionMetricsScores.Add(m_LSInsertionMetrics[i], 0.0f);
        }
    }

    /// <summary>
    /// Update scores for the enterotomy when a new cut was made by checking
    ///     1) if the corners with mesentery layer are opened 
    ///     2) if the corners are secured during cutting
    /// </summary>
    /// <param name="objIdx"></param> which sphereJoint model, 0 or 1
    /// <param name="LorR"></param> which corner of the model was cut, 0: left/ 1: right
    /// <param name="bOpeningSecure"></param> if the corner was secured by forceps during cutting
    public void updateEnterotomyScores(int objIdx, int LorR, bool bOpeningSecure)
    {
        // check only if mesentery corner has not been opened yet 
        if (m_openAntiMesentCorner == 1.0f)
        {
            if (objIdx == 0 && LorR == 0 || objIdx == 1 && LorR == 1)
                m_openAntiMesentCorner = 0.0f;
        }

        // check if the required corner was secured by forceps when it's being cut
        if (bOpeningSecure && (objIdx == 0 && LorR == 1 || objIdx == 1 && LorR == 0))
            m_cornerCutSecured[objIdx] = 1;

        // Update scores
        foreach (KeyValuePair<string, float> ele in m_EnterotomyMetricsScores)
        {
            if (ele.Key == m_EnterotomyMetrics[0]) // "OpenEnterotomyPoint"
            {
                m_EnterotomyMetricsScores[ele.Key] = m_openAntiMesentCorner;
            }
            else if (ele.Key == m_EnterotomyMetrics[1]) // "SecureEnterotomyPoint"
            {
                if ((m_cornerCutSecured[0] + m_cornerCutSecured[1]) == 2)
                    m_EnterotomyMetricsScores[ele.Key] = 5.0f;
                else if ((m_cornerCutSecured[0] + m_cornerCutSecured[1]) == 1)
                    m_EnterotomyMetricsScores[ele.Key] = 2.0f;
                else
                    m_EnterotomyMetricsScores[ele.Key] = 0.0f;
            }
        }
        m_EnterotomyScore = m_EnterotomyMetricsScores[m_EnterotomyMetrics[0]] * m_EnterotomyMetricsScores[m_EnterotomyMetrics[1]];

        // print scores
        Debug.Log("Enterotomy metrics scores: ");
        foreach (KeyValuePair<string, float> kvp in m_EnterotomyMetricsScores)
            Debug.Log("- " + kvp.Key + ": " + kvp.Value.ToString());
    }

    /// <summary>
    /// Updating scores for LS-insertion by checking 
    ///     1) if the LS is closed when trying to staple {evaluated with 'StapledAnastomosis'}
    ///     2) if the opening is secured during the insertion
    /// </summary>
    /// <param name="objIdx"></param> which sphereJointModel
    /// <param name="bOpeningSecure"></param> whether or not use forceps to assist with insertion
    public void updateLSInsertionScores(int objIdx, bool bOpeningSecure)
    {
        // "CloseLS"
        m_LSInsertionMetricsScores[m_LSInsertionMetrics[0]] = (m_bLSInsertionClosed == true) ? 5.0f : 0.0f;

        // "SecureInsertionOpening"      
        if (bOpeningSecure)// check if the opening is secured during insertion
        {
            m_LSInsertionSecured[objIdx] = 1;
        }
        if (m_LSInsertionSecured[0] + m_LSInsertionSecured[1] == 2)
            m_LSInsertionMetricsScores[m_LSInsertionMetrics[1]] = 5.0f;
        else if (m_LSInsertionSecured[0] + m_LSInsertionSecured[1] == 1)
            m_LSInsertionMetricsScores[m_LSInsertionMetrics[1]] = 2.0f;
        else
            m_LSInsertionMetricsScores[m_LSInsertionMetrics[1]] = 0.0f;

        // total LS-Insertion score
        m_LSInsertionScore = m_LSInsertionMetricsScores[m_LSInsertionMetrics[0]] + m_LSInsertionMetricsScores[m_LSInsertionMetrics[1]];

        // print scores
        Debug.Log("LS-Insertion metrics scores: ");
        foreach (KeyValuePair<string, float> kvp in m_LSInsertionMetricsScores)
            Debug.Log("- " + kvp.Key + ": " + kvp.Value.ToString());
    }

    /// <summary>
    /// Update scores for stapled anastomosis by checking
    ///     1) if the button is full-down during stapling
    ///     2) if LS is unlocked when trying to remove the LS
    /// </summary>
    /// <param name="bStapling"></param> true: pushing LS button now
    /// <param name="bLSButtonFullDown"></param> true: full down/ false: partial down or no pushing
    /// <param name="bJoin"></param> true: join operation is already done
    /// <param name="m_bLSRemoving"></param> true: LS is removing from the colons
    /// <param name="bLSLocked"></param> true: LS is locked 
    public void updateStapledAnastScores(bool bStapling, bool bLSButtonFullDown, bool bJoin, bool m_bLSRemoving, bool bLSLocked)
    {
        // Evaluate "closeLS" for LS-Insertion
        if (bStapling == true && m_bLSInsertCloseEvaluated == false)
        {
            m_bLSInsertionClosed = (bLSLocked == true) ? true : false;
            m_bLSInsertCloseEvaluated = true;
        }
        // "Stapling": button full down?
        if (bJoin == true && m_bSAFullStaplingEvaluated == false)
        {
            m_StapledAnastMetricsScores[m_StapledAnastMetrics[0]] = (bLSButtonFullDown == true) ? 5.0f : 0.0f;
            m_bSAFullStaplingEvaluated = true;
        }

        // "LSOpenRemove": LS unlocked when removing?
        if (m_bLSRemoving == false && m_bLSOpenRemoveEvaluated == false)
        {
            m_StapledAnastMetricsScores[m_StapledAnastMetrics[1]] = (bLSLocked) ? 5.0f : 0.0f;
            m_bLSOpenRemoveEvaluated = true;
        }

        // total Stapled Anastomosis score
        m_StapledAnastScore = m_StapledAnastMetricsScores[m_StapledAnastMetrics[0]] + m_StapledAnastMetricsScores[m_StapledAnastMetrics[1]];

        // print scores
        Debug.Log("Stapled Anastomosis metrics scores: ");
        foreach (KeyValuePair<string, float> kvp in m_StapledAnastMetricsScores)
            Debug.Log("- " + kvp.Key + ": " + kvp.Value.ToString());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
