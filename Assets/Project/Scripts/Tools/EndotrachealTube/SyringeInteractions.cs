using HurricaneVR.Framework.Shared.HandPoser;
using UnityEngine;
using CEMSIM.Medication;

namespace CEMSIM
{
	namespace Tools
	{
		/// <summary>
        /// This class defines the state of the current gameobject.
        /// </summary>
		public class SyringeState: ToolBaseState
        {
			public MedicineMixture content = new MedicineMixture();
			public float capacity { get; } // capacity of the syringe (ml)
			public float speed { get; set; } // injection speed (ml/sec)
			public float volume { get; set; } // volume of liquid/medication in syringe

			public SyringeState(float _capacity, float _speed, float _volume=0)
            {
				capacity = _capacity;
				speed = _speed;
				volume = _volume;
			}

			public float Injection(float _deltaVolume) {
				if (_deltaVolume <= volume)
					volume -= _deltaVolume;
				else
					volume = 0;
				return volume;
			}

			public float Refill(MedicineMixture medicationMixture)
            {
				float refillVolume = Mathf.Min(medicationMixture.volume, capacity - volume);

				content.Mix(medicationMixture.Split(refillVolume));

				volume += refillVolume;

				return refillVolume;
            }

			public float Refill(Medication.Medication.Drugs medicine, float _volume)
			{
				float refillVolume = Mathf.Min(_volume, capacity - volume);

				content.Mix(medicine, refillVolume);
				volume += refillVolume;
				return refillVolume;
			}
		}

		public class SyringeInteractions : ToolBaseInteraction<SyringeState>
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
			public MedicineMixture medication;

			private SyringeState state;

			public HVRHandPoser poser;
			public Transform balloon;
			public Vector3 balloonInflatedSize;

			public bool isGrabbed { get; set; } = false;
			public bool isRightPrimaryButtonPressed { get; set; } = false;
			public bool isRightSecondaryButtonPressed { get; set; } = false;

			public SyringeInteractions(){
				plungerLength = Vector3.Distance(plungerStartPos, plungerEndPos); // the length of the plunger measured by the model
				state = new SyringeState(syringeCapacity, injectSpeed, medication.volume);
				speed = injectSpeed / syringeCapacity * plungerLength;
			}



			public override SyringeState GetState()
            {
				return state;
            }


            public override void SetState(SyringeState curState)
            {
				// TODO: adjust thje state of the syringe gameobject based on the input state
				plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerEndPos, medication.volume/ plungerLength* syringeCapacity);

			}


			// Update is called once per frame
			void Update()
			{
				if (isGrabbed)
				{
					if (isRightPrimaryButtonPressed) // inject
					{
						CheckBalloonStatus();
						poser.PrimaryPose.Type = BlendType.BooleanParameter;
						plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerEndPos, Time.deltaTime * speed);

						state.Injection(Time.deltaTime * injectSpeed);
					}
					if (isRightSecondaryButtonPressed) // refill
					{
						plunger.transform.localPosition = Vector3.Lerp(plunger.transform.localPosition, plungerStartPos, Time.deltaTime * speed);
						state.Refill(Medication.Medication.Drugs.empty, Time.deltaTime * injectSpeed); // TODO: refill empty content?
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