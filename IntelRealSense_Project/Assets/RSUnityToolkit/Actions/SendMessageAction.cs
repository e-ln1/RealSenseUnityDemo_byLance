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
/// Send Message Action: allow the registeration of other scripts to its' triggers
/// </summary>
public class SendMessageAction : BaseAction {
	
	#region Public Fields
	
	/// <summary>
	/// The name of the function that will be called using Unity’s “Send Message” capability
	/// </summary>
	public string FunctionName = "OnTrigger";
	
	#endregion
	
	#region Public Methods
	
	/// <summary>
	/// Returns the actions's description for GUI purposes.
	/// </summary>
	/// <returns>
	/// The action description.
	/// </returns>
	public override string GetActionDescription()
	{ 
		return "This Action allow the registeration of other scripts to its' triggers.";
	}
	
	#endregion
	
	#region Private Methods
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update () 
	{	 		
		ProcessAllTriggers();
			
		foreach (Trigger trgr in SupportedTriggers)
		{						
			if ( trgr.Success )
			{
				SendMessage(FunctionName, trgr);
			}
		}			
	}
	
	#endregion
	
	#region Menu
	#if UNITY_EDITOR
	
	/// <summary>
	/// Sets the default triggers for that action.
	/// </summary>
	[UnityEditor.MenuItem ("RealSense Unity Toolkit/Add Action/Send Message")]
	static void AddThisAction () 
	{
		AddAction<SendMessageAction>();
	} 
	
	#endif
	#endregion
}
  