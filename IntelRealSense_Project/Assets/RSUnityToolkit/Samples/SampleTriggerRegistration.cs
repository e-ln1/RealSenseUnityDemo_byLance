/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/
using UnityEngine;
using System.Collections;
using RSUnityToolkit;

public class SampleTriggerRegistration : MonoBehaviour {
	
	/* This MonoBehaviour class shows how to use the Send Message Action.
	 * 
	 * In here you will find 2 methods. OnTrigger(Trigger trigger) and OnTrackTrigger(TrackTrigger trigger).
	 * In the Send Message Action you can specify the function name. you can use either of those functions. 
	 * Unity will send the Trigger as the attribute to the function and in here we will cast it (if needed) and display it in the OnGUI method.
	 *
	 */ 

	// Use this for initialization
	void Start () { 

	}
	
	private TrackTrigger _trigger;
			
	void OnTrigger(Trigger trigger)
	{
		if (trigger is TrackTrigger)
		{
			_trigger = (TrackTrigger)trigger;
		}
		else 
		{
			Debug.LogError("We assume we suppose to get TrackTrigger in this example. It seems like we didn't");
		}
	}
	
	void OnTrackTrigger(TrackTrigger trigger)
	{
		_trigger = trigger;
	}
	
	
	void OnGUI()
	{
		if (_trigger != null)
		{
			GUILayout.Label("Got Position Data = " + _trigger.Position);
			GUILayout.Label("Got Rotation Data = " + _trigger.RotationQuaternion);
		}
	}

}
