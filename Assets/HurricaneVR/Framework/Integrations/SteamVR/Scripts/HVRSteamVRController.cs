using System;
using HurricaneVR.Framework.Shared;
using Valve.VR;

namespace HurricaneVR.Framework.SteamVR
{
    public class HVRSteamVRController : HVRController
    {
        protected override void Awake()
        {
            base.Awake();
            IsSteamVR = true;
        }

        protected override void UpdateInput()
        {
            SteamVR_Action_Skeleton skeleton;

            if (Side == HVRHandSide.Left)
            {
                JoystickAxis = SteamVR_Actions.hVR_LeftPrimaryAxis.axis;
                JoystickClicked = SteamVR_Actions.hVR_LeftPrimaryAxisClick.state;
                TrackpadAxis = SteamVR_Actions.hVR_LeftSecondaryAxis.axis;
                TrackPadClicked = SteamVR_Actions.hVR_LeftSecondaryAxisClick.state;

                Grip = SteamVR_Actions.hVR_LeftGrip.axis;
                Trigger = SteamVR_Actions.hVR_LeftTrigger.axis;

                PrimaryButton = SteamVR_Actions.hVR_LeftPrimary.state;
                SecondaryButton = SteamVR_Actions.hVR_LeftSecondary.state;

                PrimaryTouch = SteamVR_Actions.hVR_LeftPrimaryTouch.state;
                SecondaryTouch = SteamVR_Actions.hVR_LeftSecondaryTouch.state;

                TriggerNearTouch = SteamVR_Actions.hVR_LeftTriggerNearTouch.state;
                ThumbNearTouch = SteamVR_Actions.hVR_LeftThumbNearTouch.state;

                TriggerTouch = SteamVR_Actions.hVR_LeftTriggerTouch.state;
                ThumbTouch = SteamVR_Actions.hVR_LeftThumbTouch.state;
                JoystickTouch = SteamVR_Actions.hVR_LeftPrimaryAxisTouch.state;
                TrackPadTouch = SteamVR_Actions.hVR_LeftSecondaryAxisTouch.state;

                MenuButton = SteamVR_Actions.hVR_LeftMenu.state;


                GripButton = SteamVR_Actions.hVR_LeftGripButton.state;
                TriggerButton = SteamVR_Actions.hVR_LeftTriggerButton.state;

                skeleton = SteamVR_Actions.hVR_left_skeleton;

            }
            else
            {
                JoystickAxis = SteamVR_Actions.hVR_RightPrimaryAxis.axis;
                JoystickClicked = SteamVR_Actions.hVR_RightPrimaryAxisClick.state;
                TrackpadAxis = SteamVR_Actions.hVR_RightSecondaryAxis.axis;
                TrackPadClicked = SteamVR_Actions.hVR_RightSecondaryAxisClick.state;

                Grip = SteamVR_Actions.hVR_RightGrip.axis;
                Trigger = SteamVR_Actions.hVR_RightTrigger.axis;

                PrimaryButton = SteamVR_Actions.hVR_RightPrimary.state;
                SecondaryButton = SteamVR_Actions.hVR_RightSecondary.state;

                PrimaryTouch = SteamVR_Actions.hVR_RightPrimaryTouch.state;
                SecondaryTouch = SteamVR_Actions.hVR_RightSecondaryTouch.state;

                TriggerNearTouch = SteamVR_Actions.hVR_RightTriggerNearTouch.state;
                ThumbNearTouch = SteamVR_Actions.hVR_RightThumbNearTouch.state;

                TriggerTouch = SteamVR_Actions.hVR_RightTriggerTouch.state;
                ThumbTouch = SteamVR_Actions.hVR_RightThumbTouch.state;
                JoystickTouch = SteamVR_Actions.hVR_RightPrimaryAxisTouch.state;
                TrackPadTouch = SteamVR_Actions.hVR_RightSecondaryAxisTouch.state;

                MenuButton = SteamVR_Actions.hVR_RightMenu.state;


                GripButton = SteamVR_Actions.hVR_RightGripButton.state;
                TriggerButton = SteamVR_Actions.hVR_RightTriggerButton.state;

                skeleton = SteamVR_Actions.hVR_right_skeleton;
            }

            ThumbCurl = skeleton.thumbCurl;
            IndexCurl = skeleton.indexCurl;
            MiddleCurl = skeleton.middleCurl;
            RingCurl = skeleton.ringCurl;
            PinkyCurl = skeleton.pinkyCurl;

            FingerCurls[0] = ThumbCurl;
            FingerCurls[1] = IndexCurl;
            FingerCurls[2] = MiddleCurl;
            FingerCurls[3] = RingCurl;
            FingerCurls[4] = PinkyCurl;

            if (FingerSettings)
            {
                if (ControllerType != HVRControllerType.Knuckles)
                {

                    if (FingerSettings.OverrideThumb)
                    {
                        UpdateThumbWeight();
                    }

                    if (FingerSettings.OverrideTrigger)
                    {
                        UpdateIndexWeight();
                    }

                    if (FingerSettings.OverrideTriggerGrab)
                    {
                        UpdateGripFingers();
                    }

                }
                else
                {
                    if (FingerSettings.KnucklesOverrideThumb)
                    {
                        UpdateThumbWeight();
                    }

                    if (FingerSettings.KnucklesOverrideTrigger)
                    {
                        UpdateIndexWeight();
                    }
                }
            }
        }

        protected override void CheckButtonState(HVRButtons button, ref HVRButtonState buttonState)
        {
            ResetButton(ref buttonState);

            switch (button)
            {
                case HVRButtons.Grip:
                    buttonState.Value = Grip;

                    if (!InputMap.GripUseAnalog)
                    {
                        SetButtonState(button, ref buttonState, GripButton);
                    }

                    if (InputMap.GripUseAnalog || InputMap.GripUseEither && !buttonState.Active)
                    {
                        SetButtonState(button, ref buttonState, Grip >= InputMap.GripThreshold);
                    }


                    break;
                case HVRButtons.Trigger:
                    buttonState.Value = Trigger;
                    if (InputMap.TriggerUseAnalog)
                        SetButtonState(button, ref buttonState, Trigger >= InputMap.TriggerThreshold);
                    else
                        SetButtonState(button, ref buttonState, TriggerButton);
                    break;
                case HVRButtons.Primary:
                    SetButtonState(button, ref buttonState, PrimaryButton);
                    break;
                case HVRButtons.PrimaryTouch:
                    SetButtonState(button, ref buttonState, PrimaryTouch);
                    break;
                case HVRButtons.Secondary:
                    SetButtonState(button, ref buttonState, SecondaryButton);
                    break;
                case HVRButtons.SecondaryTouch:
                    SetButtonState(button, ref buttonState, SecondaryTouch);
                    break;
                case HVRButtons.Menu:
                    SetButtonState(button, ref buttonState, MenuButton);
                    break;
                case HVRButtons.JoystickButton:
                    SetButtonState(button, ref buttonState, JoystickClicked);
                    break;
                case HVRButtons.TrackPadButton:
                    SetButtonState(button, ref buttonState, TrackPadClicked);
                    break;
                case HVRButtons.JoystickTouch:
                    SetButtonState(button, ref buttonState, JoystickTouch);
                    break;
                case HVRButtons.TrackPadTouch:
                    SetButtonState(button, ref buttonState, TrackPadTouch);
                    break;
                case HVRButtons.TriggerTouch:
                    SetButtonState(button, ref buttonState, TriggerTouch);
                    break;
                case HVRButtons.ThumbTouch:
                    SetButtonState(button, ref buttonState, ThumbTouch);
                    break;
                case HVRButtons.TriggerNearTouch:
                    SetButtonState(button, ref buttonState, TriggerNearTouch);
                    break;
                case HVRButtons.ThumbNearTouch:
                    SetButtonState(button, ref buttonState, ThumbNearTouch);
                    break;
                case HVRButtons.TrackPadLeft:
                    SetButtonState(button, ref buttonState, TrackPadClicked && TrackpadAxis.x <= -InputMap.Axis2DLeftThreshold);
                    break;
                case HVRButtons.TrackPadRight:
                    SetButtonState(button, ref buttonState, TrackPadClicked && TrackpadAxis.x >= InputMap.Axis2DRighThreshold);
                    break;
                case HVRButtons.TrackPadUp:
                    SetButtonState(button, ref buttonState, TrackPadClicked && TrackpadAxis.y >= InputMap.Axis2DUpThreshold);
                    break;
                case HVRButtons.TrackPadDown:
                    SetButtonState(button, ref buttonState, TrackPadClicked && TrackpadAxis.y <= -InputMap.Axis2DDownThreshold);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }
    }
}
