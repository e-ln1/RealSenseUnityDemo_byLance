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
/// Rotation action: This action implements object rotation
/// </summary>
public class RotationAction : BaseAction {

	#region Public Fields	
	
	/// <summary>
	/// The min/max rotation angles constraints
	/// </summary>
	public RotationConstraints Constraints = new RotationConstraints();
	
	/// <summary>
	/// When enabled the object will continue to rotate in a constant speed (set in the Continuous Rotation Speed parameter) as long as the Rotation Trigger is fired
	/// </summary>
	public bool ContinuousRotation = false;
	
	/// <summary>
	/// Sets the continuous rotation speed when enabled
	/// </summary>
    public float ContinuousRotationSpeed = 0f;
	
	/// <summary>
	/// The rotate dumping factor. More means less rotation.
	/// </summary>
	public float RotateDumpingFactor = 0.5f;	
	
		
	/// <summary>
	/// The smoothing factor.
	/// </summary>
	public float SmoothingFactor = 0;
	
	public SmoothingUtility.SmoothingTypes SmoothingType = SmoothingUtility.SmoothingTypes.Weighted;		
	
	/// <summary>
	/// Rotate around is setting the axis of rotation. When set to GameObject, A Ref Game Object needs to be set
	/// </summary>
	public RotateAroundE RotateAround = RotateAroundE.WorldAxis;
	
	/// <summary>
	/// A reference game object around which rotation will occure (if Rotate around is set to 'Gameobject')
	/// </summary>
	public GameObject RefGameObject;	
	
	#endregion

	#region Private Fields
	
	private float _xRot = 0f;
	private float _yRot = 0f;
	private float _zRot = 0f;

	private float _lastX = 0f;
	private float _lastY = 0f;
	private float _lastZ = 0f;

	private float _rotationAngleX = 0f;
	private float _rotationAngleY = 0f;
	private float _rotationAngleZ = 0f;

	private Vector3 upAxis, forwardAxis;
	
	private bool _actionTriggered = false;
	
	private SmoothingUtility _rotationSmoothingUtility = new SmoothingUtility();
	
	#endregion
	
	#region CTOR	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public RotationAction() : base()
	{
		Constraints.Freeze.X = false;
		Constraints.Freeze.Y = false;
		Constraints.Freeze.Z = false;
		
		Constraints.XRotation.MaxPos = 360;
		Constraints.YRotation.MaxPos = 360;
		Constraints.ZRotation.MaxPos = 360;
		
		Constraints.XRotation.MaxNeg = -360;
		Constraints.YRotation.MaxNeg = -360;
		Constraints.ZRotation.MaxNeg = -360;
	}	
	
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
		return "This Action rotates the object";
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
			((RotationTrigger)trigger).Rules = new BaseRule[1] {AddHiddenComponent<TwoHandsInteractionRule>() };
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
		AddHiddenComponent<RotationTrigger>(),
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
			
			((RotationTrigger)SupportedTriggers[1]).Restart = true;
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
		
		RotationTrigger trgr = (RotationTrigger)SupportedTriggers[1];

		if( trgr.Success )
		{				
			if (RotateDumpingFactor==0)
			{
				Debug.LogError("RotateDumpingFactor must not be zero. Changing it to 1");
				RotateDumpingFactor = 1;
			}
			
			_xRot = trgr.Pitch / RotateDumpingFactor;
			_yRot = -trgr.Yaw / RotateDumpingFactor;
			_zRot = -trgr.Roll / RotateDumpingFactor;
			
			//Keep it relative - for each axis, if rotation angle is 0 or if this is the first "frame" after a rotation angle is detected, save the data for next frame		
			if (!ContinuousRotation)
			{
				if (_xRot == 0 || _lastX==0)
				{
					_lastX = _xRot;
				}
				else
				{
					_xRot = _xRot - _lastX;
				}
				
				if (_yRot == 0 || _lastY==0)
				{
					_lastY = _yRot;
				}
				else
				{
					_yRot = _yRot - _lastY;
				}
				
				if (_zRot == 0 || _lastZ==0)
				{
					_lastZ = _zRot;
				}
				else
				{
					_zRot = _zRot - _lastZ;
				}
			}

			if (ContinuousRotation && ContinuousRotationSpeed != 0)
			{
				_xRot = Mathf.Sign(_xRot) * ContinuousRotationSpeed;
				_yRot = Mathf.Sign(_yRot) * ContinuousRotationSpeed;
				_zRot = Mathf.Sign(_zRot) * ContinuousRotationSpeed;
			}

			//Make sure we didn't pass the Maximum Rotation Angles
			_xRot = ((_xRot + _rotationAngleX) > Constraints.XRotation.MaxPos) ? (Constraints.XRotation.MaxPos - _rotationAngleX): _xRot;		
			_yRot = ((_yRot + _rotationAngleY) > Constraints.YRotation.MaxPos) ? (Constraints.YRotation.MaxPos - _rotationAngleY): _yRot;	
			_zRot = ((_zRot + _rotationAngleZ) > Constraints.ZRotation.MaxPos) ? (Constraints.ZRotation.MaxPos - _rotationAngleZ): _zRot;	

			_xRot = ((_xRot + _rotationAngleX) < Constraints.XRotation.MaxNeg) ? (Constraints.XRotation.MaxNeg - _rotationAngleX): _xRot;		
			_yRot = ((_yRot + _rotationAngleY) < Constraints.YRotation.MaxNeg) ? (Constraints.YRotation.MaxNeg - _rotationAngleY): _yRot;	
			_zRot = ((_zRot + _rotationAngleZ) < Constraints.ZRotation.MaxNeg) ? (Constraints.ZRotation.MaxNeg - _rotationAngleZ): _zRot;	

			//Enable / Disable Axis
			_xRot = Constraints.Freeze.X ? 0f : _xRot;
			_yRot = Constraints.Freeze.Y ? 0f : _yRot;
			_zRot = Constraints.Freeze.Z ? 0f : _zRot;
			
						
			if (SmoothingFactor > 0)
			{
				Vector3 vec = new Vector3(_xRot,_yRot,_zRot);
				
				vec = _rotationSmoothingUtility.ProcessSmoothing(SmoothingType, SmoothingFactor, vec);
				_xRot = vec.x;
				_yRot = vec.y;
				_zRot = vec.z;
			}
			
			//Set Axis && Rotate
			if (RotateAround == RotateAroundE.SelfAxis)
			{
				upAxis = Vector3.up;
				forwardAxis = Vector3.forward;

				this.gameObject.transform.Rotate(upAxis, _yRot, Space.Self);
				this.gameObject.transform.Rotate(forwardAxis, _zRot, Space.Self);	
				this.gameObject.transform.Rotate(forwardAxis, _xRot, Space.Self);
			}
			else if (RotateAround == RotateAroundE.WorldAxis)
			{
				upAxis = Vector3.up;
				forwardAxis = Vector3.forward;

				this.gameObject.transform.Rotate(upAxis, _yRot, Space.World);
				this.gameObject.transform.Rotate(forwardAxis, _zRot, Space.World);	
				this.gameObject.transform.Rotate(forwardAxis, _xRot, Space.World);
			}
			else if (RotateAround == RotateAroundE.GameObjectAxis)
			{
				upAxis = RefGameObject.transform.up;
				forwardAxis = RefGameObject.transform.forward;

				Vector3 point = this.transform.localPosition;
				this.gameObject.transform.RotateAround(point, upAxis, _yRot);
				this.gameObject.transform.RotateAround(point, forwardAxis, _zRot);	
				this.gameObject.transform.RotateAround(point, forwardAxis, _xRot);			
			}
			else if (RotateAround == RotateAroundE.Gameobject)
			{
				upAxis = RefGameObject.transform.up;
				forwardAxis = RefGameObject.transform.forward;

				Vector3 point = RefGameObject.transform.position;
				this.gameObject.transform.RotateAround(point, upAxis, _yRot);
				this.gameObject.transform.RotateAround(point, forwardAxis, _zRot);	
				this.gameObject.transform.RotateAround(point, forwardAxis, _xRot);
			}

			//Update new rotation angle
			_rotationAngleX = _rotationAngleX + _xRot;
			_rotationAngleY = _rotationAngleY + _yRot;
			_rotationAngleZ = _rotationAngleZ + _zRot;
				
			//Enable Full Rotations
			if (_rotationAngleX >= 360f) { _rotationAngleX = _rotationAngleX - 360f; };
			if (_rotationAngleY >= 360f) { _rotationAngleY = _rotationAngleY - 360f; };
			if (_rotationAngleZ >= 360f) { _rotationAngleZ = _rotationAngleZ - 360f; };

			//Enable Full Rotations
			if (_rotationAngleX <= -360f) { _rotationAngleX = _rotationAngleX + 360f; };
			if (_rotationAngleY <= -360f) { _rotationAngleY = _rotationAngleY + 360f; };
			if (_rotationAngleZ <= -360f) { _rotationAngleZ = _rotationAngleZ + 360f; };

			//Update last rotation
			_lastX = trgr.Pitch / RotateDumpingFactor;
			_lastY = -trgr.Yaw / RotateDumpingFactor;
			_lastZ = -trgr.Roll / RotateDumpingFactor;
		}		
	}
	
	#endregion
	
	#region Nested Types
	
	/// <summary>
	/// Enum that indicates around what we rotate the object 
	/// </summary>
	public enum RotateAroundE {
		SelfAxis,
		WorldAxis,
		GameObjectAxis,
		Gameobject,
	};
	
	#endregion
	
	#region Menu
	#if UNITY_EDITOR
	
	/// <summary>
	/// Adds the action to the RealSense Unity Toolkit menu.
	/// </summary>
	[UnityEditor.MenuItem ("RealSense Unity Toolkit/Add Action/Rotation")]
	static void AddThisAction () 
	{
		AddAction<RotationAction>();
	} 
	
	#endif
	#endregion
}
 