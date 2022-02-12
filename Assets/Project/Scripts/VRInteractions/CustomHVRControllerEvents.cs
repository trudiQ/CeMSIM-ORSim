using UnityEngine;
using UnityEngine.Events;

namespace HurricaneVR.Framework.ControllerInput {
	public class CustomHVRControllerEvents : MonoBehaviour {

		public UnityEvent LeftGripActivated = new UnityEvent();
		public UnityEvent LeftGripDeactivated = new UnityEvent();
		public UnityEvent RightGripActivated = new UnityEvent();
		public UnityEvent RightGripDeactivated = new UnityEvent();

		public UnityEvent LeftTriggerActivated = new UnityEvent();
		public UnityEvent LeftTriggerDeactivated = new UnityEvent();
		public UnityEvent RightTriggerActivated = new UnityEvent();
		public UnityEvent RightTriggerDeactivated = new UnityEvent();

		public UnityEvent LeftPrimaryActivated = new UnityEvent();
		public UnityEvent LeftPrimaryDeactivated = new UnityEvent();
		public UnityEvent RightPrimaryActivated = new UnityEvent();
		public UnityEvent RightPrimaryDeactivated = new UnityEvent();

		public UnityEvent LeftSecondaryActivated = new UnityEvent();
		public UnityEvent LeftSecondaryDeactivated = new UnityEvent();
		public UnityEvent RightSecondaryActivated = new UnityEvent();
		public UnityEvent RightSecondaryDeactivated = new UnityEvent();

		public UnityEvent LeftMenuActivated = new UnityEvent();
		public UnityEvent LeftMenuDeactivated = new UnityEvent();
		public UnityEvent RightMenuActivated = new UnityEvent();
		public UnityEvent RightMenuDeactivated = new UnityEvent();

		public UnityEvent LeftPrimaryTouchActivated = new UnityEvent();
		public UnityEvent LeftPrimaryTouchDeactivated = new UnityEvent();
		public UnityEvent RightPrimaryTouchActivated = new UnityEvent();
		public UnityEvent RightPrimaryTouchDeactivated = new UnityEvent();

		public UnityEvent LeftSecondaryTouchActivated = new UnityEvent();
		public UnityEvent LeftSecondaryTouchDeactivated = new UnityEvent();
		public UnityEvent RightSecondaryTouchActivated = new UnityEvent();
		public UnityEvent RightSecondaryTouchDeactivated = new UnityEvent();

		public UnityEvent LeftJoystickActivated = new UnityEvent();
		public UnityEvent LeftJoystickDeactivated = new UnityEvent();
		public UnityEvent RightJoystickActivated = new UnityEvent();
		public UnityEvent RightJoystickDeactivated = new UnityEvent();

		public UnityEvent LeftTrackpadActivated = new UnityEvent();
		public UnityEvent LeftTrackpadDeactivated = new UnityEvent();
		public UnityEvent RightTrackpadActivated = new UnityEvent();
		public UnityEvent RightTrackpadDeactivated = new UnityEvent();

		public UnityEvent LeftJoystickTouchActivated = new UnityEvent();
		public UnityEvent LeftJoystickTouchDeactivated = new UnityEvent();
		public UnityEvent RightJoystickTouchActivated = new UnityEvent();
		public UnityEvent RightJoystickTouchDeactivated = new UnityEvent();

		public UnityEvent LeftTrackPadTouchActivated = new UnityEvent();
		public UnityEvent LeftTrackPadTouchDeactivated = new UnityEvent();
		public UnityEvent RightTrackPadTouchActivated = new UnityEvent();
		public UnityEvent RightTrackPadTouchDeactivated = new UnityEvent();

		public UnityEvent LeftTriggerTouchActivated = new UnityEvent();
		public UnityEvent LeftTriggerTouchDeactivated = new UnityEvent();
		public UnityEvent RightTriggerTouchActivated = new UnityEvent();
		public UnityEvent RightTriggerTouchDeactivated = new UnityEvent();

		public UnityEvent LeftThumbTouchActivated = new UnityEvent();
		public UnityEvent LeftThumbTouchDeactivated = new UnityEvent();
		public UnityEvent RightThumbTouchActivated = new UnityEvent();
		public UnityEvent RightThumbTouchDeactivated = new UnityEvent();

		public UnityEvent LeftTrackPadUpActivated = new UnityEvent();
		public UnityEvent LeftTrackPadUpDeactivated = new UnityEvent();
		public UnityEvent RightTrackPadUpActivated = new UnityEvent();
		public UnityEvent RightTrackPadUpDeactivated = new UnityEvent();

		public UnityEvent LeftTrackPadLeftActivated = new UnityEvent();
		public UnityEvent LeftTrackPadLeftDeactivated = new UnityEvent();
		public UnityEvent RightTrackPadLeftActivated = new UnityEvent();
		public UnityEvent RightTrackPadLeftDeactivated = new UnityEvent();

		public UnityEvent LeftTrackPadRightActivated = new UnityEvent();
		public UnityEvent LeftTrackPadRightDeactivated = new UnityEvent();
		public UnityEvent RightTrackPadRightActivated = new UnityEvent();
		public UnityEvent RightTrackPadRightDeactivated = new UnityEvent();

		public UnityEvent LeftTrackPadDownActivated = new UnityEvent();
		public UnityEvent LeftTrackPadDownDeactivated = new UnityEvent();
		public UnityEvent RightTrackPadDownActivated = new UnityEvent();
		public UnityEvent RightTrackPadDownDeactivated = new UnityEvent();

		private HVRGlobalInputs globalInputs;

		private void Start() {
			globalInputs = HVRGlobalInputs.Instance;
		}


		private void Update() {
			if (!globalInputs)
				return;

			if (globalInputs.LeftGripButtonState.JustActivated) {
				LeftGripActivated.Invoke();
			} else if (globalInputs.LeftGripButtonState.JustDeactivated) {
				LeftGripDeactivated.Invoke();
			}
			if (globalInputs.RightGripButtonState.JustActivated) {
				RightGripActivated.Invoke();
			} else if (globalInputs.RightGripButtonState.JustDeactivated) {
				RightGripDeactivated.Invoke();
			}
			if (globalInputs.LeftTriggerButtonState.JustActivated) {
				LeftTriggerActivated.Invoke();
			} else if (globalInputs.LeftTriggerButtonState.JustDeactivated) {
				LeftTriggerDeactivated.Invoke();
			}
			if (globalInputs.RightTriggerButtonState.JustActivated) {
				RightTriggerActivated.Invoke();
			} else if (globalInputs.RightTriggerButtonState.JustDeactivated) {
				RightTriggerDeactivated.Invoke();
			}
			if (globalInputs.LeftPrimaryButtonState.JustActivated) {
				LeftPrimaryActivated.Invoke();
			} else if (globalInputs.LeftPrimaryButtonState.JustDeactivated) {
				LeftPrimaryDeactivated.Invoke();
			}
			if (globalInputs.RightPrimaryButtonState.JustActivated) {
				RightPrimaryActivated.Invoke();
			} else if (globalInputs.RightPrimaryButtonState.JustDeactivated) {
				RightPrimaryDeactivated.Invoke();
			}
			if (globalInputs.LeftSecondaryButtonState.JustActivated) {
				LeftSecondaryActivated.Invoke();
			} else if (globalInputs.LeftSecondaryButtonState.JustDeactivated) {
				LeftSecondaryDeactivated.Invoke();
			}
			if (globalInputs.RightSecondaryButtonState.JustActivated) {
				RightSecondaryActivated.Invoke();
			} else if (globalInputs.RightSecondaryButtonState.JustDeactivated) {
				RightSecondaryDeactivated.Invoke();
			}
			if (globalInputs.LeftMenuButtonState.JustActivated) {
				LeftMenuActivated.Invoke();
			} else if (globalInputs.LeftMenuButtonState.JustDeactivated) {
				LeftMenuDeactivated.Invoke();
			}
			if (globalInputs.RightMenuButtonState.JustActivated) {
				RightMenuActivated.Invoke();
			} else if (globalInputs.RightMenuButtonState.JustDeactivated) {
				RightMenuDeactivated.Invoke();
			}
			if (globalInputs.LeftPrimaryTouchButtonState.JustActivated) {
				LeftPrimaryTouchActivated.Invoke();
			} else if (globalInputs.LeftPrimaryTouchButtonState.JustDeactivated) {
				LeftPrimaryTouchDeactivated.Invoke();
			}
			if (globalInputs.RightPrimaryTouchButtonState.JustActivated) {
				RightPrimaryTouchActivated.Invoke();
			} else if (globalInputs.RightPrimaryTouchButtonState.JustDeactivated) {
				RightPrimaryTouchDeactivated.Invoke();
			}
			if (globalInputs.LeftSecondaryTouchButtonState.JustActivated) {
				LeftSecondaryTouchActivated.Invoke();
			} else if (globalInputs.LeftSecondaryTouchButtonState.JustDeactivated) {
				LeftSecondaryTouchDeactivated.Invoke();
			}
			if (globalInputs.RightSecondaryTouchButtonState.JustActivated) {
				RightSecondaryTouchActivated.Invoke();
			} else if (globalInputs.RightSecondaryTouchButtonState.JustDeactivated) {
				RightSecondaryTouchDeactivated.Invoke();
			}
			if (globalInputs.LeftJoystickButtonState.JustActivated) {
				LeftJoystickActivated.Invoke();
			} else if (globalInputs.LeftJoystickButtonState.JustDeactivated) {
				LeftJoystickDeactivated.Invoke();
			}
			if (globalInputs.RightJoystickButtonState.JustActivated) {
				RightJoystickActivated.Invoke();
			} else if (globalInputs.RightJoystickButtonState.JustDeactivated) {
				RightJoystickDeactivated.Invoke();
			}
			if (globalInputs.LeftTrackpadButtonState.JustActivated) {
				LeftTrackpadActivated.Invoke();
			} else if (globalInputs.LeftTrackpadButtonState.JustDeactivated) {
				LeftTrackpadDeactivated.Invoke();
			}
			if (globalInputs.RightTrackpadButtonState.JustActivated) {
				RightTrackpadActivated.Invoke();
			} else if (globalInputs.RightTrackpadButtonState.JustDeactivated) {
				RightTrackpadDeactivated.Invoke();
			}
			if (globalInputs.LeftJoystickTouchState.JustActivated) {
				LeftJoystickTouchActivated.Invoke();
			} else if (globalInputs.LeftJoystickTouchState.JustDeactivated) {
				LeftJoystickTouchDeactivated.Invoke();
			}
			if (globalInputs.RightJoystickTouchState.JustActivated) {
				RightJoystickTouchActivated.Invoke();
			} else if (globalInputs.RightJoystickTouchState.JustDeactivated) {
				RightJoystickTouchDeactivated.Invoke();
			}
			if (globalInputs.LeftTrackPadTouchState.JustActivated) {
				LeftTrackPadTouchActivated.Invoke();
			} else if (globalInputs.LeftTrackPadTouchState.JustDeactivated) {
				LeftTrackPadTouchDeactivated.Invoke();
			}
			if (globalInputs.RightTrackPadTouchState.JustActivated) {
				RightTrackPadTouchActivated.Invoke();
			} else if (globalInputs.RightTrackPadTouchState.JustDeactivated) {
				RightTrackPadTouchDeactivated.Invoke();
			}
			if (globalInputs.LeftTriggerTouchState.JustActivated) {
				LeftTriggerTouchActivated.Invoke();
			} else if (globalInputs.LeftTriggerTouchState.JustDeactivated) {
				LeftTriggerTouchDeactivated.Invoke();
			}
			if (globalInputs.RightTriggerTouchState.JustActivated) {
				RightTriggerTouchActivated.Invoke();
			} else if (globalInputs.RightTriggerTouchState.JustDeactivated) {
				RightTriggerTouchDeactivated.Invoke();
			}
			if (globalInputs.LeftThumbTouchState.JustActivated) {
				LeftThumbTouchActivated.Invoke();
			} else if (globalInputs.LeftThumbTouchState.JustDeactivated) {
				LeftThumbTouchDeactivated.Invoke();
			}
			if (globalInputs.RightThumbTouchState.JustActivated) {
				RightThumbTouchActivated.Invoke();
			} else if (globalInputs.RightThumbTouchState.JustDeactivated) {
				RightThumbTouchDeactivated.Invoke();
			}
			if (globalInputs.LeftTrackPadUp.JustActivated) {
				LeftTrackPadUpActivated.Invoke();
			} else if (globalInputs.LeftTrackPadUp.JustDeactivated) {
				LeftTrackPadUpDeactivated.Invoke();
			}
			if (globalInputs.RightTrackPadUp.JustActivated) {
				RightTrackPadUpActivated.Invoke();
			} else if (globalInputs.RightTrackPadUp.JustDeactivated) {
				RightTrackPadUpDeactivated.Invoke();
			}
			if (globalInputs.LeftTrackPadLeft.JustActivated) {
				LeftTrackPadLeftActivated.Invoke();
			} else if (globalInputs.LeftTrackPadLeft.JustDeactivated) {
				LeftTrackPadLeftDeactivated.Invoke();
			}
			if (globalInputs.RightTrackPadLeft.JustActivated) {
				RightTrackPadLeftActivated.Invoke();
			} else if (globalInputs.RightTrackPadLeft.JustDeactivated) {
				RightTrackPadLeftDeactivated.Invoke();
			}
			if (globalInputs.LeftTrackPadRight.JustActivated) {
				LeftTrackPadRightActivated.Invoke();
			} else if (globalInputs.LeftTrackPadRight.JustDeactivated) {
				LeftTrackPadRightDeactivated.Invoke();
			}
			if (globalInputs.RightTrackPadRight.JustActivated) {
				RightTrackPadRightActivated.Invoke();
			} else if (globalInputs.RightTrackPadRight.JustDeactivated) {
				RightTrackPadRightDeactivated.Invoke();
			}
			if (globalInputs.LeftTrackPadDown.JustActivated) {
				LeftTrackPadDownActivated.Invoke();
			} else if (globalInputs.LeftTrackPadDown.JustDeactivated) {
				LeftTrackPadDownDeactivated.Invoke();
			}
			if (globalInputs.RightTrackPadDown.JustActivated) {
				RightTrackPadDownActivated.Invoke();
			} else if (globalInputs.RightTrackPadDown.JustDeactivated) {
				RightTrackPadDownDeactivated.Invoke();
			}

		}
	}
}