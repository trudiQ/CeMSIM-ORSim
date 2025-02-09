﻿using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class HVRPhysicsDrawer : MonoBehaviour
    {
        [Header("Settings")]
        public HVRAxis Axis;
        public Rigidbody ConnectedBody;
        public float Spring = 0;
        public float Damper = 10;


        [Tooltip("The resting position of the button")]
        public Vector3 StartPosition;

        [Tooltip("Furthest position the button can travel")]
        public Vector3 EndPosition;

        [Header("Debug")]
        public bool UpdateSpring;

        public Rigidbody Rigidbody { get; private set; }

        private Vector3 _axis;
        private ConfigurableJoint _joint;
        private ConfigurableJoint _limitJoint;


        protected virtual void Awake()
        {
            transform.localPosition = StartPosition;
            Rigidbody = GetComponent<Rigidbody>();
            _axis = Axis.GetVector();
            Rigidbody.useGravity = false;
            SetupJoint();
        }

        private void SetupJoint()
        {
            _joint = gameObject.AddComponent<ConfigurableJoint>();
            _joint.connectedBody = ConnectedBody;
            //_joint.anchor = StartPosition;
            _joint.autoConfigureConnectedAnchor = false;

            var worldStartPosition = StartPosition;
            if (transform.parent)
                worldStartPosition = transform.parent.TransformPoint(StartPosition);

            var worldEndPosition = EndPosition;
            if (transform.parent)
                worldEndPosition = transform.parent.TransformPoint(EndPosition);


            if (ConnectedBody)
            {
                _joint.connectedAnchor = ConnectedBody.transform.InverseTransformPoint(worldStartPosition);
            }
            else
            {
                _joint.connectedAnchor = worldStartPosition;
            }

            _joint.SetXDrive(Spring, Damper, Spring);

            _joint.LimitXMotion();
            _joint.LockYMotion();
            _joint.LockZMotion();
            _joint.LockAllAngularMotion();
            _joint.axis = _axis;
            _joint.secondaryAxis = _joint.axis.OrthogonalVector();
            _joint.SetLinearLimit(Vector3.Distance(StartPosition, EndPosition));

            _limitJoint = gameObject.AddComponent<ConfigurableJoint>();
            _limitJoint.connectedBody = ConnectedBody;
            //_limitJoint.anchor = EndPosition;
            _limitJoint.autoConfigureConnectedAnchor = false;

            if (ConnectedBody)
            {
                _limitJoint.connectedAnchor = ConnectedBody.transform.InverseTransformPoint(worldEndPosition);
            }
            else
            {
                _limitJoint.connectedAnchor = worldEndPosition;
            }

            _limitJoint.LockYMotion();
            _limitJoint.LockZMotion();
            _limitJoint.LockAllAngularMotion();
            _limitJoint.axis = _axis;
            _limitJoint.secondaryAxis = _joint.axis.OrthogonalVector();
            _limitJoint.LimitXMotion();
            _limitJoint.SetLinearLimit(Vector3.Distance(StartPosition, EndPosition));
        }
        private void FixedUpdate()
        {
        //    if (UpdateSpring)
        //    {
        //        _joint.SetXDrive(Spring, Damper, Spring);
        //        UpdateSpring = false;
        //    }

        //    var distance = (StartPosition - transform.localPosition).magnitude;

        //    if (!IsPressed && distance >= DownThreshold)
        //    {
        //        IsPressed = true;
        //        OnButtonDown();
        //    }
        //    else if (IsPressed && distance < ResetThreshold)
        //    {
        //        IsPressed = false;
        //        OnButtonUp();
        //    }
        }


        //protected virtual void OnButtonDown()
        //{
        //    if (SFXButtonDown)
        //    {
        //        SFXPlayer.Instance?.PlaySFX(SFXButtonDown, transform.position);
        //    }

        //    ButtonDown.Invoke(this);
        //}

        //protected virtual void OnButtonUp()
        //{
        //    if (SFXButtonUp)
        //    {
        //        SFXPlayer.Instance?.PlaySFX(SFXButtonUp, transform.position);
        //    }
        //    ButtonUp.Invoke(this);
        //}
    }

    //[Serializable]
    //public class HVRButtonEvent : UnityEvent<HVRPhysicsButton> { }
}