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
/// Activate action: moves the game object in the direction of the trigger's translation data
/// </summary>
public class TranslateAction : VirtualWorldBoxAction {
	
	#region Public Fields

	/// <summary>
	/// The translation sensitivity.
	/// </summary>
	public Vector3 Sensitivity = new Vector3(200,200,200);	
	
	public float SmoothingFactor = 0;
	
	public SmoothingUtility.SmoothingTypes SmoothingType = SmoothingUtility.SmoothingTypes.Weighted;
	
	#endregion
	
	#region Private Fields
	
	private bool _actionTriggered = false;
	private SmoothingUtility _translationSmoothingUtility = new SmoothingUtility();
	
	#endregion

    #region Public Methods
	
	/// <summary>
	/// Determines whether this instance is support custom triggers.
	/// </summary>		
    public override bool IsSupportCustomTriggers()
	{
		return false;
	}
	
	/// <summary>
	/// Returns the actions's description for GUI purposes.
	/// </summary>
	/// <returns>
	/// The action description.
	/// </returns>
	public override string GetActionDescription()
	{
        return "This Action moves the game object in the direction of the trigger's translation data.";
	}

	/// <summary>
	/// Sets the default trigger values (for the triggers set in SetDefaultTriggers() )
	/// </summary>
	/// <param name='index'>
	/// Index of the trigger.
	/// </param>
	/// <param name='trigger'>
	/// A pointer to the trigger for which you can set the default rules.
	/// </param>
    public override void SetDefaultTriggerValues(int index, Trigger trigger)
    {
        switch (index)
        {
            case 0:
                trigger.FriendlyName = "Start Event";
                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<HandClosedRule>() };
                break;
            case 1:
                ((TranslationTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<HandMoveRule>() };
                break;
            case 2: 
                trigger.FriendlyName = "Stop Event";
                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<HandOpennedRule>() };
                break;
        }
    }

    #endregion

    #region Protected Methods
	
	/// <summary>
	/// Sets the default triggers for that action.
	/// </summary>
    protected override void SetDefaultTriggers()
	{	
		SupportedTriggers = new Trigger[3]{
			AddHiddenComponent<EventTrigger>(),
			AddHiddenComponent<TranslationTrigger>(),
			AddHiddenComponent<EventTrigger>()
		};			
	}

    #endregion

    #region Private Methods
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
    void Update () 
	{	 
		updateVirtualWorldBoxCenter();
		
		ProcessAllTriggers();
		
		//Start Event
		if ( !_actionTriggered && SupportedTriggers[0].Success )
		{
			_actionTriggered = true;	
			
			((TranslationTrigger)SupportedTriggers[1]).Restart = true;
		}
		
		//Stop Event
		if ( _actionTriggered && SupportedTriggers[2].Success )
		{
			_actionTriggered = false;					
		}
		
		if ( !_actionTriggered )
		{
			return;
		}
		
		TranslationTrigger trgr = (TranslationTrigger)SupportedTriggers[1];
		
		if( trgr.Success )
		{ 			
			Vector3 translate = trgr.Translation;
			translate.x = translate.x * Sensitivity.x;
			translate.y = translate.y * Sensitivity.y;
			translate.z = translate.z * Sensitivity.z;
			
			// smoothing:
			if (SmoothingFactor > 0)
			{
				translate = _translationSmoothingUtility.ProcessSmoothing(SmoothingType, SmoothingFactor, translate);
			}
			
			Vector3 vec = this.gameObject.transform.localPosition + translate;
			
			// Be sure we have valid values:				
			if ( VirtualWorldBoxDimensions.x < 0 )
			{
				VirtualWorldBoxDimensions.x = 0;
			}
						
			float left = VirtualWorldBoxCenter.x - (VirtualWorldBoxDimensions.x)/2;		
			float right = VirtualWorldBoxCenter.x + (VirtualWorldBoxDimensions.x)/2;				
			
			if (left > this.gameObject.transform.localPosition.x)
			{
				vec.x = left;
			}
			
			if (right < this.gameObject.transform.localPosition.x)
			{
				vec.x = right;
			}
						
			if ( VirtualWorldBoxDimensions.y < 0 )
			{
				VirtualWorldBoxDimensions.y = 0;
			}
			 			
			float top = VirtualWorldBoxCenter.y - (VirtualWorldBoxDimensions.y)/2;
			float bottom = VirtualWorldBoxCenter.y + (VirtualWorldBoxDimensions.y)/2;				
			
			if (top > this.gameObject.transform.localPosition.y)
			{
				vec.y = top;
			}
			
			if (bottom < this.gameObject.transform.localPosition.y)
			{
				vec.y = bottom;
			}
			
			if ( VirtualWorldBoxDimensions.z < 0 )
			{
				VirtualWorldBoxDimensions.z = 0;
			}
			
			float back = VirtualWorldBoxCenter.z - (VirtualWorldBoxDimensions.z)/2;
			float front = VirtualWorldBoxCenter.z + (VirtualWorldBoxDimensions.z)/2;
			
			if (back > this.gameObject.transform.localPosition.z)
			{
				vec.z = back;
			}
			
			if (front < this.gameObject.transform.localPosition.z)
			{
				vec.z = front;
			}
			
			if (this.gameObject.transform.parent != null) 
			{
				vec = this.gameObject.transform.parent.transform.TransformPoint(vec);
			}
			 
			this.gameObject.GetComponent<Rigidbody>().MovePosition(vec);			
		}
	}
	
    #endregion
	
	#region Menu
	#if UNITY_EDITOR
	
	/// <summary>
	/// Adds the action to the RealSense Unity Toolkit menu.
	/// </summary>
	[UnityEditor.MenuItem ("RealSense Unity Toolkit/Add Action/Translate")]
	static void AddThisAction () 
	{
		AddAction<TranslateAction>();
	} 
	
	#endif
	#endregion
	
}
 