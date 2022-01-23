using HurricaneVR.Framework.Shared.HandPoser;
using UnityEngine;
using CEMSIM.Medications;

namespace CEMSIM
{
	namespace Tools
	{
		/// <summary>
        /// This class defines the state of the current gameobject.
        /// </summary>
		public class SyringeState: ToolBaseState
        {
			public MedicationMixture content = new MedicationMixture();
			float capacity; // capacity of the syringe
			float speed; // injection speed

			public SyringeState(float _capacity)
            {
				capacity = _capacity;
			}

			
		}

		public class SyringeInteractions : ToolBaseInteraction<SyringeState>
		{

			public GameObject plunger;
			public Vector3 plungerStartPos;
			public Vector3 plungerEndPos;

			[Tooltip("Capacity of the syringe (ml)")]
			public float syringeCapacity = 2; 
			public float speed = 16;
			public HVRHandPoser poser;
			public Transform balloon;
			public Vector3 balloonInflatedSize;

			public bool isGrabbed { get; set; } = false;
			public bool isPrimaryButtonPressed { get; set; } = false;



			public override SyringeState GetState()
            {
				return new SyringeState(syringeCapacity); // TODO: just a placeholder. Need to be implemented
            }


            public override void SetState(SyringeState curState)
            {
                // TODO: adjust thje state of the syringe gameobject based on the input state
            }


            // Update is called once per frame
            void Update()
			{
				if (isGrabbed)
				{
					if (isPrimaryButtonPressed)
					{
						CheckBalloonStatus();
						poser.PrimaryPose.Type = BlendType.BooleanParameter;
						plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerEndPos, Time.deltaTime * speed);
					}
					else
					{
						plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerStartPos, Time.deltaTime * speed);
					}
				}
				else
				{
					poser.PrimaryPose.Type = BlendType.Immediate;
				}
			}

			private void CheckBalloonStatus()
			{
				if (SyringeMountManager.isAttached)
				{
					balloon.localScale = Vector3.Lerp(balloon.localScale, balloonInflatedSize, Time.deltaTime * speed);
				}
			}
		}
	}
}