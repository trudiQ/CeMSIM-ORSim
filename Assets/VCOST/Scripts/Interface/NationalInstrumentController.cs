using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using Sirenix.OdinInspector;
using NationalInstruments;
using TaskHandle = System.UInt32;

public class NationalInstrumentController : MonoBehaviour
{
    public const int DAQmx_Val_Cfg_Default = -1;
    public const int DAQmx_Val_Volts = 10348;
    public const int DAQmx_Val_Amps = 10342;
    public const int DAQmx_Val_Rising = 10280;
    public const int DAQmx_Val_FiniteSamps = 10178;
    public const bool DAQmx_Val_GroupByChannel = true;

    //DAQmxCreateAIThrmcplChan(TaskHandle taskHandle, const char physicalChannel[], const char nameToAssignToChannel[], float64 minVal, 
    //float64 maxVal, int32 units, int32 thermocoupleType, int32 cjcSource, float64 cjcVal, const char cjcChannel[]);
    //DAQmxBaseCreateAIThrmcplChan(TaskHandle taskHandle, string physicalChannel, string nameToAssignToChannel, double minVal,
    //double maxVal, int units, int thermocoupleType, int cjcSource, double cjcVal, string cjcChannel);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxCreateTask")]
    public static extern unsafe int wrapDAQmxCreateTask(char[] taskName, TaskHandle* taskHandle);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxStopTask")]
    public static extern int wrapDAQmxStopTask(TaskHandle taskHandle);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxStartTask")]
    public static extern int wrapDAQmxStartTask(TaskHandle taskHandle);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxCreateAIVoltageChan")]
    public static extern int wrapDAQmxCreateAIVoltageChan(TaskHandle taskHandle, char[] physicalChannel, char[] nameToAssignToChannel,
        int terminalConfig, double minVal, double maxVal, int units, char[] customScaleName);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxSetSampTimingType")]

    public static extern int wrapDAQmxSetSampTimingType(TaskHandle taskHandle, int data);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxReadAnalogF64")]
    public static extern unsafe int wrapDAQmxReadAnalogF64(TaskHandle taskHandle, int numSampsPerChan, double timeout, bool fillMode, double[] readArray,
        UInt32 arraySizeInSamps, int* sampsPerChanRead, bool* reserved);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxCfgSampClkTiming")]
    public static extern unsafe int wrapDAQmxCfgSampClkTiming(TaskHandle taskHandle, char[] source, float rate, int activeEdge, int sampleMode, long sampsPerChan);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxGetExtendedErrorInfo")]
    public static extern unsafe int wrapDAQmxGetExtendedErrorInfo(char[] errorchar, int bufferSize);

    public char[] errBuff;

    // Start is called before the first frame update
    void Start()
    {
        int stopSign = 0;
        int frequency = 200; //Hz
        double period = 1000 / (double)frequency; //in milliseconds
        double currentTime = 0.0;
        double startTime = 0.0;
        double lastTime = 0.0;
        double push_button_v = 0.0;
        double button_high = 4.5;
        double button_low = 2.0;
        bool button_on;
        bool output = true;
        int BEGIN = 0;

        double[] data = new double[1000];
        int read;

        errBuff = new char[2048];

        unsafe
        {
            TaskHandle taskHandle = 0;
            DAQmxErrChk(wrapDAQmxCreateTask("".ToCharArray(), &taskHandle));

            DAQmxErrChk(wrapDAQmxCreateAIVoltageChan(taskHandle, "Dev1/ai0".ToCharArray(), "".ToCharArray(), DAQmx_Val_Cfg_Default, -10.0, 10.0, DAQmx_Val_Volts, null));
            DAQmxErrChk(wrapDAQmxCfgSampClkTiming(taskHandle, "".ToCharArray(), 10000.0f, DAQmx_Val_Rising, DAQmx_Val_FiniteSamps, 1000));
            /*********************************************/
            // DAQmx Start Code
            /*********************************************/
            DAQmxErrChk(wrapDAQmxStartTask(taskHandle));

            DAQmxErrChk(wrapDAQmxReadAnalogF64(taskHandle, 1000, 10.0, DAQmx_Val_GroupByChannel, data, 1000, &read, null));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool DAQmxErrChk(int errorCode)
    {
        if (errorCode == 0)
        {
            return false;
        }
        else
        {
            PrintDAQmxErr(errorCode);
            StopDAQmxTask();
            return true;
        }
    }

    public void PrintDAQmxErr(int errorCode)
    {
        wrapDAQmxGetExtendedErrorInfo(errBuff, 2048);
        print(errBuff);
    }

    public void StopDAQmxTask()
    {

    }
}
