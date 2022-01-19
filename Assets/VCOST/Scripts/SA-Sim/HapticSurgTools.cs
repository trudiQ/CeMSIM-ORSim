using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using HapticPlugin;


//! This object can be applied to the stylus of a haptic device. 
//! It allows you to pick up simulated objects and feel the involved physics.
//! Optionally, it can also turn off physics interaction when nothing is being held.
public class HapticSurgTools : MonoBehaviour
{
    /// Haptic device
    //public int buttonID = 0;		//!< index of the button assigned to grabbing.  Defaults to the first button
    public bool ButtonActsAsToggle = false; //!< Toggle button? as opposed to a press-and-hold setup?  Defaults to off.
    private HapticPlugin hapticDevice = null;    //!< Reference to the Haptic Device
                                                 //private  GameObject hapticDevice = null;   //!< Reference to the GameObject representing the Haptic Device
    private bool[] buttonStatus = { false, false }; //!< Are the buttons currently pressed? {first, second}
                                                    //private bool buttonStatus = false;			//!< Is the button currently pressed?

    /// tool selector
    private bool bActive = false; // all tools, selector; where or not itself is active
                                  // variables for each tool, selector
    private bool bJustSwitch2Tool = false; // just tools, indicate just switch to a tool from a selector
    public Vector3 initialPos; // all tools, selector
    public Quaternion initialRot; // all tools, selector
    public Bounds toolBbox; // valid for tools only but accessed by selectors
    private List<HapticSurgTools> tool4Select = new List<HapticSurgTools>(); // valid for tools only but accessed by selectors
    private HapticSurgTools seleSurgTool = null; // valid for selectors only but accessed by tools
    public List<GameObject> toolHapticGO = new List<GameObject>(); // valid for tools only but accessed by selectors
    private GameObject seleHapticGO = null; // valid for selectors only
    public Transform defaultParent; // The original parent object of this haptic tool

    /// touching, grabing, holding, cutting tool actions (some of them could be true at the same time)
    private bool bTouching = false; //forceps, scissors
    private bool bGrabbing = false; //forceps
    private bool bHolding = false; //forceps
    private bool bCutting = false; //scissors
    public int[] holdSphereIDs = new int[] { -1, -1, -1 }; //[objIdx, layerIdx, sphereIdx]
    public int[] cutSphereIdx = new int[] { -1, -1, -1 }; //[objIdx, layerIdx, sphereIdx]
    public GameObject toolTipSphere; // forceps only
    private GameObject touching = null;         //!< Reference to the object currently touched
    private GameObject grabbing = null;         //!< Reference to the object currently grabbed
    private Joint joint = null;            //!< The Unity physics joint created between the stylus and the object being grabbed.
    private FixedJoint holdJoint = null;        //!< Attach the tool to the sphere being held (forceps only)
    public enum PhysicsToggleStyle { none, onTouch, onGrab };
    public PhysicsToggleStyle physicsToggleStyle = PhysicsToggleStyle.none;   //!< Should the grabber script toggle the physics forces on the stylus? 
    public bool DisableUnityCollisionsWithTouchableObjects = true;

    // feedback force related 
    private bool bEffectStopped = true;
    private int FXID; // ID of the effect of the haptic device
    private bool bTouching_pre = false;
    private int effectType = 3; // friction
    private double[] pos = new double[] { 0.0d, 0.0d, 0.0d };
    private double[] dir = new double[] { 0.0d, 0.0d, 0.0d };
    [Range(0.0f, 1.0f)] private double Gain = 0.333f;
    [Range(0.0f, 1.0f)] private double Magnitude = 0.333f;
    [Range(1.0f, 1000.0f)] private double Frequency = 200.0f;

    // Actions: tool-tissue interaction relate (allis forcep and scissor)
    //		to be called by sim state-machine machanism in 'globalOperators.cs'
    public enum toolAction { idle, touching, grabbing, holding, cutting };
    public toolAction curAction = toolAction.idle; // idle - no action by default

    // globalOperators
    public globalOperators gOperators = null; // forceps only

    // omni tools animations
    public OmniToolsAnimations tAnimations = null;

    // tool collision stopper
    /// Algorithm: when tool collide a colon sphere, save the point of collision (pB) and the position of the sphere (pA), tool tip collider position will be pC
    /// also save the haptics position pD
    /// continue measure the angle between the vector pApB to pApC, when the angle is greater than the threshold then stop the tool
    /// when the tool is stopped, continue measure the distance between the still updating haptics position pE to pA, if the length of pEpA > pDpA, then restore tool movement
    public GameObject jointObject; // Created joint object to attach colon sphere towards
    public Transform collisionObjectTrans; // Transform of the object that is colliding with the tool
    public Vector3 collidingPoint; // World position where the tool collide the object
    public Vector3 collidingObjectPoint; // World position of the collided object when the tool collide the object
    public Vector3 objectCollidingPoint; // World position of the tool when it collide the object
    public Transform collidingTip; // Which tip collider collided
    public Vector3 toolLockingPosition; // Where do we lock the tool's movement
    public Quaternion toolLockingRotation;
    public bool lockToolDueColonContact; // Do we lock tool movement due to collision with colon
    public float stopThresholdAngle; // 
    public float breakPushingJointDistance; // If colliding tool tip is further away from the collided colon sphere's colliding position then break the joints
    public Vector3 collidingTipLocalPosition; // Local position of the tool collider tip which collides the colon
    public Quaternion collidingTipLocalRotation;
    public Rigidbody tipBody; // Rigidbody added to the tip when attaching joint to colon
    public List<Joint> pushColonJoints; // Joints created to let the tool push colon mesh
    public List<ToolColonSphereJoint> createdJoints;
    public GameObject lockToolDummyMesh; // The dummy mesh to turn on when the tool is locked
    public List<MeshRenderer> toolMesh;

    // Colon lifting
    public bool isLifting;

    // Improved grabbing
    public Transform grabbingSphereAnchor;
    public List<Joint> neighborSphereJoints;
    public GameObject originalTool;
    public GameObject dummyManipulator;
    public Vector3 toolGrabStartPos;
    public Vector3 hapticsGrabStartRaw;
    public float forcepsMaxGrabDistance; // How far can foreps grab the colon sphere away from the initial grabbing position before it stops
    //public Vector3

    //! Automatically called for initialization
    void Start()
    {
        GameObject gOperatorsGO = GameObject.Find("globalOperators");
        if (gOperatorsGO)
            gOperators = gOperatorsGO.GetComponent<globalOperators>();

        // Initialize the haptic device
        HapticPlugin[] hapticDevices = (HapticPlugin[])FindObjectsOfType(typeof(HapticPlugin));

        for (int ii = 0; ii < hapticDevices.Length; ii++)
        {
            if (hapticDevices[ii].hapticManipulator == this.gameObject)
            {
                hapticDevice = hapticDevices[ii];
                if (physicsToggleStyle != PhysicsToggleStyle.none)
                    hapticDevice.PhysicsManipulationEnabled = false;

                // Generate an OpenHaptics effect ID for each of the devices
                FXID = HapticPlugin.effects_assignEffect(hapticDevice.configName);
            }
        }

        if (DisableUnityCollisionsWithTouchableObjects)
            disableUnityCollisions();

        // initialize bbox of each tool: forceps and scissors
        if (this.gameObject.name == "Forceps" || this.gameObject.name == "Scissors" ||
            this.gameObject.name == "Forceps1" || this.gameObject.name == "Forceps2")
        {
            bActive = false; // only selector is active by default
                             //if (this.gameObject.name == "Scissors")
                             //	bActive = true; // remove once get R-Ball

            toolBbox = GetBbox(this.gameObject, true);
            initialPos = this.gameObject.transform.position;
            initialRot = new Quaternion(this.gameObject.transform.rotation.x,
                                        this.gameObject.transform.rotation.y,
                                        this.gameObject.transform.rotation.z,
                                        this.gameObject.transform.rotation.w);

            if (this.gameObject.name == "Forceps" || this.gameObject.name == "Forceps1" || this.gameObject.name == "Forceps2")
            {
                GameObject forcepswithhaptic = null;
                if (this.gameObject.name == "Forceps")
                    forcepswithhaptic = GameObject.Find("ForcepsWithHaptic");
                else if (this.gameObject.name == "Forceps1")
                    forcepswithhaptic = GameObject.Find("ForcepsWithHaptic1");
                else if (this.gameObject.name == "Forceps2")
                    forcepswithhaptic = GameObject.Find("ForcepsWithHaptic2");

                if (forcepswithhaptic)
                {
                    GameObject forceps = forcepswithhaptic.transform.GetChild(0).gameObject;
                    toolHapticGO.Add(forcepswithhaptic.transform.GetChild(1).gameObject);// haptics object of tool disabled by default
                    if (forceps)
                    {
                        tool4Select.Add(forceps.GetComponent<HapticSurgTools>());
                        //toolTipSphere = forceps.transform.GetChild(2).gameObject;
                        //toolTipSphere = forceps.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                    }
                    toolHapticGO[0].SetActive(false);
                }
                GameObject toolSelector = GameObject.Find("LeftToolSelector");
                if (toolSelector)
                {
                    seleSurgTool = toolSelector.transform.GetChild(0).gameObject.GetComponent<HapticSurgTools>();
                    seleHapticGO = toolSelector.transform.GetChild(1).gameObject; // haptics object of selector
                }
            }
            else if (this.gameObject.name == "Scissors")
            {
                GameObject scissorswithhaptic = GameObject.Find("ScissorsWithHaptic");
                if (scissorswithhaptic)
                {
                    toolHapticGO.Add(scissorswithhaptic.transform.GetChild(1).gameObject);
                    // disable haptics by default as the tool is inactive 
                    toolHapticGO[0].GetComponent<HapticPlugin>().hapticManipulator = null;
                }
            }

            // tool animations
            if (gOperatorsGO)
                tAnimations = gOperatorsGO.GetComponent<OmniToolsAnimations>();
        }

        // initialize refs to the tools to select
        if (this.gameObject.name == "LBall")
        {
            bActive = true;

            string[] toolNames = { "ForcepsWithHaptic", "ForcepsWithHaptic1", "ForcepsWithHaptic2" };
            GameObject forcepswithhaptic = null;
            for (int i = 0; i < toolNames.Length; i++)
            {
                forcepswithhaptic = GameObject.Find(toolNames[i]);

                if (forcepswithhaptic)
                {
                    GameObject forceps = forcepswithhaptic.transform.GetChild(0).gameObject;
                    toolHapticGO.Add(forcepswithhaptic.transform.GetChild(1).gameObject);// haptics object of tool disabled by default
                    if (forceps)
                        tool4Select.Add(forceps.GetComponent<HapticSurgTools>());
                }
            }

            GameObject toolSelector = this.gameObject.transform.parent.gameObject;
            if (toolSelector)
            {
                seleHapticGO = toolSelector.transform.GetChild(1).gameObject; // haptics object of selector
            }
            initialPos = this.gameObject.transform.position;

            // Switch motion recording object to omni
            LSSimDataRecording.currentPickedForceps = -1;
            LSSimDataRecording.forcepsTip = this.gameObject.transform;
        }

        defaultParent = transform.parent;

        pushColonJoints = new List<Joint>();
        neighborSphereJoints = new List<Joint>();
    }

    void disableUnityCollisions()
    {
        GameObject[] touchableObjects;
        touchableObjects = GameObject.FindGameObjectsWithTag("Touchable") as GameObject[];  //FIXME  Does this fail gracefully?

        // Ignore my collider
        Collider myC = gameObject.GetComponent<Collider>();
        if (myC != null)
            foreach (GameObject T in touchableObjects)
            {
                Collider CT = T.GetComponent<Collider>();
                if (CT != null)
                    Physics.IgnoreCollision(myC, CT);
            }

        // Ignore colliders in children.
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (Collider C in colliders)
            foreach (GameObject T in touchableObjects)
            {
                Collider CT = T.GetComponent<Collider>();
                if (CT != null)
                    Physics.IgnoreCollision(C, CT);
            }

    }

    //! Parse touching/grasping sphere's name into [objIdx, layerIdx, sphereIdx]
    public bool parseSphereName(string sphereName, ref int[] sphereIDs)
    {
        // make sure the game object is a sphere of sphereJoint model
        string[] nameSplit = sphereName.Split('_');
        if (nameSplit.Length != 4 || nameSplit[0] != "sphere")
            return false;

        // parse the name
        for (int i = 0; i < sphereIDs.Length; i++)
            sphereIDs[i] = Int32.Parse(nameSplit[i + 1]);

        return true;
    }

    //! Get bounding box of the tool (gameobj) and enlarge it a bit
    Bounds GetBbox(GameObject obj, bool bEnlarge)
    {
        var b = new Bounds(obj.transform.position, Vector3.zero);
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }

        if (bEnlarge)
            b.Expand(0.5f);
        return b;
    }

    //! Refine touching results by updating GameObj "touching" to make sure:
    //	1. Ease grabing/holding, in case nothing caught by collision
    //	2. Always grab/hold top layer spheres
    // Return: -1: error; 0: no need to refine, good to go; 1: refined
    int refineTouching()
    {
        if (!toolTipSphere || !gOperators)
            return -1;

        Vector3 toolTip = toolTipSphere.transform.position;
        int numSphereJointModels = gOperators.m_numSphereModels;

        if (numSphereJointModels <= 0)
            return -1;


        // Check if current touching sphere is from a top layer
        int[] sphereID = new int[3];
        int[] topLayerSphereIndices = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
        int[] botLayerSphereIndices = { 0, 1, 2, 3, 15, 16, 17, 18, 19 };
        var topBomPairs = new Dictionary<int, int>
        {
            {0, 9},{1, 8},{2, 7},{3, 6},{15, 14},{16, 13},{17, 12},{18, 11},{19, 10}
        };
        if (touching)
        {
            if (parseSphereName(touching.name, ref sphereID))
            {
                foreach (int val in botLayerSphereIndices)
                {
                    // grasp a bottom layer, relace it with its corresponding top layer
                    if (val == sphereID[2])
                    {
                        touching = gOperators.m_sphereJointModels[sphereID[0]].m_sphereGameObjects[sphereID[1], topBomPairs[sphereID[2]]];
                        Debug.Log("refineTouching: replaced with sphereID {" + sphereID[0].ToString() + ", " + sphereID[1].ToString()
                                                       + ", " + topBomPairs[sphereID[2]].ToString() + "} ");
                        return 1;
                    }
                }
            }
            else
                return -1;
            // good! touching from top layer
            return 0;
        }

        // Nothing touched, manual search
        if (!touching || !bTouching)
        {
            int i, j, numLayers;
            List<int> frontLayerOfInterest = new List<int>();
            // broad search: sphereJointModels' layer bounding boxes
            for (i = 0; i < numSphereJointModels; i++)
            {
                frontLayerOfInterest.Add(-1);
                numLayers = gOperators.m_sphereJointModels[i].m_numLayers;
                for (j = 0; j < numLayers; j++)
                {
                    if (gOperators.m_sphereJointModels[i].m_layerBoundingBox[j].Contains(toolTip))
                    {
                        frontLayerOfInterest[i] = j;
                        break;
                    }
                }
            }

            // Find sphere closest to the tooltip & top layer
            bool bSkipSphere = false;
            int numSpheres;
            float distance;
            float minDistance = float.MaxValue;
            float touchDistThreshold = gOperators.m_sphereJointModels[0].m_sphereRadius + 1f;
            int[] clostestSphereID = new int[] { -1, -1, -1 };
            for (i = 0; i < numSphereJointModels; i++)
            {
                if (frontLayerOfInterest[i] < 0)
                    continue;
                numSpheres = gOperators.m_sphereJointModels[i].m_numSpheres;
                for (j = 0; j < numSpheres; j++)
                {
                    // check distance with spheres from bottom layer only
                    bSkipSphere = false;
                    foreach (int val in botLayerSphereIndices)
                    {
                        if (val == j)
                        {
                            bSkipSphere = true;
                            break;
                        }
                    }
                    if (bSkipSphere)
                        continue;
                    distance = Vector3.Distance(gOperators.m_sphereJointModels[i].m_spherePos[frontLayerOfInterest[i] * numSpheres + j], toolTip);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        clostestSphereID = new int[] { i, frontLayerOfInterest[i], j };
                    }
                }
            }

            // check if the the closest sphere is close enough
            if (minDistance <= touchDistThreshold && clostestSphereID[0] >= 0)
            {
                touching = gOperators.m_sphereJointModels[clostestSphereID[0]].m_sphereGameObjects[clostestSphereID[1], clostestSphereID[2]];
                bTouching = true;
                //Debug.Log("refineTouching: sphereID {" + clostestSphereID[0].ToString() + ", " + clostestSphereID[1].ToString()
                //                                        + ", " + clostestSphereID[2].ToString() + "} ");
            }
        }

        // still got nothing, maybe too far away
        if (!touching || !bTouching)
            return 0;

        return 1;
    }

    //! Verify the touching existes based on collision info, sometimes incorrect
    // check if there is actually touching object by checking tooltip within sphereJointModel's layer bbox
    // if no collision, set touching=null
    void verifyTouching()
    {
        if (!toolTipSphere || !gOperators)
            return;

        Vector3 toolTip = toolTipSphere.transform.position;
        int numSphereJointModels = gOperators.m_numSphereModels;

        if (numSphereJointModels <= 0)
            return;

        int i, j, numLayers;
        for (i = 0; i < numSphereJointModels; i++)
        {
            numLayers = gOperators.m_sphereJointModels[i].m_numLayers;
            for (j = 0; j < numLayers; j++)
            {
                if (gOperators.m_sphereJointModels[i].m_layerBoundingBox[j].Contains(toolTip))
                {
                    return; //good, detect actual touching, do nothing
                }
            }
        }

        // no actual touching, release touching
        touching = null;
        bTouching = false;
    }

    //! attach the tool to a sphere during holding
    void holdTool(int[] sphereID)
    {
        if (!gOperators)
        {
            Debug.Log("Error(holdTool): gOperators is null");
            return;
        }

        int numSphereJointModels = gOperators.m_numSphereModels;
        if (numSphereJointModels <= 0 || gOperators.m_sphereJointModels.Length <= 0)
        {
            Debug.Log("Error(holdTool): no sphereJointModels!");
            return;
        }

        if (sphereID[0] < 0 || sphereID[0] >= numSphereJointModels)
        {
            Debug.Log("Error(holdTool): invalid sphere being held!");
            return;
        }

        GameObject sphereHold = gOperators.m_sphereJointModels[sphereID[0]].m_sphereGameObjects[sphereID[1], sphereID[2]];
        if (!sphereHold)
        {
            Debug.Log("Error(holdTool): sphereHold is null!");
            return;
        }

        this.holdJoint = sphereHold.AddComponent<FixedJoint>();
        this.holdJoint.connectedBody = this.gameObject.GetComponent<Rigidbody>();
    }

    void releaseHoldTool()
    {
        if (this.holdJoint == null)
            return;

        Destroy(this.holdJoint);
    }

    //! Update is called once per frame
    void FixedUpdate()
    {
        // update forcep and scissors' bboxes
        if (this.gameObject.name == "Forceps" || this.gameObject.name == "Scissors" ||
            this.gameObject.name == "Forceps1" || this.gameObject.name == "Forceps2")
        {
            toolBbox = GetBbox(this.gameObject, true);
        }

        bool[] newButtonStatus = { false, false };
        if (hapticDevice)
            newButtonStatus = new bool[] { hapticDevice.Buttons[0] == 1, hapticDevice.Buttons[1] == 1 };
        bool[] oldButtonStatus = { buttonStatus[0], buttonStatus[1] };
        buttonStatus = new bool[] { newButtonStatus[0], newButtonStatus[1] };

        // Tool selectors
        if (this.gameObject.name == "LBall")
        {
            if (bActive)
            {
                if (newButtonStatus[0])
                {
                    // check LBall inside a forcep's bbox
                    for (int i = 0; i < tool4Select.Count; i++)
                    {
                        if (tool4Select[i].toolBbox.Contains(this.gameObject.transform.position))
                        {
                            if (tool4Select[i].bHolding)
                            {
                                tool4Select[i].releaseHoldTool();
                                tool4Select[i].bHolding = false;
                            }
                            // select the forceps, enable its haptics
                            toolHapticGO[i].SetActive(true);
                            // Update which forceps should be tracked
                            if (i < 3)
                            {
                                LSSimDataRecording.currentPickedForceps = i;
                                LSSimDataRecording.forcepsTip = tool4Select[i].toolTipSphere.transform;
                            }
                            // Update the active forceps for the colon movement controller
                            ColonMovementController.instance.currentGrabbingForcepsController = toolHapticGO[i].GetComponent<HapticPlugin>();
                            // disable selector's haptics
                            seleHapticGO.SetActive(false);
                            bActive = false;
                            tool4Select[i].bActive = true;
                            // Update tool status UI for forceps picked up
                            gOperators.uiController.UpdateToolStatusText(LS_UIcontroller.GetForcepsNameForToolStatusUI(i), "Held by Omni");
                            tool4Select[i].bJustSwitch2Tool = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                // reset the selector position
                this.gameObject.transform.position = initialPos;
                return;
            }
        }

        // Graspping: Forceps only, Button pressing check
        if (this.gameObject.name == "Forceps" || this.gameObject.name == "Forceps1" || this.gameObject.name == "Forceps2")
        {
            // validate touching
            verifyTouching();

            if (bActive == false)// forceps disabled
            {
                // reset the forceps
                if (!bHolding)
                {
                    this.gameObject.transform.position = initialPos;
                    this.gameObject.transform.rotation = new Quaternion(initialRot.x, initialRot.y, initialRot.z, initialRot.w);
                }
                else
                {
                    // release the forceps from holding when the final closure is done
                    if (gOperators && globalOperators.m_bFinalClosure)
                    {
                        releaseHoldTool();
                        bHolding = false;
                        if (tAnimations)
                            tAnimations.OpenForceps(this.gameObject.name);
                    }
                }
            }
            else // forceps enabled
            {
                // Left button for grabbing
                if (oldButtonStatus[0] == false && newButtonStatus[0] == true)
                {
                    if (tAnimations && bJustSwitch2Tool == false)
                        tAnimations.CloseForceps(this.gameObject.name);

                    // filter touching info.
                    refineTouching();

                    if (ButtonActsAsToggle)
                    {
                        if (grabbing)
                        {
                            release();
                            bGrabbing = false;
                        }
                        else
                        {
                            grab();
                            bGrabbing = true;
                        }
                    }
                    else
                    {
                        grab();
                        bGrabbing = true;
                    }
                }
                if (oldButtonStatus[0] == true && newButtonStatus[0] == false)
                {
                    if (tAnimations)
                        tAnimations.OpenForceps(this.gameObject.name);

                    if (ButtonActsAsToggle)
                    {
                        //Do Nothing
                    }
                    else
                    {
                        release();
                        bGrabbing = false;
                    }
                }

                // Make sure haptics is ON if we're grabbing
                if (grabbing && physicsToggleStyle != PhysicsToggleStyle.none)
                    hapticDevice.PhysicsManipulationEnabled = true;
                if (!grabbing && physicsToggleStyle == PhysicsToggleStyle.onGrab)
                    hapticDevice.PhysicsManipulationEnabled = false;

                // right button for tool dropping/holding
                if (newButtonStatus[1] == true)
                {
                    //
                    ResumeHapticsUpdateVirtualTool();

                    // holding
                    if (bTouching && touching && !bHolding)
                    {
                        // check which object being hold
                        int[] sphereIDs = new int[3]; //[objIdx, layerIdx, sphereIdx]
                        if (parseSphereName(touching.name, ref sphereIDs))
                        {
                            if (sphereIDs[1] < 2) // only first 2 layer spheres can be held
                            {
                                holdSphereIDs = new int[] { sphereIDs[0], sphereIDs[1], sphereIDs[2] };
                                holdTool(sphereIDs);
                                bHolding = true;
                                Debug.Log("Holding sphere: " + sphereIDs[0].ToString() + "," + sphereIDs[1].ToString() + "," + sphereIDs[2].ToString());
                                if (tAnimations)
                                    tAnimations.CloseForceps(this.gameObject.name);
                                // Update UI
                                gOperators.uiController.UpdateToolStatusText(LS_UIcontroller.GetForcepsNameForToolStatusUI(LSSimDataRecording.currentPickedForceps), "Grasping colon");
                            }
                            else
                            {
                                holdSphereIDs = new int[] { -1, -1, -1 };
                                bHolding = false;
                                Debug.Log("Attemp holding sphere out of range; move forceps to colon edge");
                                // Update UI
                                gOperators.uiController.UpdateToolStatusText(LS_UIcontroller.GetForcepsNameForToolStatusUI(LSSimDataRecording.currentPickedForceps), "Idle");
                            }
                        }
                        else
                        {
                            holdSphereIDs = new int[] { -1, -1, -1 };
                            bHolding = false;
                            Debug.Log("Attemp holding a non-sphere obj");
                            // Update UI
                            gOperators.uiController.UpdateToolStatusText(LS_UIcontroller.GetForcepsNameForToolStatusUI(LSSimDataRecording.currentPickedForceps), "Idle");
                        }

                        // release the touching obj when holding
                        if (bHolding)
                        {
                            bTouching = false;
                            touching = null;
                        }
                    }
                    // switch to selector
                    if (!bTouching)
                    {
                        // select the selector, enable its haptics
                        seleHapticGO.SetActive(true);
                        // disable forceps's haptics
                        toolHapticGO[0].SetActive(false);
                        bActive = false;
                        // Update UI
                        gOperators.uiController.UpdateToolStatusText(LS_UIcontroller.GetForcepsNameForToolStatusUI(LSSimDataRecording.currentPickedForceps), "Idle");
                        seleSurgTool.bActive = true;

                        // Switch motion recording object back to omni
                        LSSimDataRecording.currentPickedForceps = -1;
                        LSSimDataRecording.forcepsTip = seleSurgTool.transform;
                    }
                }
            }
        }

        // Touching: both forceps and scissors, no button, force feedback when touching an object
        if (this.gameObject.name == "Forceps" || this.gameObject.name == "Scissors" ||
            this.gameObject.name == "Forceps1" || this.gameObject.name == "Forceps2")
        {
            if (FXID == -1)
            {
                FXID = HapticPlugin.effects_assignEffect(hapticDevice.configName);
            }
            if (FXID == -1) // Still broken?
            {
                Debug.LogError("Unable to assign Haptic effect.");
                return;
            }

            bTouching = touching ? true : false;

            if ((this.gameObject.name == "Forceps" && bActive == true) || (this.gameObject.name == "Scissors" && bActive == true) ||
                (this.gameObject.name == "Forceps1" && bActive == true) || (this.gameObject.name == "Forceps2" && bActive == true))
            {
                if (bTouching == true)
                {
                    HapticPlugin.effects_settings(
                        hapticDevice.configName,
                        FXID,
                        Gain,
                        Magnitude,
                        Frequency,
                        pos,
                        dir);
                    HapticPlugin.effects_type(
                        hapticDevice.configName,
                        FXID,
                        effectType);
                    if (bEffectStopped)
                    {
                        HapticPlugin.effects_startEffect(hapticDevice.configName, FXID);
                        bEffectStopped = false;
                    }

                    // Update Tool UI
                    switch (this.gameObject.name)
                    {
                        case "Forceps":
                            gOperators.uiController.UpdateToolStatusText("forceps1", "Touching colon");
                            break;
                        case "Forceps1":
                            gOperators.uiController.UpdateToolStatusText("forceps2", "Touching colon");
                            break;
                        case "Forceps2":
                            gOperators.uiController.UpdateToolStatusText("forceps3", "Touching colon");
                            break;
                        case "Scissors":
                            gOperators.uiController.UpdateToolStatusText("scissors", "Touching colon");
                            break;
                    }
                }
                else // bTouching == false
                {
                    if (bEffectStopped == false)
                    {
                        HapticPlugin.effects_stopEffect(hapticDevice.configName, FXID);
                        bEffectStopped = true;
                    }
                    // manually stopEffect in case it's not, but flag set so
                    if (!bJustSwitch2Tool && oldButtonStatus[0] == false && newButtonStatus[0] == true)
                    {
                        HapticPlugin.effects_stopEffect(hapticDevice.configName, FXID);
                        Debug.Log("effects_stopEffect: " + hapticDevice.configName + ", " + this.gameObject.name);
                    }

                    // Update Tool UI
                    switch (this.gameObject.name)
                    {
                        case "Forceps":
                            gOperators.uiController.UpdateToolStatusText("forceps1", "Held by Omni");
                            break;
                        case "Forceps1":
                            gOperators.uiController.UpdateToolStatusText("forceps2", "Held by Omni");
                            break;
                        case "Forceps2":
                            gOperators.uiController.UpdateToolStatusText("forceps3", "Held by Omni");
                            break;
                        case "Scissors":
                            gOperators.uiController.UpdateToolStatusText("scissors", "Held by Omni");
                            break;
                    }
                }
            }

            // manually stopEffect in case it's not, but flag set so
            if ((this.gameObject.name == "Forceps" || this.gameObject.name == "Forceps1" || this.gameObject.name == "Forceps2")
                && bActive == false && bEffectStopped == false)
            {
                HapticPlugin.effects_stopEffect(hapticDevice.configName, FXID);
                Debug.Log("effects_stopEffect: " + hapticDevice.configName + ", " + this.gameObject.name);
                bEffectStopped = true;
            }

            bJustSwitch2Tool = false;
        }

        // Scissors, left-button: activate/cutting; right-button: reset 
        if (this.gameObject.name == "Scissors")
        {
            if (bActive == false) // scissors are not activated by default
            {
                // reset to initial position
                this.gameObject.transform.position = initialPos;
                this.gameObject.transform.rotation = new Quaternion(initialRot.x, initialRot.y, initialRot.z, initialRot.w);

                if (oldButtonStatus[0] == false && newButtonStatus[0] == true) // left-button activate
                {
                    bActive = true;
                    if (toolHapticGO.Count > 0)
                    {
                        toolHapticGO[0].GetComponent<HapticPlugin>().hapticManipulator = this.gameObject;
                    }

                    // Update Tool UI
                    gOperators.uiController.UpdateToolStatusText("scissors", "Held by Omni");
                }
            }
            else // bActivate == true: scissors are activated
            {
                //left button for cutting
                if (newButtonStatus[0] == true)
                {
                    if (bTouching && touching)
                    {
                        // check which object being cut
                        int[] sphereIDs = new int[3]; //[objIdx, layerIdx, sphereIdx]
                        if (parseSphereName(touching.name, ref sphereIDs))
                        {
                            // examine if the touching sphere within the layer & corner-cut range
                            bool bTouchSphereAtCorners = false;
                            if (sphereIDs[1] >= 0 && sphereIDs[1] <= 1) // layerIdx = 0 or 1
                            {
                                if (gOperators && gOperators.m_cornerSphereIdxRange.Length > 0) // sphereIdx in the corner-cut range
                                {
                                    for (int c = 0; c < gOperators.m_cornerSphereIdxRange.Length; c++)
                                    {
                                        if (sphereIDs[2] >= gOperators.m_cornerSphereIdxRange[c].Item1 && sphereIDs[2] <= gOperators.m_cornerSphereIdxRange[c].Item2)
                                        {
                                            bTouchSphereAtCorners = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (bTouchSphereAtCorners)
                            {
                                cutSphereIdx = new int[] { sphereIDs[0], sphereIDs[1], sphereIDs[2] };
                                bCutting = true;
                            }
                            else
                            {
                                cutSphereIdx = new int[] { -1, -1, -1 };
                                bCutting = false;
                                Debug.Log("Attemp cutting: cut sphere out-of-range!");
                            }
                            Debug.Log("Cutting: sphere ID: (" + sphereIDs[0].ToString() + "," + sphereIDs[1].ToString() + "," + sphereIDs[2].ToString() + ")");
                        }
                        else
                        {
                            cutSphereIdx = new int[] { -1, -1, -1 };
                            bCutting = false;
                            //Debug.Log("Attemp cutting: cut non-sphere obj!");
                        }
                    }
                    else // !bTouching
                    {
                        cutSphereIdx = new int[] { -1, -1, -1 };
                        bCutting = false;
                        //Debug.Log("Attemp cutting: not touch any sphere!");
                    }

                    if (tAnimations)
                    {
                        tAnimations.CloseOpenScissors(this, cutSphereIdx);
                    }
                }
                if (newButtonStatus[0] == false && curAction != toolAction.cutting)
                {
                    cutSphereIdx = new int[] { -1, -1, -1 };
                    bCutting = false;
                }

                // right button: de-activate scissors
                if (oldButtonStatus[1] == false && newButtonStatus[1] == true) // right-button de-activate
                {
                    bActive = false;
                    if (toolHapticGO.Count > 0)
                    {
                        toolHapticGO[0].GetComponent<HapticPlugin>().hapticManipulator = null;
                    }

                    // Update Tool UI
                    gOperators.uiController.UpdateToolStatusText("scissors", "Idle");
                }
            }
        }

        // tool-specific action determination 
        //	make sure one action at a time
        if (this.gameObject.name == "Forceps" || this.gameObject.name == "Forceps1" || this.gameObject.name == "Forceps2")
        {
            // priority: holding > grasping > touching > idle
            if (bHolding)
                curAction = toolAction.holding;
            else // !bHolding
            {
                if (bGrabbing)
                    curAction = toolAction.grabbing;
                else // !bGrabbing
                {
                    if (bTouching)
                        curAction = toolAction.touching;
                    else // !bTouching
                        curAction = toolAction.idle;
                }
            }
        }
        if (this.gameObject.name == "Scissors")
        {
            // priority: cutting > touching > idle
            if (curAction == toolAction.cutting)
            {
                //curAction = toolAction.cutting;
            }
            else // !bCutting
            {
                if (bTouching)
                    curAction = toolAction.touching;
                else // !bTouching
                    curAction = toolAction.idle;
            }
        }

        UpdateCollisionStatus();
    }

    private void hapticTouchEvent(bool isTouch)
    {
        if (physicsToggleStyle == PhysicsToggleStyle.onGrab)
        {
            if (isTouch)
                hapticDevice.PhysicsManipulationEnabled = true;
            //hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = true;
            else
                return; // Don't release haptics while we're holding something.
        }

        if (physicsToggleStyle == PhysicsToggleStyle.onTouch)
        {
            hapticDevice.PhysicsManipulationEnabled = isTouch;
            //hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = isTouch;
            GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
            GetComponentInParent<Rigidbody>().angularVelocity = Vector3.zero;

        }
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        Collider other = collisionInfo.collider;
        //Debug.unityLogger.Log("OnCollisionEnter : " + other.name);
        GameObject that = other.gameObject;
        Rigidbody thatBody = that.GetComponent<Rigidbody>();

        // If this doesn't have a rigidbody, walk up the tree. 
        // It may be PART of a larger physics object.
        while (thatBody == null)
        {
            //Debug.logger.Log("Touching : " + that.name + " Has no body. Finding Parent. ");
            if (that.transform == null || that.transform.parent == null)
                break;
            GameObject parent = that.transform.parent.gameObject;
            if (parent == null)
                break;
            that = parent;
            thatBody = that.GetComponent<Rigidbody>();
        }

        if (collisionInfo.rigidbody != null)
            hapticTouchEvent(true);

        if (thatBody == null)
            return;

        if (thatBody.isKinematic)
            return;

        touching = that;
        if (this.gameObject.name == "Scissors")
        {
            gOperators.scissorTouchingSphere = touching;
        }

        if (LinearStaplerTool.instance.simStates < 2 && touching.gameObject.name.Contains("sphere_"))
        {
            // Don't let surgeon lift colon if grab on further layers
            int layer = globalOperators.GetSphereLayer(touching.gameObject.name);
            if (layer > 1)
            {
                OnCollidingColon(collisionInfo);
            }
        }
    }
    void OnCollisionExit(Collision collisionInfo)
    {
        Collider other = collisionInfo.collider;
        //Debug.unityLogger.Log("onCollisionrExit : " + other.name);
        //collidingTip = null;

        if (collisionInfo.rigidbody != null)
            hapticTouchEvent(false);

        if (touching == null)
            return; // Do nothing

        if (other == null ||
            other.gameObject == null || other.gameObject.transform == null)
            return; // Other has no transform? Then we couldn't have grabbed it.

        if (touching == other.gameObject || other.gameObject.transform.IsChildOf(touching.transform))
        {
            touching = null;
            if (this.gameObject.name == "Scissors")
            {
                gOperators.scissorTouchingSphere = null;
            }
        }
    }

    //! Begin grabbing an object. (Like closing a claw.) Normally called when the button is pressed. 
    void grab()
    {
        GameObject touchedObject = touching;
        if (touchedObject == null) // No Unity Collision? 
        {
            // Maybe there's a Haptic Collision
            touchedObject = hapticDevice.touching;
            //touchedObject = hapticDevice.GetComponent<HapticPlugin>().touching;
        }

        if (grabbing != null) // Already grabbing
            return;
        if (touchedObject == null && !lockToolDueColonContact) // Nothing to grab
            return;

        // Grabbing a grabber is bad news.
        if (touchedObject.tag == "Gripper")
            return;

        //Debug.Log( " Object : " + touchedObject.name + "  Tag : " + touchedObject.tag );

        grabbing = touchedObject;

        toolGrabStartPos = transform.position;
        hapticsGrabStartRaw = hapticDevice.stylusPositionRaw;

        //Debug.logger.Log("Grabbing Object : " + grabbing.name);
        Rigidbody body = grabbing.GetComponent<Rigidbody>();

        if (pushColonJoints.Count > 0)
        {
            DetachColonStopPush();
        }

        // If this doesn't have a rigidbody, walk up the tree. 
        // It may be PART of a larger physics object.
        while (body == null)
        {
            //Debug.logger.Log("Grabbing : " + grabbing.name + " Has no body. Finding Parent. ");
            if (grabbing.transform.parent == null)
            {
                grabbing = null;
                return;
            }
            GameObject parent = grabbing.transform.parent.gameObject;
            if (parent == null)
            {
                grabbing = null;
                return;
            }
            grabbing = parent;
            body = grabbing.GetComponent<Rigidbody>();
        }

        isLifting = false;
        // Update colon grabbing states for colon motion controller
        // If touched colon is already cut
        int colon = int.Parse(body.gameObject.name[7].ToString());
        // Don't let surgeon lift colon if grab on further layers
        int layer = globalOperators.GetSphereLayer(body.gameObject.name);
        //if (!LinearStaplerTool.instance.usingRawSensorControl) // Temp, disable forceps lifting if testing using raw stapler input
        {
            if (gOperators.m_LRCornerCutIdices[colon][1 - colon]) 
            {
                // If grabbing first 2 layers
                if (LinearStaplerTool.instance.simStates < 2 && body.gameObject.name.Contains("sphere_"))
                {
                    if (layer == 0 || layer == 1)
                    {
                        gOperators.lsController.colonSecuredByForceps[colon] = true;
                        ColonMovementController.instance.ChangeFollowStates(colon, 1, ColonMovementController.instance.colon0FrontSphereStartPosition.Count == 0, false, ColonMovementController.instance.activeForcepsHaptic);
                        // Make forceps attach to the colon
                        toolHapticGO[0].GetComponent<HapticPlugin>().updateManipulatorTransform = false;
                        transform.parent = body.transform;
                        GetComponent<Rigidbody>().isKinematic = true;
                        GetComponent<Rigidbody>().mass = 10000000;
                        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                        ColonMovementController.instance.currentGrabbingForcepsTransform = transform;
                        ColonMovementController.instance.forcepsGrabInitialLocalPosition = transform.localPosition;

                        isLifting = true;

                    }
                }
            }
        }

        if (!isLifting)
        {
            // Don't move grabbed sphere if the colon is joined and the grabbed sphere is the supported sphere
            if (LinearStaplerTool.instance.simStates < 1 || !gOperators.supportedColonSpheresAfterJoining.Contains(grabbing.transform))
            {
                //grabbing.transform.position = collidingTip.position;
                grabbing.transform.position = grabbingSphereAnchor.position;
            }
            joint = (FixedJoint)gameObject.AddComponent(typeof(FixedJoint));
            joint.connectedBody = body;

            if (layer > 1)
            {
                List<Transform> neighborSpheres = GetNeighborColonSphere(grabbing.transform);
                foreach (Transform s in neighborSpheres)
                {
                    //s.position = s.position + (grabbing.transform.position - s.position) * 0.75f; // Bring neighbor sphere closer to the forceps
                    Joint joint = (SpringJoint)gameObject.AddComponent(typeof(SpringJoint));
                    joint.connectedBody = s.GetComponent<Rigidbody>();
                    neighborSphereJoints.Add(joint);
                }
            }
        }
        else
        {
            joint = (ConfigurableJoint)gameObject.AddComponent(typeof(ConfigurableJoint));
            joint.connectedBody = body;
        }

        // Update UI
        gOperators.uiController.UpdateToolStatusText(LS_UIcontroller.GetForcepsNameForToolStatusUI(LSSimDataRecording.currentPickedForceps), "Grasping colon");

        lockToolDueColonContact = false;
    }
    //! changes the layer of an object, and every child of that object.
    static void SetLayerRecursively(GameObject go, int layerNumber)
    {
        if (go == null) return;
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
            trans.gameObject.layer = layerNumber;
    }

    //! Stop grabbing an obhject. (Like opening a claw.) Normally called when the button is released. 
    void release()
    {
        // Update UI
        gOperators.uiController.UpdateToolStatusText(LS_UIcontroller.GetForcepsNameForToolStatusUI(LSSimDataRecording.currentPickedForceps), "Held by Omni");

        if (grabbing == null) //Nothing to release
        {
            ResumeHapticsUpdateVirtualTool();
            return;
        }

        // Update colon grabbing states for colon motion controller
        if (joint.connectedBody.gameObject.name.Contains("sphere_"))
        {
            // Do nothing if released sphere is not at front layers
            int layer = globalOperators.GetSphereLayer(joint.connectedBody.gameObject.name);
            if (layer == 0 || layer == 1)
            {
                int colon = int.Parse(joint.connectedBody.gameObject.name[7].ToString());
                gOperators.lsController.colonSecuredByForceps[colon] = false;
                if (ColonMovementController.instance.updateMode[colon] == 1)
                {
                    ColonMovementController.instance.ChangeFollowStates(colon, 0, false, false);
                }
                // Make forceps detach from the colon
                toolHapticGO[0].GetComponent<HapticPlugin>().updateManipulatorTransform = true;
                transform.parent = defaultParent;
                GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().mass = 1;
            }
        }

        Debug.Assert(joint != null);

        joint.connectedBody = null;
        Destroy(joint);

        neighborSphereJoints.ForEach(j => Destroy(j));
        neighborSphereJoints.Clear();

        grabbing = null;

        StartCoroutine(AfterRelease());
    }
    private IEnumerator AfterRelease()
    {
        yield return null;

        if (physicsToggleStyle != PhysicsToggleStyle.none)
            hapticDevice.PhysicsManipulationEnabled = false;
        //hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = false;

        ResumeHapticsUpdateVirtualTool();
    }

    //! Returns true if there is a current object. 
    public bool isGrabbing()
    {
        return (grabbing != null);
    }

    public void OnCollidingColon(Collision collision)
    {
        if (isGrabbing())
        {
            return;
        }

        if (ColonMovementController.instance.currentGrabbingForcepsController == null || toolHapticGO[0].GetComponent<HapticPlugin>() != ColonMovementController.instance.currentGrabbingForcepsController)
        {
            return;
        }

        //if (collidingTip != null)
        //{
        //    return;
        //}

        collisionObjectTrans = collision.collider.transform;
        collidingPoint = collision.GetContact(0).point;
        collidingObjectPoint = collision.collider.transform.position;
        objectCollidingPoint = transform.position;
        //collidingTip = collision.GetContact(0).thisCollider.transform;

        AttachColonForPush(collision.GetContact(0).point, collision.GetContact(0).thisCollider.gameObject, collisionObjectTrans.gameObject);
    }

    public void UpdateCollisionStatus()
    {
        if (grabbing)
        {
            return;
        }

        if (pushColonJoints.Count == 0)
        {
            return;
        }

        Vector3 collisionVector = collidingPoint - collidingObjectPoint;
        //if (Vector3.Angle(toolTipVector, collisionVector) > stopThresholdAngle && !lockToolDueColonContact)
        //{
        //    lockToolDueColonContact = true;
        //    toolLockingPosition = transform.position;
        //    toolLockingRotation = transform.rotation;
        //    LockToolDueColonCollision();
        //}
        Vector3 collisionDistance = objectCollidingPoint - collidingObjectPoint;
        Vector3 toolDistance = transform.position - collidingObjectPoint;
        //if (toolDistance.magnitude > collisionDistance.magnitude && lockToolDueColonContact)
        //{
        //    lockToolDueColonContact = false;
        //    UnlockToolDueColonCollision();
        //}
        //if (collidingTip == null || Vector3.Distance(collidingTip.position, collidingObjectPoint) > breakPushingJointDistance)
        //{
        //    DetachColonStopPush();
        //}

        //if (collidingTip == null)
        //{
        //    return;
        //}

        //Vector3 toolTipVector = collidingTip.position - collidingObjectPoint;
    }

    public void LockToolDueColonCollision()
    {
        lockToolDummyMesh.transform.position = toolLockingPosition;
        lockToolDummyMesh.transform.rotation = toolLockingRotation;
        lockToolDummyMesh.SetActive(true);

        toolMesh.ForEach(m => m.enabled = false);
    }
    public void UnlockToolDueColonCollision()
    {
        lockToolDummyMesh.SetActive(false);

        toolMesh.ForEach(m => m.enabled = true);
    }

    /// <summary>
    /// Create joints from the tool tip to the touched colon
    /// </summary>
    /// <param name="anchorPosition"></param>
    /// <param name="toolAttachPoint"></param>
    /// <param name="colonSphere"></param>
    public void AttachColonForPush(Vector3 anchorPosition, GameObject toolAttachPoint, GameObject colonSphere)
    {
        ToolColonSphereJoint newJointAnchor = Instantiate(jointObject).GetComponent<ToolColonSphereJoint>();
        newJointAnchor.transform.position = anchorPosition;
        newJointAnchor.initialPosition = anchorPosition;
        newJointAnchor.transform.parent = toolAttachPoint.transform;
        newJointAnchor.joint.connectedBody = colonSphere.GetComponent<Rigidbody>();
        createdJoints.Add(newJointAnchor);

        //Joint collideSphereJoint = (FixedJoint)toolAttachPoint.AddComponent(typeof(FixedJoint));
        //tipBody = toolAttachPoint.GetComponent<Rigidbody>();
        //tipBody.useGravity = false;
        //tipBody.isKinematic = true;
        //collideSphereJoint.connectedBody = colonSphere.GetComponent<Rigidbody>();
        //pushColonJoints.Add(collideSphereJoint);
    }

    public void DetachColonStopPush()
    {
        createdJoints.ForEach(j => Destroy(j));
        createdJoints.Clear();

        //pushColonJoints.ForEach(j => Destroy(j));
        //pushColonJoints.Clear();
        //Destroy(tipBody);
        //tipBody = null;
    }

    /// <summary>
    /// Get the 8 neighbor colon sphere of the target sphere
    /// </summary>
    /// <param name="centerSphere"></param>
    /// <returns></returns>
    public static List<Transform> GetNeighborColonSphere(Transform centerSphere)
    {
        List<Transform> neighbors = new List<Transform>();

        int colon = int.Parse(centerSphere.name[7].ToString());

        List<List<Transform>> colonLayers = new List<List<Transform>>();
        if (colon == 0)
        {
            colonLayers = globalOperators.instance.colon0Spheres;
        }
        else
        {
            colonLayers = globalOperators.instance.colon1Spheres;
        }

        int layerIndex = 0;
        int sphereIndex = 0;
        for (int i = 0; i < colonLayers.Count; i++)
        {
            if (colonLayers[i].Contains(centerSphere))
            {
                layerIndex = i;
                sphereIndex = colonLayers[i].IndexOf(centerSphere);
            }
        }

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) // Skip target sphere
                {
                    continue;
                }

                int neighborLayerIndex = layerIndex + i;
                if (neighborLayerIndex == -1 || neighborLayerIndex == colonLayers.Count) // Skip out of layer neighbor
                {
                    continue;
                }
                //if (neighborLayerIndex == -1)
                //{
                //    neighborLayerIndex = colonLayers.Count - 1;
                //}
                //else if (neighborLayerIndex == colonLayers.Count)
                //{
                //    neighborLayerIndex = 0;
                //}

                int neighborSphereIndex = sphereIndex + j;
                if (neighborSphereIndex == -1)
                {
                    neighborSphereIndex = colonLayers[neighborLayerIndex].Count - 1;
                }
                else if (neighborSphereIndex == colonLayers[neighborLayerIndex].Count)
                {
                    neighborSphereIndex = 0;
                }

                neighbors.Add(colonLayers[neighborLayerIndex][neighborSphereIndex]);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Is the lookedUpSphere a neighbor of the targetSphere
    /// </summary>
    /// <param name="lookedUpSphere"></param>
    /// <param name="targetSphere"></param>
    /// <returns></returns>
    public static bool IsNeighborOfColonSphere(Transform lookedUpSphere, Transform targetSphere)
    {
        int colon = int.Parse(targetSphere.name[7].ToString());

        List<List<Transform>> colonLayers = new List<List<Transform>>();
        if (colon == 0)
        {
            colonLayers = globalOperators.instance.colon0Spheres;
        }
        else
        {
            colonLayers = globalOperators.instance.colon1Spheres;
        }

        int layerIndex = 0;
        int sphereIndex = 0;
        for (int i = 0; i < colonLayers.Count; i++)
        {
            if (colonLayers[i].Contains(targetSphere))
            {
                layerIndex = i;
                sphereIndex = colonLayers[i].IndexOf(targetSphere);
            }
        }

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) // Skip target sphere
                {
                    continue;
                }

                int neighborLayerIndex = layerIndex + i;
                if (neighborLayerIndex == -1 || neighborLayerIndex == colonLayers.Count) // Skip out of layer neighbor
                {
                    continue;
                }

                int neighborSphereIndex = sphereIndex + j;
                if (neighborSphereIndex == -1)
                {
                    neighborSphereIndex = colonLayers[neighborLayerIndex].Count - 1;
                }
                else if (neighborSphereIndex == colonLayers[neighborLayerIndex].Count)
                {
                    neighborSphereIndex = 0;
                }

                if (colonLayers[neighborLayerIndex][neighborSphereIndex] == lookedUpSphere)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void LateUpdate()
    {
        //UpdateCollisionStatus();

        if (grabbing && !isLifting)
        {
            if (Vector3.Distance(transform.position, toolGrabStartPos) > forcepsMaxGrabDistance && hapticDevice.hapticManipulator != dummyManipulator)
            {
                PauseHapticsUpdateVirtualTool();
            }

            if (Vector3.Distance(hapticDevice.stylusPositionRaw, hapticsGrabStartRaw) < (forcepsMaxGrabDistance * 10) && hapticDevice.hapticManipulator == dummyManipulator)
            {
                ResumeHapticsUpdateVirtualTool();
            }
        }

        //if (lockToolDueColonContact)
        //{
        //    transform.position = toolLockingPosition;
        //}


    }

    /// <summary>
    /// "Freeze" the current controlled virtual tool in the space without updating its transform from the haptics input
    /// </summary>
    public void PauseHapticsUpdateVirtualTool()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        hapticDevice.hapticManipulator = dummyManipulator;
    }

    public void ResumeHapticsUpdateVirtualTool()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        hapticDevice.hapticManipulator = originalTool;
    }
}
