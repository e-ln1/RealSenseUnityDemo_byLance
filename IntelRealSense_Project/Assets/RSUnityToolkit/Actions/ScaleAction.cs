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
/// Scale action: This action implements object scaling
/// </summary>
public class ScaleAction : BaseAction {

	#region Public Fields	
	
	/// <summary>
	/// The min/max/freeze scale constraints
	/// </summary>
	public ScaleConstraints Constraints = new ScaleConstraints();
	
	/// <summary>
	/// When enabled the object will continue to scale in a constant speed (set in the Continuous Scale Speed parameter) as long as the Scale Trigger is fired
	/// </summary>
	public bool ContinuousScale = false;
	
	/// <summary>
	/// Sets the continuous scale speed when enabled
	/// </summary>
	public float ContinuousScaleSpeed = 0f;
	
	/// <summary>
	/// The scale dumping factor. More means less scaling.
	/// </summary>
	public float ScaleDumpingFactor = 10f;	
	
	#endregion

	#region Private Fields
	
	private float _scale = 0f;	
	private float _lastScale = 0f;

	private bool _actionTriggered = false;
	
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
		return "This Action changes the scale of the game object";
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
			((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<GestureDetectedRule>() };
			((GestureDetectedRule)(trigger.Rules[0])).Gesture = MCTTypes.RSUnityToolkitGestures.Grab;
			break;
		case 1:
			((ScaleTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<TwoHandsInteractionRule>() };
			break;
		case 2:
			trigger.FriendlyName = "Stop Event";
			((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<GestureLostRule>() };
			((GestureLostRule)(trigger.Rules[0])).Gesture = MCTTypes.RSUnityToolkitGestures.Grab;
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
		AddHiddenComponent<ScaleTrigger>(),
		AddHiddenComponent<EventTrigger>()};				
	}	
	
	#endregion
	
	#region Private Methods
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update () 
	{	 
		ProcessAllTriggers();
		
		//Start Event
		if ( !_actionTriggered && SupportedTriggers[0].Success )
		{
			_actionTriggered = true;	
			
			((ScaleTrigger)SupportedTriggers[1]).Restart = true;
			return;
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
		
		ScaleTrigger trgr = (ScaleTrigger)SupportedTriggers[1];
		
		if( trgr.Success )
		{	
			_scale = trgr.Scale / ScaleDumpingFactor;

			//Keep it relative - for each axis, if rotation angle is 0 or if this is the first "frame" after a rotation angle is detected, save the data for next frame		
			if (!ContinuousScale)
			{
				if (_scale == 0 || _lastScale == 0)
				{
					_lastScale = _scale;
				}
				else
				{
					_scale = _scale - _lastScale;
				}
			}

			if (ContinuousScale && ContinuousScaleSpeed != 0)
			{
				_scale = Mathf.Sign(_scale) * ContinuousScaleSpeed;
			}

			Vector3 scaleVector = this.gameObject.transform.localScale;
			
			float _scaleX = _scale;
			float _scaleY = _scale;
			float _scaleZ = _scale;
			
			//Make sure we didn't pass the max / min Scale				
			if (Constraints.XScale.Max != 0) {
				if ((_scale + scaleVector.x) > Constraints.XScale.Max) {
					_scaleX = Constraints.XScale.Max - scaleVector.x;
				}
			}		
			if (Constraints.YScale.Max != 0) {
				if ((_scale + scaleVector.y) > Constraints.YScale.Max) {
					_scaleY = Constraints.YScale.Max - scaleVector.y;
				}
			}
			if (Constraints.ZScale.Max != 0) {
				if ((_scale + scaleVector.z) > Constraints.ZScale.Max) {
					_scaleZ = Constraints.ZScale.Max - scaleVector.z;
				}
			}
			
			if (Constraints.XScale.Min != 0) {
				if ((_scale + scaleVector.x) < Constraints.XScale.Min) {
					_scaleX = Constraints.XScale.Min - scaleVector.x;
				}
			}
			if (Constraints.YScale.Min != 0) {
				if ((_scale + scaleVector.y) < Constraints.YScale.Min) {
					_scaleY = Constraints.YScale.Min - scaleVector.y;
				}
			}
			if (Constraints.ZScale.Min != 0) {
				if ((_scale + scaleVector.z) < Constraints.ZScale.Min) {
					_scaleZ = Constraints.ZScale.Min - scaleVector.z;
				}
			}

			//Enable / Disable Axis			
			_scaleX = !Constraints.Freeze.X ? scaleVector.x + _scaleX : scaleVector.x;
			_scaleY = !Constraints.Freeze.Y ? scaleVector.y + _scaleY : scaleVector.y;
			_scaleZ = !Constraints.Freeze.Z ? scaleVector.z + _scaleZ : scaleVector.z;

			//Scale				

			this.gameObject.transform.localScale = new Vector3(_scaleX, _scaleY, _scaleZ);
			
						
			if (ScaleDumpingFactor==0)
			{
				Debug.LogError("ScaleDumpingFactor must not be zero. Changing it to 1");
				ScaleDumpingFactor = 1;
			}
			
			//Update last scaling
			_lastScale = trgr.Scale / ScaleDumpingFactor;
		}
	}
	
	#endregion
	
	#region Menu
	#if UNITY_EDITOR
	
	/// <summary>
	/// Adds the action to the RealSense Unity Toolkit menu.
	/// </summary>
	[UnityEditor.MenuItem ("RealSense Unity Toolkit/Add Action/Scale")]
	static void AddThisAction () 
	{
		AddAction<ScaleAction>();
	} 
	
	#endif
	#endregion
}




































