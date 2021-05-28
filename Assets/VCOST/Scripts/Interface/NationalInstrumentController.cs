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
    //DAQmxCreateAIThrmcplChan(TaskHandle taskHandle, const char physicalChannel[], const char nameToAssignToChannel[], float64 minVal, 
    //float64 maxVal, int32 units, int32 thermocoupleType, int32 cjcSource, float64 cjcVal, const char cjcChannel[]);
    //DAQmxBaseCreateAIThrmcplChan(TaskHandle taskHandle, string physicalChannel, string nameToAssignToChannel, double minVal,
    //double maxVal, int units, int thermocoupleType, int cjcSource, double cjcVal, string cjcChannel);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxCreateTask")]
    public static extern unsafe int wrapDAQmxCreateTask(string taskName, TaskHandle* taskHandle);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxStopTask")]
    public static extern int wrapDAQmxStopTask(TaskHandle taskHandle);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxStartTask")]
    public static extern int wrapDAQmxStartTask(TaskHandle taskHandle);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxCreateAIVoltageChan")]
    public static extern int wrapDAQmxCreateAIVoltageChan(TaskHandle taskHandle, string physicalChannel, string nameToAssignToChannel,
        int terminalConfig, double minVal, double maxVal, int units, string customScaleName);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxSetSampTimingType")]

    public static extern int wrapDAQmxSetSampTimingType(TaskHandle taskHandle, int data);

    [DllImport("NIDAQmxWrapper", EntryPoint = "wrapDAQmxReadAnalogF64")]
    public static extern unsafe int wrapDAQmxReadAnalogF64(TaskHandle taskHandle, int numSampsPerChan, double timeout, bool fillMode, double[] readArray,
        UInt32 arraySizeInSamps, int* sampsPerChanRead, bool* reserved);

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

        double[] data = new double[6];
        int read;

        unsafe
        {
            TaskHandle taskHandle = 0;
            print(wrapDAQmxCreateTask("", &taskHandle));

            print(wrapDAQmxCreateAIVoltageChan(taskHandle, "Dev3/ai0:3", "Voltage", 10106, -10.0, 10.0, 10348, null));
            print(wrapDAQmxSetSampTimingType(taskHandle, 10390));
            /*********************************************/
            // DAQmx Start Code
            /*********************************************/
            print(wrapDAQmxStartTask(taskHandle));

            print(wrapDAQmxReadAnalogF64(taskHandle, 1, 10.0, false, data, 6, &read, null));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
