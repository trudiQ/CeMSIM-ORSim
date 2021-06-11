using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using Sirenix.OdinInspector;

public class TestAPI : MonoBehaviour
{
    [DllImport("ATC3DG64", EntryPoint = "InitializeBIRDSystem")]
    public static extern int InitializeBIRDSystem();

    [DllImport("ATC3DG64", EntryPoint = "SetSystemParameter")]
    public static extern unsafe int SetSystemParameter(SYSTEM_PARAMETER_TYPE parameterType, void* pBuffer, int bufferSize);

    [DllImport("ATC3DG64", EntryPoint = "GetAsynchronousRecord")]
    public static extern unsafe int GetAsynchronousRecord(ushort sensorID, void* pRecord, int recordSize);

    public Transform trackerMarker0;
    public Transform trackerMarker1;
    public Transform tracker0OriginPosition;
    public Transform tracker1OriginPosition;

    public Vector3 trackerMarkerStartPosition0;
    public Vector3 trackerMarkerStartEuler0;
    public Vector3 trackerMarkerStartPosition1;
    public Vector3 trackerMarkerStartEuler1;
    public Vector3 trackerMarkerCurrentRawRotationData1;
    public Vector3 rotaterEuler;
    public Vector3 movementScaleCalibrationStartPos; // Starting position in unity when user start to calibrate the movement scale to native Unity scale
    public float movementScaleCalibrationUnityDisplacement; // Moving distance in Unity when user move tracker to real life finish position
    public Vector3 rawRotationDataForScaleCalibration; // Record the raw rotation data at the beginning and end of the movement then take the average of the two values

    public List<Tracker> trackers;
    public List<Transform> copyees;
    public List<Transform> rotaters;
    public List<TrackerController> trackerData;

    // Start is called before the first frame update
    void Start()
    {
        // Tracker initialization
        BIRD_ERROR_CODES errorInit = GetErrorMessage((int)InitializeBIRDSystem());
        //print(errorInit);
        SetupSensor();

        //Declare the Trackers and their assisting GameObjects
        trackers = new List<Tracker>();
        trackerData = new List<TrackerController>();
        for (int i = 0; i < 4; i++)
        {
            trackerData.Add(new TrackerController());
            trackers.Add(new Tracker());
            GameObject copyee = new GameObject();
            copyees.Add(copyee.transform);
            GameObject rotater = new GameObject();
            rotaters.Add(rotater.transform);
        }

        // Model initialization
        // ### This should be removed once the start position/rotation calibration is implemented
        if (trackerMarker0 != null)
        {
            trackerMarkerStartPosition0 = trackerMarker0.position;
            trackerMarkerStartEuler0 = trackerMarker0.eulerAngles;
        }
        if (trackerMarker1 != null)
        {
            trackerMarkerStartPosition1 = trackerMarker1.position;
            trackerMarkerStartEuler1 = trackerMarker1.eulerAngles;
        }

        // Initialize tracker data
        trackerData.ForEach(t => t.rawPositions = new Vector3[t.filterSteps]);
        trackerData.ForEach(t => t.filteredSum = Vector3.zero);
    }

    private void Update()
    {
        // Keep getting tracking data
        TestTracking();

        //trackers.ForEach(t => GetTrackerData(t));
    }

    /// <summary>
    /// Test tracker setup and initial readings
    /// </summary>
    void TestSensor()
    {
        unsafe
        {
            ushort deviceID = 0;
            BIRD_ERROR_CODES errorSet = GetErrorMessage((int)SetSystemParameter(SYSTEM_PARAMETER_TYPE.SELECT_TRANSMITTER, &deviceID, sizeof(ushort)));
            print(errorSet);
            DOUBLE_POSITION_ANGLES_RECORD record;
            BIRD_ERROR_CODES errorGet = GetErrorMessage((int)GetAsynchronousRecord(deviceID, &record, Marshal.SizeOf(record)));
            print(errorGet);
        }
    }

    /// <summary>
    /// Setup all existing sensor
    /// </summary>
    void SetupSensor()
    {
        unsafe
        {
            // ### Should have flag for unconnected tracker based on the return error code
            ushort deviceID0 = 0;
            BIRD_ERROR_CODES errorSet0 = GetErrorMessage((int)SetSystemParameter(SYSTEM_PARAMETER_TYPE.SELECT_TRANSMITTER, &deviceID0, sizeof(ushort)));
            print(errorSet0);
            ushort deviceID1 = 1;
            BIRD_ERROR_CODES errorSet1 = GetErrorMessage((int)SetSystemParameter(SYSTEM_PARAMETER_TYPE.SELECT_TRANSMITTER, &deviceID1, sizeof(ushort)));
            print(errorSet1);
            ushort deviceID2 = 2;
            BIRD_ERROR_CODES errorSet2 = GetErrorMessage((int)SetSystemParameter(SYSTEM_PARAMETER_TYPE.SELECT_TRANSMITTER, &deviceID2, sizeof(ushort)));
            print(errorSet2);
            ushort deviceID3 = 3;
            BIRD_ERROR_CODES errorSet3 = GetErrorMessage((int)SetSystemParameter(SYSTEM_PARAMETER_TYPE.SELECT_TRANSMITTER, &deviceID3, sizeof(ushort)));
            print(errorSet3);
        }
    }

    /// <summary>
    /// Calibrate the movement scale between the real moving distance and the default Unity scale
    /// </summary>
    [ShowInInspector]
    public void CalibrateMovementScale()
    {
        if (movementScaleCalibrationStartPos == Vector3.zero)
        {
            movementScaleCalibrationStartPos = trackerMarker1.position;
            rawRotationDataForScaleCalibration = trackerMarkerCurrentRawRotationData1;
        }
        else
        {
            movementScaleCalibrationUnityDisplacement = Vector3.Distance(trackerMarker1.position, movementScaleCalibrationStartPos);
            Vector3 initialEuler = rawRotationDataForScaleCalibration;
            Vector3 endEuler = trackerMarkerCurrentRawRotationData1;
            // Correct the values if the angle value went from +180 to -180
            if (initialEuler.x * endEuler.x < 0 && Mathf.Abs(initialEuler.x) > 90)
            {
                endEuler.x += Mathf.Sign(initialEuler.x) * 360;
            }
            if (initialEuler.y * endEuler.y < 0 && Mathf.Abs(initialEuler.y) > 90)
            {
                endEuler.y += Mathf.Sign(initialEuler.y) * 360;
            }
            if (initialEuler.z * endEuler.z < 0 && Mathf.Abs(initialEuler.z) > 90)
            {
                endEuler.z += Mathf.Sign(initialEuler.z) * 360;
            }
            rawRotationDataForScaleCalibration = (rawRotationDataForScaleCalibration + trackerMarkerCurrentRawRotationData1) * 0.5f;
            movementScaleCalibrationStartPos = Vector3.zero;
        }
    }

    /// <summary>
    /// Update GameObject transform based on tracker's reading
    /// </summary>
    void TestTracking()
    {
        DOUBLE_POSITION_ANGLES_RECORD record0;
        ushort deviceID0 = 0;
        DOUBLE_POSITION_ANGLES_RECORD record1;
        ushort deviceID1 = 1;

        unsafe
        {
            // Get the tracker readings
            BIRD_ERROR_CODES errorGet0 = GetErrorMessage((int)GetAsynchronousRecord(deviceID0, &record0, Marshal.SizeOf(record0)));
            BIRD_ERROR_CODES errorGet1 = GetErrorMessage((int)GetAsynchronousRecord(deviceID1, &record1, Marshal.SizeOf(record1)));
        }

        // Update transform based on readings, the position xyz and euler xyz are already matched with the tracker reading
        if (trackerMarker0 != null)
        {
            trackerMarker0.position = tracker0OriginPosition.position + new Vector3((float)record0.x, -(float)record0.z, -(float)record0.y);
            trackerMarker0.rotation = Quaternion.Euler(new Vector3(-(float)record0.r, (float)record0.a, 0));
            RotateZ(trackerMarker0, trackerMarker0.parent, (float)record0.a, (float)record0.e, rotaters[0]);
        }
        if (trackerMarker1 != null)
        {
            trackerMarkerCurrentRawRotationData1 = new Vector3((float)record1.r, (float)record1.e, (float)record1.a);
            trackerMarker1.position = tracker1OriginPosition.position + new Vector3((float)record1.x, -(float)record1.z, -(float)record1.y);
            trackerMarker1.rotation = Quaternion.Euler(new Vector3(-(float)record1.r, (float)record1.a, 0));
            RotateZ(trackerMarker1, trackerMarker1.parent, (float)record1.a, (float)record1.e, rotaters[1]);
        }

        // Update tracker data
        trackerData[0].rawPosition = trackerMarker0.position;
        trackerData[1].rawPosition = trackerMarker1.position;

        // Apply filtered position data
        trackerData[0].UpdateFilter();
        trackerMarker0.position = trackerData[0].filteredPosition;
        trackerData[1].UpdateFilter();
        trackerMarker1.position = trackerData[1].filteredPosition;

        ////Update the position and orientation of Trackers
        //trackers[0].positions = tracker0OriginPosition.position + new Vector3((float)record0.x, -(float)record0.z, -(float)record0.y);
        //trackers[0].angles = new Vector3(-(float)record0.r, (float)record0.a, (float)record0.e);
        //trackers[1].positions = tracker1OriginPosition.position + new Vector3((float)record1.x, -(float)record1.z, -(float)record1.y);
        //trackers[1].angles = new Vector3(-(float)record1.r, (float)record1.a, (float)record1.e);
    }

    /// <summary>
    /// Get tracker data from API readings and store in the target tracker object
    /// ### This currently assumes that the tracker index in the API will match the connection port number on the tracking station
    /// </summary>
    /// <param name="targetTracker"></param>
    public void GetTrackerData(Tracker targetTracker)
    {
        DOUBLE_POSITION_ANGLES_RECORD record;
        ushort deviceID = (ushort)trackers.IndexOf(targetTracker);

        unsafe
        {
            // Get the tracker readings
            BIRD_ERROR_CODES errorGet = GetErrorMessage((int)GetAsynchronousRecord(deviceID, &record, Marshal.SizeOf(record)));
        }

        // Create empty GameObject to help with calculations
        //GameObject trackerObject = new GameObject();

        // Update transform based on readings, the position xyz and euler xyz are already matched with the tracker reading
        copyees[deviceID].position = new Vector3((float)record.x, -(float)record.z, -(float)record.y);
        copyees[deviceID].rotation = Quaternion.Euler(new Vector3(-(float)record.r, (float)record.a, 0));
        RotateZ(copyees[deviceID], copyees[deviceID].parent, (float)record.a, (float)record.e, rotaters[deviceID]);

        //Update the position and orientation of Trackers
        targetTracker.positions = copyees[deviceID].position;
        targetTracker.angles = copyees[deviceID].eulerAngles;
        targetTracker.rotation = copyees[deviceID].rotation;

        //Destroy(trackerObject);
    }

    /// <summary>
    /// Rotate an object around z-axis based on its current y angle
    /// </summary>
    /// <param name="toRotate"></param>
    /// <param name="originalParent"></param>
    /// <param name="yAngle"></param>
    /// <param name="zAngle"></param>
    /// <param name="rotater"></param>
    /// <returns></returns>
    public Vector3 RotateZ(Transform toRotate, Transform originalParent, float yAngle, float zAngle, Transform rotater)
    {
        //GameObject rotater = new GameObject();
        rotater.transform.rotation = Quaternion.Euler(new Vector3(0, yAngle, 0));
        toRotate.parent = rotater.transform;
        rotater.transform.eulerAngles += new Vector3(0, 0, zAngle);
        rotaterEuler = rotater.transform.eulerAngles;
        Vector3 result = toRotate.eulerAngles;
        toRotate.parent = originalParent;
        //Destroy(rotater);
        return result;
    }

    /// <summary>
    /// Parse the BIRD error code (no logging for now)
    /// </summary>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    public BIRD_ERROR_CODES GetErrorMessage(int errorCode)
    {
        int parsedErrorCode = errorCode;
        if (parsedErrorCode != 0)
        {
            parsedErrorCode -= int.MinValue;
        }

        return (BIRD_ERROR_CODES)(parsedErrorCode);
    }
}

////////////////////////////////////////////////////////////////////
////
//// Get a record from sensor #0.
//// The default record type is DOUBLE_POSITION_ANGLES
////
//USHORT sensorID = 0;
//DOUBLE_POSITION_ANGLES_RECORD record;
//errorCode = GetAsynchronousRecord(sensorID, &record, sizeof(record));
//if(errorCode!=BIRD_ERROR_SUCCESS)
//{
//errorHandler(errorCode);
//}

#region SYSTEM_PARAMETER_TYPE
public enum SYSTEM_PARAMETER_TYPE
{
    SELECT_TRANSMITTER,     // short int equal to transmitterID of selected transmitter
    POWER_LINE_FREQUENCY,   // double value (range is hardware dependent)
    AGC_MODE,               // enumerated constant of type AGC_MODE_TYPE
    MEASUREMENT_RATE,       // double value (range is hardware dependent)
    MAXIMUM_RANGE,          // double value (range is hardware dependent)
    METRIC,                 // boolean value to select metric units for position
    VITAL_PRODUCT_DATA,     // single byte parameter to be read/write from VPD section of board EEPROM
    POST_ERROR,             // system (board 0) POST_ERROR_PARAMETER
    DIAGNOSTIC_TEST,        // system (board 0) DIAGNOSTIC_TEST_PARAMETER
    REPORT_RATE,            // single byte 1-127			
    COMMUNICATIONS_MEDIA,   // Media structure
    LOGGING,                // Boolean
    RESET,                  // Boolean
    AUTOCONFIG,             // BYTE 1-127
    AUXILIARY_PORT,         // structure of type AUXILIARY_PORT_PARAMETERS
    COMMUTATION_MODE,       // boolean value to select commutation of sensor data for interconnect pickup rejection
    END_OF_LIST             // end of list place holder
};
#endregion

#region BIRD_ERROR_CODES
public enum BIRD_ERROR_CODES
{
    //	ERROR CODE DISPOSITION
    //    |		(Some error codes have been retired.
    //    |      The column below describes which codes 
    //	  |      have been retired and why. O = Obolete,
    //    V      I = handled internally)
    BIRD_ERROR_SUCCESS = 0,                 //00 < > No error	
    BIRD_ERROR_PCB_HARDWARE_FAILURE,        //01 < > indeterminate failure on PCB
    BIRD_ERROR_TRANSMITTER_EEPROM_FAILURE,  //02 <I> transmitter bad eeprom
    BIRD_ERROR_SENSOR_SATURATION_START,     //03 <I> sensor has gone into saturation
    BIRD_ERROR_ATTACHED_DEVICE_FAILURE,     //04 <O> either a sensor or transmitter reports bad
    BIRD_ERROR_CONFIGURATION_DATA_FAILURE,  //05 <O> device EEPROM detected but corrupt
    BIRD_ERROR_ILLEGAL_COMMAND_PARAMETER,   //06 < > illegal PARAMETER_TYPE passed to driver
    BIRD_ERROR_PARAMETER_OUT_OF_RANGE,      //07 < > PARAMETER_TYPE legal, but PARAMETER out of range
    BIRD_ERROR_NO_RESPONSE,                 //08 <O> no response at all from target card firmware
    BIRD_ERROR_COMMAND_TIME_OUT,            //09 < > time out before response from target board
    BIRD_ERROR_INCORRECT_PARAMETER_SIZE,    //10 < > size of parameter passed is incorrect
    BIRD_ERROR_INVALID_VENDOR_ID,           //11 <O> driver started with invalid PCI vendor ID
    BIRD_ERROR_OPENING_DRIVER,              //12 < > couldn't start driver
    BIRD_ERROR_INCORRECT_DRIVER_VERSION,    //13 < > wrong driver version found
    BIRD_ERROR_NO_DEVICES_FOUND,            //14 < > no BIRDs were found anywhere
    BIRD_ERROR_ACCESSING_PCI_CONFIG,        //15 < > couldn't access BIRDs config space
    BIRD_ERROR_INVALID_DEVICE_ID,           //16 < > device ID out of range
    BIRD_ERROR_FAILED_LOCKING_DEVICE,       //17 < > couldn't lock driver
    BIRD_ERROR_BOARD_MISSING_ITEMS,         //18 < > config space items missing
    BIRD_ERROR_NOTHING_ATTACHED,            //19 <O> card found but no sensors or transmitters attached
    BIRD_ERROR_SYSTEM_PROBLEM,              //20 <O> non specific system problem
    BIRD_ERROR_INVALID_SERIAL_NUMBER,       //21 <O> serial number does not exist in system
    BIRD_ERROR_DUPLICATE_SERIAL_NUMBER,     //22 <O> 2 identical serial numbers passed in set command
    BIRD_ERROR_FORMAT_NOT_SELECTED,         //23 <O> data format not selected yet
    BIRD_ERROR_COMMAND_NOT_IMPLEMENTED,     //24 < > valid command, not implemented yet
    BIRD_ERROR_INCORRECT_BOARD_DEFAULT,     //25 < > incorrect response to reading parameter
    BIRD_ERROR_INCORRECT_RESPONSE,          //26 <O> response received, but data,values in error
    BIRD_ERROR_NO_TRANSMITTER_RUNNING,      //27 < > there is no transmitter running
    BIRD_ERROR_INCORRECT_RECORD_SIZE,       //28 < > data record size does not match data format size
    BIRD_ERROR_TRANSMITTER_OVERCURRENT,     //29 <I> transmitter over-current detected
    BIRD_ERROR_TRANSMITTER_OPEN_CIRCUIT,    //30 <I> transmitter open circuit or removed
    BIRD_ERROR_SENSOR_EEPROM_FAILURE,       //31 <I> sensor bad eeprom
    BIRD_ERROR_SENSOR_DISCONNECTED,         //32 <I> previously good sensor has been removed
    BIRD_ERROR_SENSOR_REATTACHED,           //33 <I> previously good sensor has been reattached
    BIRD_ERROR_NEW_SENSOR_ATTACHED,         //34 <O> new sensor attached
    BIRD_ERROR_UNDOCUMENTED,                //35 <I> undocumented error code received from bird
    BIRD_ERROR_TRANSMITTER_REATTACHED,      //36 <I> previously good transmitter has been reattached
    BIRD_ERROR_WATCHDOG,                    //37 < > watchdog timeout
    BIRD_ERROR_CPU_TIMEOUT_START,           //38 <I> CPU ran out of time executing algorithm (start)
    BIRD_ERROR_PCB_RAM_FAILURE,             //39 <I> BIRD on-board RAM failure
    BIRD_ERROR_INTERFACE,                   //40 <I> BIRD PCI interface error
    BIRD_ERROR_PCB_EPROM_FAILURE,           //41 <I> BIRD on-board EPROM failure
    BIRD_ERROR_SYSTEM_STACK_OVERFLOW,       //42 <I> BIRD program stack overrun
    BIRD_ERROR_QUEUE_OVERRUN,               //43 <I> BIRD error message queue overrun
    BIRD_ERROR_PCB_EEPROM_FAILURE,          //44 <I> PCB bad EEPROM
    BIRD_ERROR_SENSOR_SATURATION_END,       //45 <I> Sensor has gone out of saturation
    BIRD_ERROR_NEW_TRANSMITTER_ATTACHED,    //46 <O> new transmitter attached
    BIRD_ERROR_SYSTEM_UNINITIALIZED,        //47 < > InitializeBIRDSystem not called yet
    BIRD_ERROR_12V_SUPPLY_FAILURE,          //48 <I > 12V Power supply not within specification
    BIRD_ERROR_CPU_TIMEOUT_END,             //49 <I> CPU ran out of time executing algorithm (end)
    BIRD_ERROR_INCORRECT_PLD,               //50 < > PCB PLD not compatible with this API DLL
    BIRD_ERROR_NO_TRANSMITTER_ATTACHED,     //51 < > No transmitter attached to this ID
    BIRD_ERROR_NO_SENSOR_ATTACHED,          //52 < > No sensor attached to this ID

    // new error codes added 2/27/03 
    // (Version 1,31,5,01)  multi-sensor, synchronized
    BIRD_ERROR_SENSOR_BAD,                  //53 < > Non-specific hardware problem
    BIRD_ERROR_SENSOR_SATURATED,            //54 < > Sensor saturated error
    BIRD_ERROR_CPU_TIMEOUT,                 //55 < > CPU unable to complete algorithm on current cycle
    BIRD_ERROR_UNABLE_TO_CREATE_FILE,       //56 < > Could not create and open file for saving setup
    BIRD_ERROR_UNABLE_TO_OPEN_FILE,         //57 < > Could not open file for restoring setup
    BIRD_ERROR_MISSING_CONFIGURATION_ITEM,  //58 < > Mandatory item missing from configuration file
    BIRD_ERROR_MISMATCHED_DATA,             //59 < > Data item in file does not match system value
    BIRD_ERROR_CONFIG_INTERNAL,             //60 < > Internal error in config file handler
    BIRD_ERROR_UNRECOGNIZED_MODEL_STRING,   //61 < > Board does not have a valid model string
    BIRD_ERROR_INCORRECT_SENSOR,            //62 < > Invalid sensor type attached to this board
    BIRD_ERROR_INCORRECT_TRANSMITTER,       //63 < > Invalid transmitter type attached to this board

    // new error code added 1/18/05
    // (Version 1.31.5.22) 
    //		multi-sensor, 
    //		synchronized-fluxgate, 
    //		integrating micro-sensor,
    //		flat panel transmitter
    BIRD_ERROR_ALGORITHM_INITIALIZATION,    //64 < > Flat panel algorithm initialization failed

    // new error code for multi-sync
    BIRD_ERROR_LOST_CONNECTION,             //65 < > USB connection has been lost
    BIRD_ERROR_INVALID_CONFIGURATION,       //66 < > Invalid configuration

    // VPD error code
    BIRD_ERROR_TRANSMITTER_RUNNING,         //67 < > TX running while reading/writing VPD

    BIRD_ERROR_MAXIMUM_VALUE = 0x7F         //	     ## value = number of error codes ##
};

//// error message defines
//#define ERROR_FLAG					0x80000000
//#define WARNING_FLAG				0x40000000

//#define XMTR_ERROR_SOURCE			0x20000000
//#define RCVR_ERROR_SOURCE			0x10000000
//#define BIRD_ERROR_SOURCE			0x08000000

//#define DIAG_ERROR_SOURCE			0x04000000

//// SYSTEM error = none of the above

//// NOTE: The MULTIPLE_ERRORS flag is no longer generated
//// It has been left in for backwards compatibility
//#define MULTIPLE_ERRORS				0x04000000

//// DEVICE STATUS ERROR BIT DEFINITIONS
//#define VALID_STATUS				0x00000000
//#define GLOBAL_ERROR				0x00000001
//#define NOT_ATTACHED				0x00000002
//#define SATURATED					0x00000004
//#define BAD_EEPROM					0x00000008
//#define HARDWARE					0x00000010
//#define NON_EXISTENT				0x00000020
//#define UNINITIALIZED				0x00000040
//#define NO_TRANSMITTER_RUNNING		0x00000080
//#define BAD_12V						0x00000100
//#define CPU_TIMEOUT					0x00000200
//#define INVALID_DEVICE				0x00000400
//#define NO_TRANSMITTER_ATTACHED		0x00000800
//#define OUT_OF_MOTIONBOX			0x00001000
//#define ALGORITHM_INITIALIZING		0x00002000

//#define TRUE	1
//#define FALSE	0
#endregion

#region DOUBLE_POSITION_ANGLES_RECORD
[StructLayout(LayoutKind.Sequential)]
public struct DOUBLE_POSITION_ANGLES_RECORD
{
    public double x;
    public double y;
    public double z;
    public double a;
    public double e;
    public double r;
}
#endregion
