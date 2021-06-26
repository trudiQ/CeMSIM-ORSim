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

    private bool m_bDataSaved;
    private List<string[]> m_scoreData; // performance data
    private List<string[]> m_simData; // simulation data: state, action, metric related variables
    private List<string[]> m_toolMotionData; // motion data (position, orientation) of the tools: LS, forceps, scissors
    private string m_scoreDataFolderPath;
    private string m_simDataFolderPath;
    private string m_toolMotionDataFolderPath;

    // Sim data
    public List<string> currentFrameSimData; // simulation data in current frame

    // Tool motion data
    public static Transform forcepsTip; // It will track the omni sphere if no forceps are picked up
    public Transform scissors;
    public List<string> currentFrameToolMotionData; // Tool motion data in current frame;
    public static int currentPickedForceps; // -1 means no forceps is picked up, record omni raw data (actually this always record omni (the one controls forceps) data)

    public static string subjID;
    public static string trialID;

    void Start()
    {
        // initialize refs to other modules
        gOperators = FindObjectOfType<globalOperators>();

        // intialize data files to be exported 
        m_bDataSaved = false;
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

        // Final Result
        string[] valueRow = new string[2];
        valueRow[0] = "Total Score";
        valueRow[1] = "" + metricScoring.m_totalScore;
        m_scoreData.Add(valueRow);
        valueRow = new string[2];
        valueRow[0] = "Total Time";
        valueRow[1] = "" + metricScoring.m_totalCompletionTime;
        m_scoreData.Add(valueRow);
        valueRow = new string[2];
        valueRow[0] = "Pass/Fail";
        valueRow[1] = (metricScoring.m_bPass == true) ? "P" : "F";
        m_scoreData.Add(valueRow);

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
        File.AppendAllText(filePath, strBuilder.ToString());
    }

    /// <summary>
    /// Save real-time simulation data all at once when sim ends
    /// </summary>
    /// <param name="subjID"></param>
    /// <param name="trialID"></param>
    public void saveSimData(string subjID, string trialID)
    {
        // first row
        List<string> simDataCategories = new List<string>();
        simDataCategories.Add("Time");
        simDataCategories.Add("CornerCut_left");
        simDataCategories.Add("CornerCut_right");
        simDataCategories.Add("LS_Insertion_left");
        simDataCategories.Add("LS_Insertion_right");
        simDataCategories.Add("LS_SA");
        simDataCategories.Add("HoldOpening");
        simDataCategories.Add("FinalClosure");
        simDataCategories.Add("Forceps_Action");
        simDataCategories.Add("Forceps1_Action");
        simDataCategories.Add("Forceps2_Action");
        simDataCategories.Add("Scissors_Action");
        simDataCategories.Add("LS_Action");
        simDataCategories.Add("InsertDep_left");
        simDataCategories.Add("InsertDep_right");
        simDataCategories.Add("SA_ButtonValue");
        simDataCategories.Add("SA_LayerIdx");
        simDataCategories.Add("FC_LayerIdx");
        simDataCategories.Add("FC_GraspLen");
        m_simData.Insert(0, simDataCategories.ToArray());

        // Data saving
        string[][] output = new string[m_simData.Count][];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = m_simData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";
        StringBuilder strBuilder = new StringBuilder();
        for (int index = 0; index < length; index++)
            strBuilder.AppendLine(string.Join(delimiter, output[index]));

        string filePath = m_simDataFolderPath + "simData_Subj" + subjID.ToString() + "_Trial" + trialID.ToString() + ".csv";
        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.Close();
        File.AppendAllText(filePath, strBuilder.ToString());
    }

    /// <summary>
    /// Record real-time simulation data 
    /// </summary>
    public void RecordSimulationData()
    {
        currentFrameSimData.Clear();
        currentFrameSimData.Add((Time.time - globalOperators.m_startTime).ToString("N3")); //"Time"
        currentFrameSimData.Add(gOperators.m_LRCornerCutIdices[0].ToString()); //"CornerCut_left"
        currentFrameSimData.Add(gOperators.m_LRCornerCutIdices[1].ToString());//"CornerCut_right"
        currentFrameSimData.Add(globalOperators.m_bInsert[0].ToString()); //"LSInsertion_left"
        currentFrameSimData.Add(globalOperators.m_bInsert[1].ToString()); //"LSInsertion_right"
        currentFrameSimData.Add(gOperators.m_bSAStarted.ToString()); //"LS_SA"
        currentFrameSimData.Add(gOperators.m_sphereIdx4EachOpening.ToString()); //"HoldOpening"
        currentFrameSimData.Add(gOperators.m_bFinalClosureStarted.ToString()); //"FinalClosure"
        currentFrameSimData.Add(gOperators.m_hapticSurgTools[gOperators.m_surgToolNames[0]].curAction.ToString()); //"Forceps_Action"
        currentFrameSimData.Add(gOperators.m_hapticSurgTools[gOperators.m_surgToolNames[1]].curAction.ToString());//"Forceps1_Action"
        currentFrameSimData.Add(gOperators.m_hapticSurgTools[gOperators.m_surgToolNames[2]].curAction.ToString());//"Forceps2_Action"
        currentFrameSimData.Add(gOperators.m_hapticSurgTools[gOperators.m_surgToolNames[3]].curAction.ToString());//"Scissors_Action"
        currentFrameSimData.Add(gOperators.m_LSStates.ToString()); //"LS_Action"
        currentFrameSimData.Add(globalOperators.m_insertDepth[0].ToString("N3")); //"InsertDep_left"
        currentFrameSimData.Add(globalOperators.m_insertDepth[1].ToString("N3"));//"InsertDep_right"
        currentFrameSimData.Add(gOperators.m_LSButtonValue.ToString("N3"));//"SA_ButtonValue"
        currentFrameSimData.Add(gOperators.m_layers2Split[1].ToString()); //"SA_LayerIdx"
        currentFrameSimData.Add(gOperators.m_layer2FinalClose.ToString()); //"FC_LayerIdx"
        currentFrameSimData.Add(gOperators.m_LSGraspLengthFinalClosure.ToString()); //"FC_GraspLen"

        // Add data to data set
        m_simData.Add(currentFrameSimData.ToArray());
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
        File.AppendAllText(filePath, strBuilder.ToString());
    }

    /// <summary>
    /// Continuously recording tool motion data during the entire simulation
    /// </summary>
    public void RecordToolMotionData()
    {
        // Add current frame motion data
        currentFrameToolMotionData.Clear();
        currentFrameToolMotionData.Add((Time.time - globalOperators.m_startTime).ToString("N3")); // Time
        currentFrameToolMotionData.Add(currentPickedForceps.ToString()); // whichForceps
        currentFrameToolMotionData.Add(forcepsTip.position.x.ToString("N3")); // x_F
        currentFrameToolMotionData.Add(forcepsTip.position.y.ToString("N3")); // y_F
        currentFrameToolMotionData.Add(forcepsTip.position.z.ToString("N3")); // z_F
        currentFrameToolMotionData.Add(forcepsTip.rotation.x.ToString("N3")); // qx_F
        currentFrameToolMotionData.Add(forcepsTip.rotation.y.ToString("N3")); // qy_F
        currentFrameToolMotionData.Add(forcepsTip.rotation.z.ToString("N3")); // qz_F
        currentFrameToolMotionData.Add(forcepsTip.rotation.w.ToString("N3")); // qw_F
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tracked"></param>
    /// <param name="index"></param>
    public static void SwitchTrackedForceps(Transform tracked, int index)
    {
        forcepsTip = tracked;
        currentPickedForceps = index;
    }

    private void Update()
    {
        if (globalOperators.m_bSimStart && !globalOperators.m_bSimEnd)
        {
            RecordToolMotionData();
            RecordSimulationData();
        }

        // Test data save
        if (globalOperators.m_bSimEnd == true && m_bDataSaved == false) //Input.GetKeyDown(KeyCode.Equals)
        {
            saveScoreData(LSSimDataRecording.subjID, LSSimDataRecording.trialID);
            saveSimData(LSSimDataRecording.subjID, LSSimDataRecording.trialID);
            SaveMotionData(LSSimDataRecording.subjID, LSSimDataRecording.trialID);
            m_bDataSaved = true;
        }
    }
}
