﻿using System;
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

	/// touching, grabing, holding, cutting tool actions (some of them could be true at the same time)
	private bool bTouching = false; //forceps, scissors
	private bool bGrabbing = false; //forceps
	private bool bHolding = false; //forceps
	private bool bCutting = false; //scissors
	public int holdSphereJointObjIdx = -1; // 0 or 1
	public int cutSphereJointObjIdx = -1; // 0 or 1
	private GameObject touching = null;			//!< Reference to the object currently touched
	private GameObject grabbing = null;			//!< Reference to the object currently grabbed
	private FixedJoint joint = null;            //!< The Unity physics joint created between the stylus and the object being grabbed.
	public enum PhysicsToggleStyle { none, onTouch, onGrab };
	public PhysicsToggleStyle physicsToggleStyle = PhysicsToggleStyle.none;   //!< Should the grabber script toggle the physics forces on the stylus? 
	public bool DisableUnityCollisionsWithTouchableObjects = true;

	// feedback force related 
	private int FXID; // ID of the effect of the haptic device
	private bool bTouching_pre = false;
	private int effectType = 3; // friction
	//private int ID = -1; // handle ID for the new effect
	private double[] pos = new double[] { 0.0d, 0.0d, 0.0d };
	private double[] dir = new double[] { 0.0d, 0.0d, 0.0d };
	[Range(0.0f, 1.0f)] private double Gain = 0.333f;
	[Range(0.0f, 1.0f)] private double Magnitude = 0.333f;
	[Range(1.0f, 1000.0f)] private double Frequency = 200.0f;

	// Actions: tool-tissue interaction relate (allis forcep and scissor)
	//		to be called by sim state-machine machanism in 'globalOperators.cs'
	public enum toolAction {idle, touching, grabbing, holding, cutting};
	public toolAction curAction = toolAction.idle; // idle - no action by default

	//! Automatically called for initialization
	void Start () 
	{
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

		/*if (hapticDevice == null)
		{

			HapticPlugin[] HPs = (HapticPlugin[])Object.FindObjectsOfType(typeof(HapticPlugin));
			foreach (HapticPlugin HP in HPs)
			{
				if (HP.hapticManipulator == this.gameObject)
				{
					hapticDevice = HP.gameObject;
				}
			}

		}

		if ( physicsToggleStyle != PhysicsToggleStyle.none)
			hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = false;

		if (DisableUnityCollisionsWithTouchableObjects)
			disableUnityCollisions();

		// feedback force effect
		ID = HapticPlugin.effects_assignEffect(hapticDevice.GetComponent<HapticPlugin>().configName);
		*/
	}

	void disableUnityCollisions()
	{
		GameObject[] touchableObjects;
		touchableObjects =  GameObject.FindGameObjectsWithTag("Touchable") as GameObject[];  //FIXME  Does this fail gracefully?

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
	bool parseSphereName(string sphereName, ref int[] sphereIDs)
	{
		// make sure the game object is a sphere of sphereJoint model
		string[] nameSplit = sphereName.Split('_');
		if (nameSplit.Length != 4 || nameSplit[0] != "sphere")
			return false;

		// parse the name
		for (int i = 0; i < sphereIDs.Length; i++)
			sphereIDs[i] = Int32.Parse(nameSplit[i+1]);

		return true;
	}
	
	//! Update is called once per frame
	void FixedUpdate () 
	{
		bool[] newButtonStatus = { hapticDevice.Buttons[0] == 1, hapticDevice.Buttons[1] == 1 } ;
		//bool newButtonStatus = hapticDevice.GetComponent<HapticPlugin>().Buttons [buttonID] == 1;
		bool[] oldButtonStatus = { buttonStatus[0], buttonStatus[1] };
		buttonStatus = new bool[] { newButtonStatus[0], newButtonStatus[1] };

		// Graspping: Forceps only, Button pressing check
		if (this.gameObject.name == "Forceps")
		{
			//left button for grasping/releasing
			if (oldButtonStatus[0] == false && newButtonStatus[0] == true) 
			{
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
			//hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = true;
			if (!grabbing && physicsToggleStyle == PhysicsToggleStyle.onGrab)
				hapticDevice.PhysicsManipulationEnabled = false;
			//hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = false;

			/*
			if (grabbing)
				hapticDevice.GetComponent<HapticPlugin>().shapesEnabled = false;
			else
				hapticDevice.GetComponent<HapticPlugin>().shapesEnabled = true;
				*/

		}

		// Touching: both forceps and scissors, no button, force feedback when touching an object
		if (FXID == -1)
		{
			FXID = HapticPlugin.effects_assignEffect(hapticDevice.configName);
		}
		if (FXID == -1) // Still broken?
		{
			Debug.LogError("Unable to assign Haptic effect.");
			return;
		}
		if (touching)
		{
			bTouching = true;
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
		}
		else
		{
			bTouching = false;
		}

		// If the on/off state has changed since last frame, send a Start or Stop event to OpenHaptics
		//Debug.Log(bTouching_pre + "," + bTouching);
		if (bTouching_pre != bTouching)
		{
			if (bTouching)
				HapticPlugin.effects_startEffect(hapticDevice.configName, FXID);
			else
				HapticPlugin.effects_stopEffect(hapticDevice.configName, FXID);
		}

		bTouching_pre = bTouching;

		// Cutting: scissors, left-button 
		if (this.gameObject.name == "Scissors")
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
						cutSphereJointObjIdx = sphereIDs[0];
						bCutting = true;
					}
					else
					{
						cutSphereJointObjIdx = -1;
						bCutting = false;
					}
				}
				else
				{
					cutSphereJointObjIdx = -1;
					bCutting = false;
				}
				Debug.Log(bCutting);
			}
			if (newButtonStatus[0] == false)
			{
				cutSphereJointObjIdx = -1;
				bCutting = false;
			}
		}

		// tool-specific action determination 
		//	make sure one action at a time
		if (this.gameObject.name == "Forceps")
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
			if (bCutting)
				curAction = toolAction.cutting;
			else // !bCutting
			{
				if (bTouching)
					curAction = toolAction.touching;
				else // !bTouching
					curAction = toolAction.idle;
			}
		}
	}

	private void hapticTouchEvent( bool isTouch )
	{
		if (physicsToggleStyle == PhysicsToggleStyle.onGrab)
		{
			if (isTouch)
				hapticDevice.PhysicsManipulationEnabled = true;
				//hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = true;
			else			
				return; // Don't release haptics while we're holding something.
		}
			
		if( physicsToggleStyle == PhysicsToggleStyle.onTouch )
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

		if( collisionInfo.rigidbody != null )
			hapticTouchEvent(true);

		if (thatBody == null)
			return;

		if (thatBody.isKinematic)
			return;
	
		touching = that;
	}
	void OnCollisionExit(Collision collisionInfo)
	{
		Collider other = collisionInfo.collider;
		//Debug.unityLogger.Log("onCollisionrExit : " + other.name);

		if( collisionInfo.rigidbody != null )
			hapticTouchEvent( false );

		if (touching == null)
			return; // Do nothing

		if (other == null ||
		    other.gameObject == null || other.gameObject.transform == null)
			return; // Other has no transform? Then we couldn't have grabbed it.

		if( touching == other.gameObject || other.gameObject.transform.IsChildOf(touching.transform))
		{
			touching = null;
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
		if (touchedObject == null) // Nothing to grab
			return;

		// Grabbing a grabber is bad news.
		if (touchedObject.tag =="Gripper")
			return;

		//Debug.Log( " Object : " + touchedObject.name + "  Tag : " + touchedObject.tag );

		grabbing = touchedObject;

		//Debug.logger.Log("Grabbing Object : " + grabbing.name);
		Rigidbody body = grabbing.GetComponent<Rigidbody>();

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

		joint = (FixedJoint)gameObject.AddComponent(typeof(FixedJoint));
		joint.connectedBody = body;
	}
	//! changes the layer of an object, and every child of that object.
	static void SetLayerRecursively(GameObject go, int layerNumber )
	{
		if( go == null ) return;
		foreach(Transform trans in go.GetComponentsInChildren<Transform>(true))
			trans.gameObject.layer = layerNumber;
	}

	//! Stop grabbing an obhject. (Like opening a claw.) Normally called when the button is released. 
	void release()
	{
		if( grabbing == null ) //Nothing to release
			return;


		Debug.Assert(joint != null);

		joint.connectedBody = null;
		Destroy(joint);



		grabbing = null;

		if (physicsToggleStyle != PhysicsToggleStyle.none)
			hapticDevice.PhysicsManipulationEnabled = false;
			//hapticDevice.GetComponent<HapticPlugin>().PhysicsManipulationEnabled = false;

	}

	//! Returns true if there is a current object. 
	public bool isGrabbing()
	{
		return (grabbing != null);
	}
}
