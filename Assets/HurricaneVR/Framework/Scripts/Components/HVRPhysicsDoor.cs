﻿using System.Collections;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    [RequireComponent(typeof(HVRRotationTracker))]
    [RequireComponent(typeof(Rigidbody))]
    public class HVRPhysicsDoor : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Local axis of rotation")]
        public HVRAxis Axis;
        public float Mass = 10f;
        public bool DisableGravity = true;
        public bool StartLocked;
        [Tooltip("Rigidbody to connect the joint to")]
        public Rigidbody ConnectedBody;

        [Header("Door Closing Settings")]
        public float CloseAngle = 5f;
        public float CloseOverTime = .25f;
        public float CloseDetectionTime = .5f;


        [Header("Handle")]
        public bool HandleRequiresRotation;
        public float HandleThreshold = 45f;
        public HVRRotationTracker HandleRotationTracker;
        public HVRGrabbable HandleGrabbable;
        public HVRPhysicsDial DoorKnob;


        [Header("Joint Limits")]
        public bool LimitRotation = true;

        [Tooltip("Minimum Angle about the axis of rotation")]
        public float MinAngle;

        [Tooltip("Maximum rotation about the axis of rotation")]
        public float MaxAngle;

        [Header("Joint Settings")]

        [Tooltip("Angular Damper when the dial is not grabbed")]
        public float Damper = 10;

        public float Spring;

        [Header("Editor")]
        [SerializeField]
        protected Quaternion JointStartRotation;

        [Header("Debugging")]
        public float TargetAngularVelocity = 0f;
        public bool DoorLatched;
        public bool DoorClosed;
        public bool PreviousDoorLatched;
        public bool PreviousClosed;
        public bool VerboseLogging;
        public bool Locked;

        public Rigidbody Rigidbody { get; private set; }

        public HVRRotationTracker Tracker { get; private set; }

        protected ConfigurableJoint Joint { get; set; }

        private Quaternion _startRotation;
        private bool _doorClosing;
        private float _detectionTimer;

        public virtual void Start()
        {
            Rigidbody = this.GetRigidbody();
            Rigidbody.mass = Mass;
            Rigidbody.useGravity = !DisableGravity;

            Tracker = GetComponent<HVRRotationTracker>();

            if (MinAngle > 0)
            {
                MinAngle *= -1;
            }

            if (MaxAngle < 0)
            {
                MaxAngle *= -1;
            }

            MinAngle = Mathf.Clamp(MinAngle, MinAngle, 0);
            MaxAngle = Mathf.Clamp(MaxAngle, 0, MaxAngle);

            if (HandleRequiresRotation)
            {
                if (!HandleRotationTracker)
                {
                    Debug.LogError("HandleRotationTracker not assigned.");
                }

                DoorLatched = true;
            }

            DoorClosed = true;
            PreviousDoorLatched = DoorLatched;
            PreviousClosed = DoorClosed;

            SetupJoint();
            LockDoorJoint();

            _startRotation = transform.localRotation;

            if (StartLocked)
            {
                DoorKnob?.SetLimits(0f, 0f);
            }
        }

        protected virtual void Update()
        {
            Joint.targetAngularVelocity = new Vector3(TargetAngularVelocity, 0f, 0f);

            if (_doorClosing)
                return;

            if (Tracker.UnsignedAngle < CloseAngle)
            {
                _detectionTimer += Time.deltaTime;
            }
            else if (Tracker.UnsignedAngle >= CloseAngle)
            {
                _detectionTimer = 0f;
                DoorClosed = false;
            }

            //if (HandleGrabbable && HandleGrabbable.IsBeingHeld)
            //{
            //    //_detectionTimer = 0f;
            //}

            if (_detectionTimer > CloseDetectionTime)
            {
                DoorClosed = true;
            }

            if (!PreviousClosed && DoorClosed)
            {
                OnDoorClosed();
            }
            else if (PreviousClosed && !DoorClosed)
            {
                OnDoorOpened();
            }

            if (HandleRequiresRotation)
            {
                if (HandleRotationTracker.UnsignedAngle >= HandleThreshold)
                {
                    DoorLatched = false;
                }
                else if (HandleRotationTracker.UnsignedAngle < HandleThreshold && DoorClosed)
                {
                    DoorLatched = true;
                }

                if (!Locked)
                {
                    if (PreviousDoorLatched && !DoorLatched)
                    {
                        OnDoorUnLatched();
                    }
                    else if (!PreviousDoorLatched && DoorLatched)
                    {
                        OnDoorLatched();
                    }
                }
            }

            PreviousDoorLatched = DoorLatched;
            PreviousClosed = DoorClosed;
        }

        protected virtual void OnDoorUnLatched()
        {
            if (VerboseLogging)
                Debug.Log($"OnDoorUnLatched");
            UnlockDoorJoint();
        }

        protected virtual void OnDoorLatched()
        {
            if (VerboseLogging)
                Debug.Log($"OnDoorLatched");
            LockDoorJoint();
        }

        protected virtual void OnDoorClosed()
        {
            if (VerboseLogging)
                Debug.Log($"OnDoorClosed");

            StartCoroutine(DoorCloseRoutine());
        }

        protected virtual void OnDoorOpened()
        {
            if (VerboseLogging)
                Debug.Log($"OnDoorOpened");
        }

        protected virtual void LockDoorJoint()
        {
            if (!LimitRotation)
                return;

            Joint.SetAngularXHighLimit(0);
            Joint.SetAngularXLowLimit(0);
        }

        protected virtual void UnlockDoorJoint()
        {
            Joint.SetAngularXHighLimit(-MinAngle);
            Joint.SetAngularXLowLimit(-MaxAngle);
        }

        public void Lock()
        {
            Locked = true;
            LockDoorJoint();
        }

        public void Unlock()
        {
            Locked = false;
            UnlockDoorJoint();
            DoorKnob?.ResetLimits();
        }

        protected IEnumerator DoorCloseRoutine()
        {
            _doorClosing = true;
            var startRotation = transform.localRotation;
            var elapsed = 0f;
            while (elapsed < CloseOverTime)
            {
                transform.localRotation = Quaternion.Lerp(startRotation, _startRotation, elapsed / CloseOverTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localRotation = _startRotation;
            _doorClosing = false;
        }

        private void SetupJoint()
        {
            var currentRotation = transform.localRotation;
            transform.localRotation = JointStartRotation;
            Joint = gameObject.AddComponent<ConfigurableJoint>();
            Joint.connectedBody = ConnectedBody;
            Joint.LockLinearMotion();
            Joint.LockAngularYMotion();
            Joint.LockAngularZMotion();

            Joint.axis = Axis.GetVector();

            if (LimitRotation)
            {
                Joint.LimitAngularXMotion();
                Joint.SetAngularXHighLimit(-MinAngle);
                Joint.SetAngularXLowLimit(-MaxAngle);
            }
            else
            {
                Joint.angularXMotion = ConfigurableJointMotion.Free;
            }

            Joint.secondaryAxis = Joint.axis.OrthogonalVector();
            Joint.SetAngularXDrive(Spring, Damper, 10000f);

            transform.localRotation = currentRotation;
        }
    }
}