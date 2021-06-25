using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class LSSimDataRecording : MonoBehaviour
{
    public bool m_bEnableDataRecording = true;
    private globalOperators gOperators = null;
    public LinearStaplerTool lsManager;
    public Transform forcepsOmni;
    public Transform scissors;
    private List<string[]> m_scoreData; // performance data
    private List<string[]> m_simData; // simulation data: state, action, metric related variables
    private List<string[]> m_toolMotionData; // motion data (position, orientation) of the tools: LS, forceps, scissors

    private string m_scoreDataFolderPath;
    private string m_simDataFolderPath;
    private string m_toolMotionDataFolderPath;
    public float timeWhenSimulationStarted; // What's the real time since app start when user click on "Start" button on the start menu
    public List<string> currentFrameToolMotionData; // Tool motion data in current frame;
    public int currentPickedForceps; // -1 means no forceps is picked up, record omni raw data (actually this always record omni (the one controls forceps) data)

    public static string subjID;
    public static string trialID;

    void Start()
    {
        // initialize refs to other modules
        gOperators = FindObjectOfType<globalOperators>();

        // intialize data files to be exported 
        m_scoreData = new List<string[]>();
        m_simData = new List<string[]>();
        m_toolMotionData = new List<string[]>();

        // initialize file paths
        string rootPath = Application.dataPath + "/VCOST/";
        m_scoreDataFolderPath = rootPath + "Score_Data/";
        m_simDataFolderPath = rootPath + "Realtime_Data/" + "Sim_Data/";
        m_toolMotionDataFolderPath = rootPath + "Realtime_Data/" + "ToolMotion_Data/";
    }

    /// <summary>
    /// Save metrics data for each step
    /// </summary>
    /// <param name="metricsScores"></param>
    /// <param name="totalMetricsScore"></param>
    /// <param name="totalMetricsTime"></param>
    void saveMetricsData(Dictionary<string, float> metricsScores, (string, float) totalMetricsScore, (string, float) totalMetricsTime)
    {
        // sub-metrics rows
        string[] valueRow = new string[2];

        // Enterotomy
        foreach (KeyValuePair<string, float> kvp in metricsScores)
        {
            valueRow = new string[2];
            valueRow[0] = kvp.Key;
            valueRow[1] = kvp.Value.ToString();
            m_scoreData.Add(valueRow);
        }
        valueRow = new string[2];
        valueRow[0] = totalMetricsScore.Item1;
        valueRow[1] = "" + totalMetricsScore.Item2;
        m_scoreData.Add(valueRow);
        valueRow = new string[2];
        valueRow[0] = totalMetricsTime.Item1;
        valueRow[1] = "" + totalMetricsTime.Item2;
        m_scoreData.Add(valueRow);
    }

    /// <summary>
    /// Called when the entire sim is done, all data is saved at once
    /// </summary>
    /// <param name="subjID"></param>
    /// <param name="trialID"></param>
    public void saveScoreData(string subjID, string trialID)
    {
        if (gOperators == null)
        {
            Debug.Log("Cannot save data, globalOperators is null!");
            return;
        }

        LSMetricsScoring metricScoring = gOperators.MetricsScoringManager;
        if (metricScoring == null)
        {
            Debug.Log("Cannot save data, LSMetricsScoring is null!");
            return;
        }

        // first row
        string[] titleRow = new string[] { "Metrics", "Value" };
        m_scoreData.Add(titleRow);

        // Enterotomy
        (string, float) totalMetricsScore = ("Enterotomy Score", metricScoring.m_EnterotomyScore);
        (string, float) totalMetricsTime = ("Enterotomy Time", metricScoring.m_EnterotomyTime);
        saveMetricsData(metricScoring.m_EnterotomyMetricsScores, totalMetricsScore, totalMetricsTime);

        // LS-Insertion
        totalMetricsScore = ("LS-Insertion Score", metricScoring.m_LSInsertionScore);
        totalMetricsTime = ("LS-Insertion Time", metricScoring.m_LSInsertionTime);
        saveMetricsData(metricScoring.m_LSInsertionMetricsScores, totalMetricsScore, totalMetricsTime);

        // Stapled-Anastamosis
        totalMetricsScore = ("Staple-Anastamosis Score", metricScoring.m_StapledAnastScore);
        totalMetricsTime = ("Stapled-Anastamosis Time", metricScoring.m_StapledAnastTime);
        saveMetricsData(metricScoring.m_StapledAnastMetricsScores, totalMetricsScore, totalMetricsTime);

        // Final-Closure
        totalMetricsScore = ("Final-Closure Score", metricScoring.m_FinalClosureScore);
        totalMetricsTime = ("Final-Closure Time", metricScoring.m_FinalClosureTime);
        saveMetricsData(metricScoring.m_FinalClosureMetricsScores, totalMetricsScore, totalMetricsTime);

        // Data saving
        string[][] output = new string[m_scoreData.Count][];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = m_scoreData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";
        StringBuilder strBuilder = new StringBuilder();
        for (int index = 0; index < length; index++)
            strBuilder.AppendLine(string.Join(delimiter, output[index]));

        string filePath = m_scoreDataFolderPath + "score_Subj" + subjID.ToString() + "_Trial" + trialID.ToString() + ".csv";
        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.Close();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjID"></param>
    /// <param name="trialID"></param>
    public void SaveMotionData(string subjID, string trialID)
    {
        // first row
        List<string> motionDataCategories = new List<string>();
        motionDataCategories.Add("Time");
        motionDataCategories.Add("whichForceps");
        motionDataCategories.Add("x_F");
        motionDataCategories.Add("y_F");
        motionDataCategories.Add("z_F");
        motionDataCategories.Add("qx_F");
        motionDataCategories.Add("qy_F");
        motionDataCategories.Add("qz_F");
        motionDataCategories.Add("qw_F");
        motionDataCategories.Add("x_S");
        motionDataCategories.Add("y_S");
        motionDataCategories.Add("z_S");
        motionDataCategories.Add("qx_S");
        motionDataCategories.Add("qy_S");
        motionDataCategories.Add("qz_S");
        motionDataCategories.Add("qw_S");
        motionDataCategories.Add("x_LSB");
        motionDataCategories.Add("y_LSB");
        motionDataCategories.Add("z_LSB");
        motionDataCategories.Add("qx_LSB");
        motionDataCategories.Add("qy_LSB");
        motionDataCategories.Add("qz_LSB");
        motionDataCategories.Add("qw_LSB");
        motionDataCategories.Add("x_LST");
        motionDataCategories.Add("y_LST");
        motionDataCategories.Add("z_LST");
        motionDataCategories.Add("qx_LST");
        motionDataCategories.Add("qy_LST");
        motionDataCategories.Add("qz_LST");
        motionDataCategories.Add("qw_LST");
        motionDataCategories.Add("x_TSB");
        motionDataCategories.Add("y_TSB");
        motionDataCategories.Add("z_TSB");
        motionDataCategories.Add("qx_TSB");
        motionDataCategories.Add("qy_TSB");
        motionDataCategories.Add("qz_TSB");
        motionDataCategories.Add("qw_TSB");
        motionDataCategories.Add("x_TST");
        motionDataCategories.Add("y_TST");
        motionDataCategories.Add("z_TST");
        motionDataCategories.Add("qx_TST");
        motionDataCategories.Add("qy_TST");
        motionDataCategories.Add("qz_TST");
        motionDataCategories.Add("qw_TST");
        m_toolMotionData.Insert(0, motionDataCategories.ToArray());

        // Data saving
        string[][] output = new string[m_toolMotionData.Count][];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = m_toolMotionData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";
        StringBuilder strBuilder = new StringBuilder();
        for (int index = 0; index < length; index++)
            strBuilder.AppendLine(string.Join(delimiter, output[index]));

        string filePath = m_toolMotionDataFolderPath + "motionData_Subj" + subjID.ToString() + "_Trial" + trialID.ToString() + ".csv";
        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.Close();
    }

    /// <summary>
    /// Continuously recording tool motion data during the entire simulation
    /// </summary>
    public void RecordToolMotionData()
    {
        // Add current frame motion data
        currentFrameToolMotionData.Clear();
        currentFrameToolMotionData.Add((Time.realtimeSinceStartup - timeWhenSimulationStarted).ToString("N3")); // Time
        currentFrameToolMotionData.Add(currentPickedForceps.ToString()); // whichForceps
        currentFrameToolMotionData.Add(forcepsOmni.position.x.ToString("N3")); // x_F
        currentFrameToolMotionData.Add(forcepsOmni.position.y.ToString("N3")); // y_F
        currentFrameToolMotionData.Add(forcepsOmni.position.z.ToString("N3")); // z_F
        currentFrameToolMotionData.Add(forcepsOmni.rotation.x.ToString("N3")); // qx_F
        currentFrameToolMotionData.Add(forcepsOmni.rotation.y.ToString("N3")); // qy_F
        currentFrameToolMotionData.Add(forcepsOmni.rotation.z.ToString("N3")); // qz_F
        currentFrameToolMotionData.Add(forcepsOmni.rotation.w.ToString("N3")); // qw_F
        currentFrameToolMotionData.Add(scissors.position.x.ToString("N3")); // x_S
        currentFrameToolMotionData.Add(scissors.position.y.ToString("N3")); // y_S
        currentFrameToolMotionData.Add(scissors.position.z.ToString("N3")); // z_S
        currentFrameToolMotionData.Add(scissors.rotation.x.ToString("N3")); // qx_S
        currentFrameToolMotionData.Add(scissors.rotation.y.ToString("N3")); // qy_S
        currentFrameToolMotionData.Add(scissors.rotation.z.ToString("N3")); // qz_S
        currentFrameToolMotionData.Add(scissors.rotation.w.ToString("N3")); // qw_S
        currentFrameToolMotionData.Add(lsManager.bottomHalf.transform.position.x.ToString("N3")); // x_LSB
        currentFrameToolMotionData.Add(lsManager.bottomHalf.transform.position.y.ToString("N3")); // y_LSB
        currentFrameToolMotionData.Add(lsManager.bottomHalf.transform.position.z.ToString("N3")); // z_LSB
        currentFrameToolMotionData.Add(lsManager.bottomHalf.transform.rotation.x.ToString("N3")); // qx_LSB
        currentFrameToolMotionData.Add(lsManager.bottomHalf.transform.rotation.y.ToString("N3")); // qy_LSB
        currentFrameToolMotionData.Add(lsManager.bottomHalf.transform.rotation.z.ToString("N3")); // qz_LSB
        currentFrameToolMotionData.Add(lsManager.bottomHalf.transform.rotation.w.ToString("N3")); // qw_LSB
        currentFrameToolMotionData.Add(lsManager.topHalf.transform.position.x.ToString("N3")); // x_LST
        currentFrameToolMotionData.Add(lsManager.topHalf.transform.position.y.ToString("N3")); // y_LST
        currentFrameToolMotionData.Add(lsManager.topHalf.transform.position.z.ToString("N3")); // z_LST
        currentFrameToolMotionData.Add(lsManager.topHalf.transform.rotation.x.ToString("N3")); // qx_LST
        currentFrameToolMotionData.Add(lsManager.topHalf.transform.rotation.y.ToString("N3")); // qy_LST
        currentFrameToolMotionData.Add(lsManager.topHalf.transform.rotation.z.ToString("N3")); // qz_LST
        currentFrameToolMotionData.Add(lsManager.topHalf.transform.rotation.w.ToString("N3")); // qw_LST
        currentFrameToolMotionData.Add(lsManager.topTracker.position.x.ToString("N3")); // x_TSB
        currentFrameToolMotionData.Add(lsManager.topTracker.position.y.ToString("N3")); // y_TSB
        currentFrameToolMotionData.Add(lsManager.topTracker.position.z.ToString("N3")); // z_TSB
        currentFrameToolMotionData.Add(lsManager.topTracker.rotation.x.ToString("N3")); // qx_TSB
        currentFrameToolMotionData.Add(lsManager.topTracker.rotation.y.ToString("N3")); // qy_TSB
        currentFrameToolMotionData.Add(lsManager.topTracker.rotation.z.ToString("N3")); // qz_TSB
        currentFrameToolMotionData.Add(lsManager.topTracker.rotation.w.ToString("N3")); // qw_TSB
        currentFrameToolMotionData.Add(lsManager.bottomTracker.position.x.ToString("N3")); // x_TST
        currentFrameToolMotionData.Add(lsManager.bottomTracker.position.y.ToString("N3")); // y_TST
        currentFrameToolMotionData.Add(lsManager.bottomTracker.position.z.ToString("N3")); // z_TST
        currentFrameToolMotionData.Add(lsManager.bottomTracker.rotation.x.ToString("N3")); // qx_TST
        currentFrameToolMotionData.Add(lsManager.bottomTracker.rotation.y.ToString("N3")); // qy_TST
        currentFrameToolMotionData.Add(lsManager.bottomTracker.rotation.z.ToString("N3")); // qz_TST
        currentFrameToolMotionData.Add(lsManager.bottomTracker.rotation.w.ToString("N3")); // qw_TST

        // Add data to data set
        m_toolMotionData.Add(currentFrameToolMotionData.ToArray());
    }

    void update()
    {

    }
}
