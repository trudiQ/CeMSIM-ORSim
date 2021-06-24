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
	private List<string[]> m_scoreData; // performance data
	private List<string[]> m_simData; // simulation data: state, action, metric related variables
	private List<string[]> m_toolMotionData; // motion data (position, orientation) of the tools: LS, forceps, scissors

	private string m_scoreDataFolderPath;
	private string m_simDataFolderPath;
	private string m_toolMotionDataFolderPath;

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

	void update()
    {

    }
}
