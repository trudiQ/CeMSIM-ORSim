using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class LSMetricsScoring : MonoBehaviour
{
    public bool m_bPass = false;
    public bool m_bFinalResultEvaluated = false;
    public float m_totalCompletionTime = 0.0f;
    public float m_totalScore = 0.0f;
    /// Enterotomy
    // metrics scores
    public bool m_bEnterotomyTimeEvaluated = false;
    public float m_EnterotomyStartTime = 0.0f; 
    public float m_EnterotomyTime = 0.0f; // Enterotomy completion time
    public float m_EnterotomyScore = 0.0f; // Enterotomy total score
    public string[] m_EnterotomyMetrics = { "OpenEnterotomyPoint", 
                                    "SecureEnterotomyPoint" };
    public Dictionary<string, float> m_EnterotomyMetricsScores = new Dictionary<string, float>();
    // Variables used to determine scores (updated by external code)
    public float m_openAntiMesentCorner = 1.0f; // 1.0: all cut-corners are anti-mesentery; 0.0: at least one cut-corner is mesentery
    public int[] m_cornerCutSecured = { 0, 0 }; // for both colons, 0: not secured, 1: secured during cutting

    /// LS-Insertion
    // metrics scores
    public bool m_bLSInsertionTimeEvaluated = false;
    public float m_LSInsertionStartTime = 0.0f;
    public float m_LSInsertionTime = 0.0f;
    public float m_LSInsertionScore = 0.0f;
    public string[] m_LSInsertionMetrics = { "CloseLS",
                                      "SecureInsertionOpening" };
    public Dictionary<string, float> m_LSInsertionMetricsScores = new Dictionary<string, float>();
    // Variables used to determine scores (updated by external code)
    public bool m_bLSInsertCloseEvaluated = false; // true: the 'CloseLS' metric already evaluated
    public bool m_bLSInsertionClosed = false; // true: LS sticks close at the momemnt stapling
    public int[] m_LSInsertionSecured = { 0, 0 }; // for both colons, 0: not secured, 1: secured during LS insertion

    /// Staple-Anastomosis 
    // metrics scores
    public bool m_bStapledAnastTimeEvaluated = false;
    public float m_StapledAnastStartTime = 0.0f;
    public float m_StapledAnastTime = 0.0f;
    public float m_StapledAnastScore = 0.0f;
    public string[] m_StapledAnastMetrics = { "FullStapling",
                                       "LSOpenRemove" };
    public Dictionary<string, float> m_StapledAnastMetricsScores = new Dictionary<string, float>();
    // Variables used to determine scores (udpated by external code)
    public bool m_bSAFullStaplingEvaluated = false;
    public bool m_bSAFullyStapled = false; // if LS fully stapled in SA (button full-down)
    public bool m_bLSOpenRemoveEvaluated = false;
    public bool m_bLSOpenBeforeRemoving = false; // if LS opened before removing from colons

    /// Final-Closure
    public bool m_FinalClosurePass = false;
    // metrics scores
    public bool m_FinalClosureTimeEvaluated = false;
    public float m_FinalClosureStartTime = 0.0f;
    public float m_FinalClosureTime = 0.0f;
    public float m_FinalClosureScore = 0.0f;
    public string[] m_FinalClosureMetrics = { "OpeningSecured",
                                       "OpeningFullyGrasped",
                                       "CutZoneCrossed",
                                       "CloseLS",
                                       "FullyStapling",
                                       "MesenteryClear" };
    public Dictionary<string, float> m_FinalClosureMetricsScores = new Dictionary<string, float>();
    // Variables used to determine scores (updated by external code)
    public int m_numOpeningSecuredForceps = 0; // #forceps holds the opening during the final-closure
    public bool m_LSFullyGraspOpening = false; // if LS grasps both colon ends fully
    public bool m_LSCutZoneCrossed = true; // if LS places in the cut-zone to do the final-closure
    public bool m_LSFinalClosureClosed = false; // if LS fully closed before stapling
    public bool m_LSFinalCloseEvaluated = false;
    public bool m_FCFullyStapled = false; // if LS fully stapled in final-closure (button full-down)
    public bool m_FCFullStapleEvaluated = false;
    public bool m_MesenteryCleared = false; // if mesentery layers are clear after final-closure
    public int m_cutZoneLayerIdx = 1; // sphereJointModel's layerIdx where final-closure is applied
    public int m_mesenteryLayerIdx = 4; // sphereJointModel's layerIdx where mesentery begins to attach

    // Start is called before the first frame update
    void Start()
    {
        // initialize metrics scores
        m_FinalClosurePass = false;
        for (int i = 0; i < m_EnterotomyMetrics.Length; i++)
        {
            m_EnterotomyMetricsScores.Add(m_EnterotomyMetrics[i], 0.0f);
        }
        for (int i = 0; i < m_LSInsertionMetrics.Length; i++)
        {
            m_LSInsertionMetricsScores.Add(m_LSInsertionMetrics[i], 0.0f);
        }
        for (int i = 0; i < m_StapledAnastMetrics.Length; i++)
        {
            m_StapledAnastMetricsScores.Add(m_StapledAnastMetrics[i], 0.0f);
        }
        for (int i = 0; i < m_FinalClosureMetrics.Length; i++)
        {
            m_FinalClosureMetricsScores.Add(m_FinalClosureMetrics[i], 0.0f);
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
        foreach (string key in m_EnterotomyMetrics)
        {
            if (key == m_EnterotomyMetrics[0]) // "OpenEnterotomyPoint"
            {
                m_EnterotomyMetricsScores[key] = m_openAntiMesentCorner;
            }
            else if (key == m_EnterotomyMetrics[1]) // "SecureEnterotomyPoint"
            {
                if ((m_cornerCutSecured[0] + m_cornerCutSecured[1]) == 2)
                    m_EnterotomyMetricsScores[key] = 5.0f;
                else if ((m_cornerCutSecured[0] + m_cornerCutSecured[1]) == 1)
                    m_EnterotomyMetricsScores[key] = 2.0f;
                else
                    m_EnterotomyMetricsScores[key] = 0.0f;
            }
        }
        float score = m_EnterotomyMetricsScores[m_EnterotomyMetrics[0]] * m_EnterotomyMetricsScores[m_EnterotomyMetrics[1]];

        // print scores
        if (score != m_EnterotomyScore)
        {
            m_EnterotomyScore = score;
            Debug.Log("Enterotomy metrics scores: ");
            foreach (KeyValuePair<string, float> kvp in m_EnterotomyMetricsScores)
                Debug.Log("- " + kvp.Key + ": " + kvp.Value.ToString());
        }
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
        float score = m_LSInsertionMetricsScores[m_LSInsertionMetrics[0]] + m_LSInsertionMetricsScores[m_LSInsertionMetrics[1]];

        // print scores
        if (score != m_LSInsertionScore)
        {
            m_LSInsertionScore = score;
            Debug.Log("LS-Insertion metrics scores: ");
            foreach (KeyValuePair<string, float> kvp in m_LSInsertionMetricsScores)
                Debug.Log("- " + kvp.Key + ": " + kvp.Value.ToString());
        }
    }

    /// <summary>
    /// Update scores for stapled anastomosis by checking
    ///     1) if the button is full-down during stapling
    ///     2) if LS is unlocked when trying to remove the LS
    /// </summary>
    /// <param name="bLSButtonPushing"></param> true: pushing LS button now
    /// <param name="bLSButtonFullDown"></param> true: full down/ false: partial down or no pushing
    /// <param name="bJoin"></param> true: join operation is already done
    /// <param name="m_bLSRemoving"></param> true: LS is removing from the colons
    /// <param name="bLSLocked"></param> true: LS is locked 
    public void updateStapledAnastScores(bool bLSButtonPushing, bool bLSButtonFullDown, bool bJoin, bool m_bLSRemoving, bool bLSLocked)
    {
        // Evaluate "closeLS" for LS-Insertion when just about to push the button
        if (bLSButtonPushing == true && m_bLSInsertCloseEvaluated == false)
        {
            m_bLSInsertionClosed = (bLSLocked == true) ? true : false;
            m_bLSInsertCloseEvaluated = true;
        }
        // "FullyStaple": button full down?
        if (bJoin == true && m_bSAFullStaplingEvaluated == false)
        {
            m_bSAFullyStapled = bLSButtonFullDown;
            m_StapledAnastMetricsScores[m_StapledAnastMetrics[0]] = (m_bSAFullyStapled == true) ? 5.0f : 0.0f;
            m_bSAFullStaplingEvaluated = true;
        }

        // "LSOpenRemove": LS unlocked when removing?
        if (bJoin == true && m_bLSRemoving == true && m_bLSOpenRemoveEvaluated == false)
        {
            m_bLSOpenBeforeRemoving = (bLSLocked == false) ? true : false;
            m_StapledAnastMetricsScores[m_StapledAnastMetrics[1]] = (m_bLSOpenBeforeRemoving == true) ? 5.0f : 0.0f;
            m_bLSOpenRemoveEvaluated = true;
        }

        // total Stapled Anastomosis score
        float score = m_StapledAnastMetricsScores[m_StapledAnastMetrics[0]] + m_StapledAnastMetricsScores[m_StapledAnastMetrics[1]];

        // print scores
        if (score != m_StapledAnastScore)
        {
            m_StapledAnastScore = score;
            Debug.Log("Stapled Anastomosis metrics scores: ");
            foreach (KeyValuePair<string, float> kvp in m_StapledAnastMetricsScores)
                Debug.Log("- " + kvp.Key + ": " + kvp.Value.ToString());
        }
    }

    /// <summary>
    /// Update scores for final-closure by checking
    ///     1) if the opening is fully secured based on #forceps grasping
    ///     2) if the colon ends are fully grasped by the LS
    ///     3) if LS crosses the cut-zone based on layerIdx for final closure
    ///     4) if LS closed before button-pushing
    ///     5) if LS button full-down
    ///     6) if mesentery layer is clear based on the layerIdx for final closure
    /// </summary>
    /// <param name="bFinalClosure"></param> # if final-closure is done
    /// <param name="numGraspingForceps"></param> #forceps used to grasp the colons openings
    /// <param name="bLSFullGrasping"></param> if LS fully grasps both colons
    /// <param name="fullClosureLayerIdx"></param> sphereJointModels' layerIdx for final-closure
    /// <param name="bLSButton2Push"></param> if the button is about to push
    /// <param name="bLSLocked"></param> if LS is locked
    /// <param name="bLSButtonFullDown"></param> if LS button is full-down
    public void updateFinalClosureScores(bool bFinalClosure, int numGraspingForceps, bool bLSFullGrasping, int fullClosureLayerIdx, 
                                         bool bLSButton2Push, bool bLSLocked, bool bLSButtonFullDown)
    {
        //"OpeningSecured"
        if (bLSLocked == true)
        {
            m_numOpeningSecuredForceps = numGraspingForceps;
            switch(m_numOpeningSecuredForceps)
            {
                case 3:
                    m_FinalClosureMetricsScores[m_FinalClosureMetrics[0]] = 5.0f;
                    break;
                case 2:
                    m_FinalClosureMetricsScores[m_FinalClosureMetrics[0]] = 3.0f;
                    break;
                case 1:
                    m_FinalClosureMetricsScores[m_FinalClosureMetrics[0]] = 2.0f;
                    break;
                case 0:
                    m_FinalClosureMetricsScores[m_FinalClosureMetrics[0]] = 0.0f;
                    break;
            }
        }

        //"OpeningFullyGrasped"
        if (bLSLocked == true)
        {
            m_LSFullyGraspOpening = bLSFullGrasping;
            if (m_LSFullyGraspOpening)
            {
                m_FinalClosureMetricsScores[m_FinalClosureMetrics[1]] = 5.0f;
                m_FinalClosurePass = true;
            }
            else
            {
                m_FinalClosureMetricsScores[m_FinalClosureMetrics[1]] = 0.0f;
            }
        }

        //"CutZoneCrossed"
        if (bLSLocked == true)
        {
            m_LSCutZoneCrossed = (fullClosureLayerIdx <= m_cutZoneLayerIdx) ? true : false;
            if (m_LSCutZoneCrossed == true)
                m_FinalClosureMetricsScores[m_FinalClosureMetrics[2]] = 0.0f;
            else
                m_FinalClosureMetricsScores[m_FinalClosureMetrics[2]] = 5.0f;
        }

        //"CloseLS"
        if (bLSButton2Push == true && m_LSFinalCloseEvaluated == false)
        {
            m_LSFinalClosureClosed = (bLSLocked == true) ? true : false;
            m_LSFinalCloseEvaluated = true;
            if (m_LSFinalClosureClosed == true)
                m_FinalClosureMetricsScores[m_FinalClosureMetrics[3]] = 5.0f;
            else
                m_FinalClosureMetricsScores[m_FinalClosureMetrics[3]] = 0.0f;
        }

        //"FullyStapling"
        if (bFinalClosure && m_FCFullStapleEvaluated == false) // evaluated when final-closure is just done
        {
            m_FCFullyStapled = bLSButtonFullDown;
            m_FinalClosureMetricsScores[m_FinalClosureMetrics[4]] = (m_FCFullyStapled == true) ? 5.0f : 0.0f;
            m_FCFullStapleEvaluated = true;
        }

        //"MesenteryClear"
        if (bLSLocked == true) //evaluate when LS is locked and starts button pushing
        {
            m_MesenteryCleared = (fullClosureLayerIdx < m_mesenteryLayerIdx) ? true : false;
            if (m_MesenteryCleared == true)
            {
                m_FinalClosureMetricsScores[m_FinalClosureMetrics[5]] = 5.0f;
                m_FinalClosurePass = true;
            }
            else
            {
                m_FinalClosureMetricsScores[m_FinalClosureMetrics[5]] = 0.0f;
            }
        }

        // update total final-closure score
        float score = 0.0f;
        foreach (KeyValuePair<string, float> kvp in m_FinalClosureMetricsScores)
        {
            score += kvp.Value;
        }

        // print scores
        if (score != m_FinalClosureScore)
        {
            m_FinalClosureScore = score;
            Debug.Log("Final-Closure metrics scores: ");
            foreach (KeyValuePair<string, float> kvp in m_FinalClosureMetricsScores)
                Debug.Log("- " + kvp.Key + ": " + kvp.Value.ToString());
        }
    }

    public void evaluateLSCompletion()
    {
        // Pass?
        m_bPass = m_FinalClosurePass;

        // total score
        m_totalScore = m_EnterotomyScore + m_LSInsertionScore + m_StapledAnastScore + m_FinalClosureScore;

        // total completion time
        m_totalCompletionTime = m_EnterotomyTime + m_LSInsertionTime + m_StapledAnastTime + m_FinalClosureTime;

        m_bFinalResultEvaluated = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
