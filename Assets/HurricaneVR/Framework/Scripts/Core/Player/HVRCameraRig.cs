using System;
using System.Collections;
using HurricaneVR.Framework.ControllerInput;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HurricaneVR.Framework.Core.Player
{
    public class HVRCameraRig : MonoBehaviour
    {
        [Header("Required Transforms")]
        public Transform Camera;
        public Transform FloorOffset;
        
        [Header("Manual Camera Offsetting")]
        [Tooltip("Manually modify the camera height if needed")]
        public float CameraYOffset;

        
        [Header("Debugging")]
        public float StartingHeightSpeed = .05f;
        public float StartingHeight = 1.5f;
        public bool ForceStartingHeight;

        [Tooltip("If true, use up and down arrow to change YOffset to help with testing.")]
        public bool DebugKeyboardOffset;

        [Header("For Debugging Display only")]
        public float PlayerControllerYOffset = 0f;
        public float AdjustedCameraHeight;

        public bool IsMine { get; set; } = true;

        void Start()
        {
            Setup();
            if (ForceStartingHeight)
                StartCoroutine(EnforceStartingHeight());
        }

        private IEnumerator EnforceStartingHeight()
        {
            yield return null;

            while (Mathf.Abs(StartingHeight - AdjustedCameraHeight) > .05f)
            {
                var delta = StartingHeight - AdjustedCameraHeight;
                CameraYOffset += StartingHeightSpeed * Mathf.Sign(delta);
                yield return new WaitForFixedUpdate();
            }
        }

        void Update()
        {
            if (FloorOffset)
            {
                var pos = FloorOffset.transform.localPosition;
                var intendedOffset = CameraYOffset + PlayerControllerYOffset;
                var intendedCameraHeight = intendedOffset + Camera.localPosition.y;
                //if (intendedCameraHeight < 0)
                //{
                //    intendedOffset += (0 - intendedCameraHeight);
                //}
                FloorOffset.transform.localPosition = new Vector3(pos.x, intendedOffset, pos.z);
            }

            AdjustedCameraHeight = FloorOffset.transform.localPosition.y + Camera.localPosition.y;

            if (IsMine)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                if (DebugKeyboardOffset && UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                {
                    CameraYOffset += -.25f;
                }
                else if (DebugKeyboardOffset && UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
                {
                    CameraYOffset += .25f;
                }
#endif
            }
        }

        private void Setup()
        {
            var offset = CameraYOffset;

            if (FloorOffset)
            {
                var pos = FloorOffset.transform.localPosition;
                FloorOffset.transform.localPosition = new Vector3(pos.x, offset, pos.z);
            }
        }


      
    }
}