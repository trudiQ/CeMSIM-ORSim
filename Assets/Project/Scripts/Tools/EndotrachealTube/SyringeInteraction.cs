using HurricaneVR.Framework.Shared.HandPoser;
using UnityEngine;
using CEMSIM.Medication;
using CEMSIM.GameLogic;
using CEMSIM.Network;
using static CEMSIM.Medication.Medication;

namespace CEMSIM
{
	namespace Tools
	{
		/// <summary>
        /// This class defines the state of the current gameobject.
        /// </summary>
		public class SyringeState: ToolBaseState
		{
            public MedicineMixture drugContent;
            public float capacity { get; } // capacity of the syringe (ml)

			public SyringeState(float _capacity, float _volume=0, Drugs drug=Drugs.empty)
            {
				capacity = _capacity;
				drugContent = new MedicineMixture(drug, _volume);
			}

			public float Injection(float _deltaVolume) {
					
				return drugContent.Split(_deltaVolume).volume;
			}

			public float Refill(MedicineMixture medicationMixture)
            {
				float refillVolume = Mathf.Min(medicationMixture.volume, capacity - drugContent.volume); // how much the syringe can fill

				drugContent.Mix(medicationMixture.Split(refillVolume));

				return refillVolume;
            }

			public float Refill(Medication.Medication.Drugs medicine, float _volume)
			{
				float refillVolume = Mathf.Min(_volume, capacity - drugContent.volume);

				drugContent.Mix(medicine, refillVolume);
				return refillVolume;
			}

            public override bool FromPacketPayload(Packet _remainderPacket)
            {
				bool isChange = drugContent.FromPacketPayload(_remainderPacket);
				if (drugContent.volume > capacity) // ideally, this if statement will not be executed
                {
					drugContent.Split(drugContent.volume - capacity);
				}
				return isChange;

			}

            public override byte[] ToPacketPayload()
            {
				return drugContent.ToPacketPayload();
            }
        }



		public class SyringeInteraction : ToolBaseInteraction
		{

			public GameObject plunger;
			public Vector3 plungerStartPos;
			public Vector3 plungerEndPos;
			private float plungerLength;

			[Tooltip("Capacity of the syringe (ml)")]
			public float syringeCapacity = 2;

			[Tooltip("Injection speed ml/sec")]
			public float injectSpeed = 16;

			private float speed; // # of moving distance of the plunger per second, speed = injectSpeed / syringeCapacity * (plungerEndPos-plungerStartPos)

			[Tooltip("Contained Medication")]
            public Drugs injectDrug=Drugs.empty;

			[Tooltip("Refill Component")]
			public Drugs refillDrug = Drugs.empty;


			[Tooltip("Medication Volume (ml)")]
			public float volume=2;

            public HVRHandPoser poser;
			public Transform balloon;
			public Vector3 balloonInflatedSize;

            public bool isGrabbed { get; set; } = false;
			public bool isRightPrimaryButtonPressed { get; set; } = false;
			public bool isRightSecondaryButtonPressed { get; set; } = false;

			private bool isButtonPressedBefore = false; // use to check whether the button is pressed in the last update, which is used to indicate whethere is a state change
			private bool isButtonPressedNow = false;

			public SyringeInteraction() : base(ToolType.syringe)
			{
				base.toolState = new SyringeState(syringeCapacity, volume, injectDrug); // initialize state 

				plungerLength = Vector3.Distance(plungerStartPos, plungerEndPos); // the length of the plunger measured by the model
				speed = injectSpeed / syringeCapacity * plungerLength;
			}


            public override void UpdateState()
            {
				float curVolume = ((SyringeState)toolState).drugContent.volume;
				Debug.Log($"syringe curVolume {curVolume}");
				// set plunger position
				plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerEndPos, curVolume / plungerLength* syringeCapacity);
				// TODO: may still have other states to be configured
			}

            // Update is called once per frame
            void Update()
			{
				if (isGrabbed)
				{
					isButtonPressedBefore = isButtonPressedNow;
					if (isRightPrimaryButtonPressed || isRightSecondaryButtonPressed) // button pressed
					{
						if (isRightPrimaryButtonPressed) // inject
						{
							CheckBalloonStatus();
							poser.PrimaryPose.Type = BlendType.BooleanParameter;
							plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerEndPos, Time.deltaTime * speed);

							((SyringeState)toolState).Injection(Time.deltaTime * injectSpeed);
							isButtonPressedNow = true;
						}
						else if (isRightSecondaryButtonPressed) // refill
						{
							CheckBalloonStatus();
							poser.PrimaryPose.Type = BlendType.BooleanParameter;
							plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerStartPos, Time.deltaTime * speed);
							((SyringeState)toolState).Refill(refillDrug, Time.deltaTime * injectSpeed);
							isButtonPressedNow = true;
						}
					}
                    else // no button pressed
                    {
						isButtonPressedNow = false;
					}


					if (isButtonPressedBefore != isButtonPressedNow) // one the transition time when the player presses the button or release the button
                    {
						StateUpdateEvent();
					}

				}
				else
				{
					poser.PrimaryPose.Type = BlendType.Immediate;

					isButtonPressedBefore = false;
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