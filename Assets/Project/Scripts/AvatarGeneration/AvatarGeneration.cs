using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Shared.HandPoser;
using HurricaneVR.Framework.Core.Grabbers;
using RootMotion.FinalIK;

#if UNITY_EDITOR
using UnityEditor;

namespace CEMSIM
{
    public class AvatarGeneration : EditorWindow
    {
        Object avatarObject;
        Object VRIKRigObject;
        Object openHandPose;
        Object closedHandPose;
        Object mirrorSettings;
        string assetPath;
        GameObject avatar;
        GameObject VRIKRig;
        bool useIKPosing = false;

        [MenuItem("Window/Avatar Generation/Generate VRIK Rig")]
        
        /// <summary>
        /// Displays editor window
        /// </summary>
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(AvatarGeneration));
        }

        /// <summary>
        /// Creates the content for the editor window and displays relevant data entry fields
        /// </summary>
        void OnGUI()
        {
            GUILayout.Label("Avatar Generation", EditorStyles.boldLabel);

            // Object field for assigning VRIK Rig Base prefab
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("VRIK Rig Base");
            VRIKRigObject = EditorGUILayout.ObjectField(VRIKRigObject, typeof(Object), true);
            EditorGUILayout.EndHorizontal();

            // Object field for assigning Avatar prefab
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Avatar");
            avatarObject = EditorGUILayout.ObjectField(avatarObject, typeof(Object), true);
            EditorGUILayout.EndHorizontal();

            // Object field for desired open hand pose
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Default Open Hand Pose");
            openHandPose = EditorGUILayout.ObjectField(openHandPose, typeof(Object), true);
            EditorGUILayout.EndHorizontal();

            // Object field for desired closed hand pose
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Default Closed Hand Pose");
            closedHandPose = EditorGUILayout.ObjectField(closedHandPose, typeof(Object), true);
            EditorGUILayout.EndHorizontal();

            // Object field for desired Hand Mirror Settings
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hand Mirror Settings");
            mirrorSettings = EditorGUILayout.ObjectField(mirrorSettings, typeof(Object), true);
            EditorGUILayout.EndHorizontal();

            // Toggle field for determining whether to generate a rig using IK posing or not
            // For most all cases this should remain false
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use IK Hand Posing");
            useIKPosing = EditorGUILayout.Toggle(useIKPosing);
            EditorGUILayout.EndHorizontal();

            // Setup button that initializes the operation
            if(GUI.Button(new Rect(140, 150, 50, 25), "setup"))
            {
                GenerateAvatar();
            }
        }

        /// <summary>
        /// Calls all relevant methods to generate the VRIK Rig
        /// </summary>
        public void GenerateAvatar()
        {
            CacheReferences();
            ClearAnimatorController();
            SetupIKTargets();
            SetupVRIK();
            SetupAvatarHeightUtilities();
            if(useIKPosing)
            {
                SetupHVRHandsWithIKPosing();
            }
            else
            {
                SetUpHVRHandsWithDetachedHands();
            }
            AddForearmTwistRelaxers();
        }

        /// <summary>
        /// Caches GameObject references from relevant object fields
        /// </summary>
        public void CacheReferences()
        {
            VRIKRig = VRIKRigObject as GameObject;
            avatar = avatarObject as GameObject;
        }

        /// <summary>
        /// Clears out the animator controller if there is one attached to the avatar
        /// </summary>
        public void ClearAnimatorController()
        {
            if(avatar.GetComponent<Animator>() != null)
                avatar.GetComponent<Animator>().runtimeAnimatorController = null;
        }

        /// <summary>
        /// Assigns IK targets
        /// </summary>
        public void SetupIKTargets()
        {
            VRIK vrik = VRIKRig.GetComponentInChildren<VRIK>();
            vrik.solver.spine.headTarget = VRIKRig.transform.Find("PlayerController/CameraRig/FloorOffset/Camera/CameraIKTarget");
            vrik.solver.leftArm.target = VRIKRig.transform.Find("LeftHand/LeftIKTarget");
            vrik.solver.rightArm.target = VRIKRig.transform.Find("RightHand/RightIKTarget");
        }

        /// <summary>
        /// Sets up the VRIK componenet with references to relevant armature components
        /// </summary>
        public void SetupVRIK()
        {
            VRIK vrik = VRIKRig.GetComponentInChildren<VRIK>();

            // Cache a few paths to make it easier to access later on
            Transform pelvis = avatar.transform.Find("Armature/CC_Base_BoneRoot/CC_Base_Hip");
            Transform spine = avatar.transform.Find("Armature/CC_Base_BoneRoot/CC_Base_Hip/CC_Base_Waist/CC_Base_Spine01");
            Transform chest = spine.transform.Find("CC_Base_Spine02");
            Transform leftClavicle = chest.transform.Find("CC_Base_L_Clavicle");
            Transform rightClavicle = chest.transform.Find("CC_Base_R_Clavicle");
            Transform leftThigh = pelvis.transform.Find("CC_Base_Pelvis/CC_Base_L_Thigh");
            Transform rightThigh = pelvis.transform.Find("CC_Base_Pelvis/CC_Base_R_Thigh");

            // Assign armature parts to VRIK references
            vrik.references.pelvis = pelvis;
            vrik.references.spine = spine;
            vrik.references.chest = chest;
            vrik.references.neck = chest.transform.Find("CC_Base_NeckTwist01");
            vrik.references.head = chest.transform.Find("CC_Base_NeckTwist01/CC_Base_NeckTwist02/CC_Base_Head");
            vrik.references.leftShoulder = leftClavicle;
            vrik.references.leftUpperArm = leftClavicle.transform.Find("CC_Base_L_Upperarm");
            vrik.references.leftForearm = leftClavicle.transform.Find("CC_Base_L_Upperarm/CC_Base_L_Forearm");
            vrik.references.leftHand = leftClavicle.transform.Find("CC_Base_L_Upperarm/CC_Base_L_Forearm/CC_Base_L_Hand");
            vrik.references.rightShoulder = rightClavicle;
            vrik.references.rightUpperArm = rightClavicle.transform.Find("CC_Base_R_Upperarm");
            vrik.references.rightForearm = rightClavicle.transform.Find("CC_Base_R_Upperarm/CC_Base_R_Forearm");
            vrik.references.rightHand = rightClavicle.transform.Find("CC_Base_R_Upperarm/CC_Base_R_Forearm/CC_Base_R_Hand");
            vrik.references.leftThigh = leftThigh;
            vrik.references.leftCalf = leftThigh.transform.Find("CC_Base_L_Calf");
            vrik.references.leftFoot = leftThigh.transform.Find("CC_Base_L_Calf/CC_Base_L_Foot");
            vrik.references.leftToes = leftThigh.transform.Find("CC_Base_L_Calf/CC_Base_L_Foot/CC_Base_L_ToeBase");
            vrik.references.rightThigh = rightThigh;
            vrik.references.rightCalf = rightThigh.transform.Find("CC_Base_R_Calf");
            vrik.references.rightFoot = rightThigh.transform.Find("CC_Base_R_Calf/CC_Base_R_Foot");
            vrik.references.rightToes = rightThigh.transform.Find("CC_Base_R_Calf/CC_Base_R_Foot/CC_Base_R_ToeBase");
        }

        /// <summary>
        /// Adds CenterEye to the Head of the Armature.
        /// Sets up Height Utility and calculates avatar height.
        /// </summary>
        public void SetupAvatarHeightUtilities()
        {
            AvatarPrefabHeightUtility heightUtility = VRIKRig.GetComponentInChildren<AvatarPrefabHeightUtility>();
            heightUtility.avatarFloor = avatar.transform;
            heightUtility.avatarEyes = AddCenterEye().transform;
            heightUtility.CalculateHeight();
        }

        /// <summary>
        /// Adds a new GameObject and posiitions it to be used the CenterEye.
        /// </summary>
        public GameObject AddCenterEye()
        {
            GameObject centerEye = new GameObject("CenterEye");

            Transform head = avatar.transform.Find("Armature/CC_Base_BoneRoot/CC_Base_Hip/CC_Base_Waist/CC_Base_Spine01/CC_Base_Spine02/CC_Base_NeckTwist01/CC_Base_NeckTwist02/CC_Base_Head");
            centerEye.transform.parent = head;
            
            Vector3 centerEyePosition = new Vector3(.003f, .08f, .09f); 
            centerEye.transform.localPosition = centerEyePosition;

            return centerEye;
        }

        /// <summary>
        /// Set up Armature hands with HVR components
        /// </summary>
        public void SetupHVRHandsWithIKPosing()
        {
            // Cache some Armature references
            Transform spine = avatar.transform.Find("Armature/CC_Base_BoneRoot/CC_Base_Hip/CC_Base_Waist/CC_Base_Spine01/CC_Base_Spine02");
            Transform leftHand = spine.transform.Find("CC_Base_L_Clavicle/CC_Base_L_Upperarm/CC_Base_L_Forearm/CC_Base_L_Hand");
            Transform rightHand = spine.transform.Find("CC_Base_R_Clavicle/CC_Base_R_Upperarm/CC_Base_R_Forearm/CC_Base_R_Hand");

            // Add required componenets and cache them
            var leftHandAnimator = leftHand.gameObject.AddComponent<HVRHandAnimator>();
            var leftRotationLimiter = leftHand.gameObject.AddComponent<RotationLimitAngle>();
            var leftPhysicsPoser = leftHand.gameObject.AddComponent<HVRPhysicsPoser>();
            var leftHandPoser = leftHand.gameObject.AddComponent<HVRHandPoser>();
            var leftPosableHand = leftHand.gameObject.AddComponent<HVRPosableHand>();

            var rightHandAnimator = rightHand.gameObject.AddComponent<HVRHandAnimator>();
            var rightRotationLimiter = rightHand.gameObject.AddComponent<RotationLimitAngle>();
            var rightPhysicsPoser = rightHand.gameObject.AddComponent<HVRPhysicsPoser>();
            var rightHandPoser = rightHand.gameObject.AddComponent<HVRHandPoser>();
            var rightPosableHand = rightHand.gameObject.AddComponent<HVRPosableHand>();

            // Setup Posable Hands
            leftPosableHand.IsLeft = true;
            leftPosableHand.ThumbRoot = leftHand.Find("CC_Base_L_Thumb1");
            leftPosableHand.ThumbTip = leftHand.Find("CC_Base_L_Thumb1/CC_Base_L_Thumb2/CC_Base_L_Thumb3/CC_Base_L_Thumb3_end");
            leftPosableHand.IndexRoot = leftHand.Find("CC_Base_L_Index1");
            leftPosableHand.IndexTip = leftHand.Find("CC_Base_L_Index1/CC_Base_L_Index2/CC_Base_L_Index3/CC_Base_L_Index3_end");
            leftPosableHand.MiddleRoot = leftHand.Find("CC_Base_L_Mid1");
            leftPosableHand.MiddleTip = leftHand.Find("CC_Base_L_Mid1/CC_Base_L_Mid2/CC_Base_L_Mid3/CC_Base_L_Mid3_end");
            leftPosableHand.RingRoot = leftHand.Find("CC_Base_L_Ring1");
            leftPosableHand.RingTip = leftHand.Find("CC_Base_L_Ring1/CC_Base_L_Ring2/CC_Base_L_Ring3/CC_Base_L_Ring3_end");
            leftPosableHand.PinkyRoot = leftHand.Find("CC_Base_L_Pinky1");
            leftPosableHand.PinkyTip = leftHand.Find("CC_Base_L_Pinky1/CC_Base_L_Pinky2/CC_Base_L_Pinky3/CC_Base_L_Pinky3_end");
            leftPosableHand.MirrorSettings = mirrorSettings as HVRHandMirrorSettings;
            leftPosableHand.FingerSetup();

            rightPosableHand.IsLeft = false;
            rightPosableHand.ThumbRoot = rightHand.Find("CC_Base_R_Thumb1");
            rightPosableHand.ThumbTip = rightHand.Find("CC_Base_R_Thumb1/CC_Base_R_Thumb2/CC_Base_R_Thumb3/CC_Base_R_Thumb3_end");
            rightPosableHand.IndexRoot = rightHand.Find("CC_Base_R_Index1");
            rightPosableHand.IndexTip = rightHand.Find("CC_Base_R_Index1/CC_Base_R_Index2/CC_Base_R_Index3/CC_Base_R_Index3_end");
            rightPosableHand.MiddleRoot = rightHand.Find("CC_Base_R_Mid1");
            rightPosableHand.MiddleTip = rightHand.Find("CC_Base_R_Mid1/CC_Base_R_Mid2/CC_Base_R_Mid3/CC_Base_R_Mid3_end");
            rightPosableHand.RingRoot = rightHand.Find("CC_Base_R_Ring1");
            rightPosableHand.RingTip = rightHand.Find("CC_Base_R_Ring1/CC_Base_R_Ring2/CC_Base_R_Ring3/CC_Base_R_Ring3_end");
            rightPosableHand.PinkyRoot = rightHand.Find("CC_Base_R_Pinky1");
            rightPosableHand.PinkyTip = rightHand.Find("CC_Base_R_Pinky1/CC_Base_R_Pinky2/CC_Base_R_Pinky3/CC_Base_R_Pinky3_end");
            rightPosableHand.MirrorSettings = mirrorSettings as HVRHandMirrorSettings;
            rightPosableHand.FingerSetup();

            // Setup Physics Posers
            leftPhysicsPoser.Hand = leftPosableHand;
            leftPhysicsPoser.OpenPose = openHandPose as HVRHandPose;
            leftPhysicsPoser.ClosedPose = closedHandPose as HVRHandPose;
            GameObject leftPalm = new GameObject("PalmL");
            leftPalm.transform.parent = leftHand;
            leftPalm.transform.localPosition = new Vector3(-.0164f, .05f, .012f);
            leftPalm.transform.localRotation = Quaternion.Euler(0, 0, 180);
            leftPhysicsPoser.Palm = leftPalm.transform;
            leftPhysicsPoser._fingerIndices = new int[]{0, 3, 6, 9, 12};
            leftPhysicsPoser.Setup();

            rightPhysicsPoser.Hand = rightPosableHand;
            rightPhysicsPoser.OpenPose = openHandPose as HVRHandPose;
            rightPhysicsPoser.ClosedPose = closedHandPose as HVRHandPose;
            GameObject rightPalm = new GameObject("PalmR");
            rightPalm.transform.parent = rightHand;
            rightPalm.transform.localPosition = new Vector3(.0164f, .05f, .012f);
            rightPalm.transform.localRotation = Quaternion.Euler(0, 0, 0);
            rightPhysicsPoser.Palm = rightPalm.transform;
            rightPhysicsPoser.Setup();

            // Setup Hand Posers
            leftHandPoser.PrimaryPose = new HVRHandPoseBlend();
            leftHandPoser.PrimaryPose.Pose = openHandPose as HVRHandPose;
            leftHandPoser.Blends.Add(new HVRHandPoseBlend());
            leftHandPoser.Blends[0].Pose = closedHandPose as HVRHandPose;
            leftHandPoser.Blends[0].ThumbType = HVRFingerType.Close;
            leftHandPoser.Blends[0].ThumbStart = 0;
            leftHandPoser.Blends[0].IndexType = HVRFingerType.Close;
            leftHandPoser.Blends[0].IndexStart = 0;
            leftHandPoser.Blends[0].MiddleType = HVRFingerType.Close;
            leftHandPoser.Blends[0].MiddleStart = 0;
            leftHandPoser.Blends[0].RingType = HVRFingerType.Close;
            leftHandPoser.Blends[0].RingStart = 0;
            leftHandPoser.Blends[0].PinkyType = HVRFingerType.Close;
            leftHandPoser.Blends[0].PinkyStart = 0;

            rightHandPoser.PrimaryPose = new HVRHandPoseBlend();
            rightHandPoser.PrimaryPose.Pose = openHandPose as HVRHandPose;
            rightHandPoser.Blends.Add(new HVRHandPoseBlend());
            rightHandPoser.Blends[0].Pose = closedHandPose as HVRHandPose;
            rightHandPoser.Blends[0].ThumbType = HVRFingerType.Close;
            rightHandPoser.Blends[0].ThumbStart = 0;
            rightHandPoser.Blends[0].IndexType = HVRFingerType.Close;
            rightHandPoser.Blends[0].IndexStart = 0;
            rightHandPoser.Blends[0].MiddleType = HVRFingerType.Close;
            rightHandPoser.Blends[0].MiddleStart = 0;
            rightHandPoser.Blends[0].RingType = HVRFingerType.Close;
            rightHandPoser.Blends[0].RingStart = 0;
            rightHandPoser.Blends[0].PinkyType = HVRFingerType.Close;
            rightHandPoser.Blends[0].PinkyStart = 0;

            // Setup Rotation Limiters
            leftRotationLimiter.axis = new Vector3(0, 1, 0);
            leftRotationLimiter.limit = 90;
            leftRotationLimiter.twistLimit = 180;

            rightRotationLimiter.axis = new Vector3(0, 1, 0);
            rightRotationLimiter.limit = 90;
            rightRotationLimiter.twistLimit = 180;

            // Setup Hand Animators
            leftHandAnimator.PhysicsPoser = leftPhysicsPoser;
            leftHandAnimator.Hand = leftPosableHand;
            leftHandAnimator.DefaultPoser = leftHandPoser;
            leftHandAnimator.PosePosAndRot = false;

            rightHandAnimator.PhysicsPoser = rightPhysicsPoser;
            rightHandAnimator.Hand = rightPosableHand;
            rightHandAnimator.DefaultPoser = rightHandPoser;
            rightHandAnimator.PosePosAndRot = false;
            

            // Assign references in HVRJointHand components
            var leftHandGrabber = VRIKRig.transform.Find("LeftHand").GetComponent<HVRHandGrabber>();
            var rightHandGrabber = VRIKRig.transform.Find("RightHand").GetComponent<HVRHandGrabber>();

            leftHandGrabber.HandAnimator = leftHandAnimator;
            leftHandGrabber.PhysicsPoser = leftPhysicsPoser;

            rightHandGrabber.HandAnimator = rightHandAnimator;
            rightHandGrabber.PhysicsPoser = rightPhysicsPoser;
        }

        /// <summary>
        /// Sets up VRIK Rig using the Detached Hands
        public void SetUpHVRHandsWithDetachedHands()
        {
            // Cache reference to SynchronizeHand
            SyncronizeHand leftHandSync = VRIKRig.transform.Find("LeftHand").GetComponentInChildren<SyncronizeHand>();
            SyncronizeHand rightHandSync = VRIKRig.transform.Find("RightHand").GetComponentInChildren<SyncronizeHand>();

            // Cache some Armature references
            Transform spine = avatar.transform.Find("Armature/CC_Base_BoneRoot/CC_Base_Hip/CC_Base_Waist/CC_Base_Spine01/CC_Base_Spine02");
            Transform leftHand = spine.transform.Find("CC_Base_L_Clavicle/CC_Base_L_Upperarm/CC_Base_L_Forearm/CC_Base_L_Hand");
            Transform rightHand = spine.transform.Find("CC_Base_R_Clavicle/CC_Base_R_Upperarm/CC_Base_R_Forearm/CC_Base_R_Hand");

            // Create new lists for storing hand transforms for Armature hands
            List<Transform> leftAvatarHandTransforms = new List<Transform>();
            List<Transform> rightAvatarHandTransforms = new List<Transform>();

            // Add Armature hand transforms to list and then assign the list to Visual Transforms list for left hand synchronizer
            foreach(Transform handFeature in leftHand.GetComponentsInChildren<Transform>())
            {
                leftAvatarHandTransforms.Add(handFeature);
            }
            leftHandSync.visualTransforms = leftAvatarHandTransforms;
            
            // Add Armature hand transforms to list and then assign the list to Visual Transforms list for right hand synchronizer
            foreach(Transform handFeature in rightHand.GetComponentsInChildren<Transform>())
            {
                rightAvatarHandTransforms.Add(handFeature);
            }
            rightHandSync.visualTransforms = rightAvatarHandTransforms;

        }

        /// <summary>
        /// Adds forearm twist relaxers to both arms of Armature
        /// </summary>
        public void AddForearmTwistRelaxers()
        {
            // Cache some Armature references for easier access
            Transform spine = avatar.transform.Find("Armature/CC_Base_BoneRoot/CC_Base_Hip/CC_Base_Waist/CC_Base_Spine01/CC_Base_Spine02");
            Transform leftForearm = spine.transform.Find("CC_Base_L_Clavicle/CC_Base_L_Upperarm/CC_Base_L_Forearm/CC_Base_L_ForearmTwist01/CC_Base_L_ForearmTwist02");
            Transform rightForearm = spine.transform.Find("CC_Base_R_Clavicle/CC_Base_R_Upperarm/CC_Base_R_Forearm/CC_Base_R_ForearmTwist01/CC_Base_R_ForearmTwist02");

            // Add the left twist relaxer and set it up
            var leftTwistRelaxer = leftForearm.gameObject.AddComponent<TwistRelaxer>();
            leftTwistRelaxer.twistSolvers = new TwistSolver[] {new TwistSolver(), new TwistSolver()};
            leftTwistRelaxer.twistSolvers[0].transform = leftForearm;
            leftTwistRelaxer.twistSolvers[0].parent = spine.transform.Find("CC_Base_L_Clavicle/CC_Base_L_Upperarm/CC_Base_L_Forearm/CC_Base_L_ForearmTwist01/");
            leftTwistRelaxer.twistSolvers[0].children = new Transform[] {leftForearm.Find("CC_Base_L_ForearmTwist02_end")};
            leftTwistRelaxer.twistSolvers[0].weight = 1.0f;
            leftTwistRelaxer.twistSolvers[0].parentChildCrossfade = 0.5f;
            leftTwistRelaxer.twistSolvers[0].twistAngleOffset = 0.0f;
            leftTwistRelaxer.twistSolvers[1].transform = spine.transform.Find("CC_Base_L_Clavicle/CC_Base_L_Upperarm/CC_Base_L_Forearm/CC_Base_L_ForearmTwist01");
            leftTwistRelaxer.twistSolvers[1].parent = spine.transform.Find("CC_Base_L_Clavicle/CC_Base_L_Upperarm/CC_Base_L_Forearm");
            leftTwistRelaxer.twistSolvers[1].children = new Transform[] {spine.transform.Find("CC_Base_L_Clavicle/CC_Base_L_Upperarm/CC_Base_L_Forearm/CC_Base_L_Hand")};
            leftTwistRelaxer.twistSolvers[1].weight = 1.0f;
            leftTwistRelaxer.twistSolvers[1].parentChildCrossfade = 0.5f;
            leftTwistRelaxer.twistSolvers[1].twistAngleOffset = 0.0f;

            // Add the right twist relaxer and set it up
            var rightTwistRelaxer = rightForearm.gameObject.AddComponent<TwistRelaxer>();
            rightTwistRelaxer.twistSolvers = new TwistSolver[] {new TwistSolver(), new TwistSolver()};
            rightTwistRelaxer.twistSolvers[0].transform = rightForearm;
            rightTwistRelaxer.twistSolvers[0].parent = spine.transform.Find("CC_Base_R_Clavicle/CC_Base_R_Upperarm/CC_Base_R_Forearm/CC_Base_R_ForearmTwist01/");
            rightTwistRelaxer.twistSolvers[0].children = new Transform[] {rightForearm.Find("CC_Base_R_ForearmTwist02_end")};
            rightTwistRelaxer.twistSolvers[0].weight = 1.0f;
            rightTwistRelaxer.twistSolvers[0].parentChildCrossfade = 0.5f;
            rightTwistRelaxer.twistSolvers[0].twistAngleOffset = 0.0f;
            rightTwistRelaxer.twistSolvers[1].transform = spine.transform.Find("CC_Base_R_Clavicle/CC_Base_R_Upperarm/CC_Base_R_Forearm/CC_Base_R_ForearmTwist01");
            rightTwistRelaxer.twistSolvers[1].parent = spine.transform.Find("CC_Base_R_Clavicle/CC_Base_R_Upperarm/CC_Base_R_Forearm");
            rightTwistRelaxer.twistSolvers[1].children = new Transform[] {spine.transform.Find("CC_Base_R_Clavicle/CC_Base_R_Upperarm/CC_Base_R_Forearm/CC_Base_R_Hand")};
            rightTwistRelaxer.twistSolvers[1].weight = 1.0f;
            rightTwistRelaxer.twistSolvers[1].parentChildCrossfade = 0.5f;
            rightTwistRelaxer.twistSolvers[1].twistAngleOffset = 0.0f;
            
        }
    }
}
#endif