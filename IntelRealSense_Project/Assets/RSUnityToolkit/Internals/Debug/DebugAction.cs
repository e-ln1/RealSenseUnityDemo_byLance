/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RSUnityToolkit;

/// <summary>
/// Activate action: Activate the game objects on trigger
/// </summary>
public class DebugAction : BaseAction {
		
	public string DebugPrint = "";
		
	override protected void SetDefaultTriggers()
	{	
		SupportedTriggers = new Trigger[1]{
		AddHiddenComponent<EventTrigger>()};			
	}
	
	override public void SetDefaultTriggerValues(int index, Trigger trigger)
	{		
		
		switch (index)
		{
		case 0:
			((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<GestureDetectedRule>() };
			((GestureDetectedRule)(trigger.Rules[0])).Gesture = MCTTypes.RSUnityToolkitGestures.Grab;
			break;
		}
	}
	
	void Update () {	 
				
		ProcessAllTriggers();
		
		foreach (Trigger trgr in SupportedTriggers)
		{						
			if ( trgr.Success )
			{				
				Debug.Log("REALSENSE_DBG | " + System.DateTime.Now.ToLongTimeString() + " | " + trgr.Source + " Rule Fired " + DebugPrint);								
			}
		}		
	}
}