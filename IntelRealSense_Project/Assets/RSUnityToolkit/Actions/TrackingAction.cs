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
/// Activate action: links the transformation of the associated Game Object to the real world tracked source
/// </summary>
public class TrackingAction : VirtualWorldBoxAction {
	
	#region Public Fields
	
	/// <summary>
	/// The smoothing factor.
	/// </summary>
	public float SmoothingFactor = 0;
	
	public SmoothingUtility.SmoothingTypes SmoothingType = SmoothingUtility.SmoothingTypes.Weighted;
		
	/// <summary>
	/// Position / Rotation constraints
	/// </summary>
	public Transformation3D  Constraints;
	
	/// <summary>
	/// Invert Positions / Rotations
	/// </summary>
	public Transformation3D  InvertTransform;	

	/// <summary>
	/// Effect Physics will use Unity’s MoveRotation and MovePosition to move the object.
	/// </summary>
	public bool EffectPhysics = false;
	
	/// <summary>
	/// SetDefaultsTo lets you switch in one click between 3 different tracking modes – hands, face and object tracking
	/// </summary>
	[BaseAction.ShowAtFirst]
	public Defaults SetDefaultsTo  = Defaults.HandTracking;
	
	#endregion

    #region Private Fields
	
	[SerializeField]
	[HideInInspector]
	private Defaults _lastDefaults = Defaults.HandTracking;

    private bool _actionTriggered = false;

	private SmoothingUtility _translationSmoothingUtility = new SmoothingUtility();
	private SmoothingUtility _rotationSmoothingUtility = new SmoothingUtility();
	
    #endregion
	
	#region Ctor
	
	/// <summary>
	/// Constructor
	/// </summary>
	public TrackingAction() : base()
	{
		Constraints = new Transformation3D();			
		InvertTransform = new Transformation3D();
	}
	
	#endregion

    #region Public methods
		
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
        return "This Action links the transformation of the associated Game Object to the real world tracked source";
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
		if (SetDefaultsTo == Defaults.HandTracking)
		{
	        switch (index)
	        {
	            case 0:
	                trigger.FriendlyName = "Start Event";
	                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<HandDetectedRule>() };
	                break;
	            case 1:
	                ((TrackTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<HandTrackingRule>() };
	                break;
	            case 2:
	                trigger.FriendlyName = "Stop Event";
	                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<HandLostRule>() };
	                break;
	        }
		}
		else if (SetDefaultsTo == Defaults.FaceTracking)
		{
			switch (index)
	        {
	            case 0:
	                trigger.FriendlyName = "Start Event";
	                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<FaceDetectedRule>() };
	                break;
	            case 1:
	                ((TrackTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<FaceTrackingRule>() };
	                break;
	            case 2:
	                trigger.FriendlyName = "Stop Event";
	                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<FaceLostRule>() };
	                break;
	        }
		}		
		else if (SetDefaultsTo == Defaults.ObjectTracking)
		{
			switch (index)
	        {
	            case 0:
	                trigger.FriendlyName = "Start Event";
	                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<ObjectDetectedRule>() };
	                break;
	            case 1:
	                ((TrackTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<ObjectTrackingRule>() };
	                break;
	            case 2:
	                trigger.FriendlyName = "Stop Event";
	                ((EventTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<ObjectLostRule>() };
	                break;
	        }
		}		
    } 
	
	/// <summary>
	/// Updates the inspector.
	/// </summary>
	public override void UpdateInspector()
	{
		if (_lastDefaults != SetDefaultsTo)
		{
			CleanSupportedTriggers();
	        SupportedTriggers = null;
	        InitializeSupportedTriggers();
			_lastDefaults = SetDefaultsTo;
		}		
	}
	
    #endregion

    #region Protected methods
	
	/// <summary>
	/// Sets the default triggers for that action.
	/// </summary>
    override protected void SetDefaultTriggers()
    {
        SupportedTriggers = new Trigger[3]{
			AddHiddenComponent<EventTrigger>(),
			AddHiddenComponent<TrackTrigger>(),
			AddHiddenComponent<EventTrigger>()};			
    }
	
    #endregion	

    #region Private Methods
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
    void Update()
    {
		updateVirtualWorldBoxCenter();
		
	   	ProcessAllTriggers();
				
        //Start Event
        if (!_actionTriggered && SupportedTriggers[0].Success)
        {
            _actionTriggered = true;
        }

        //Stop Event
        if (_actionTriggered && SupportedTriggers[2].Success)
        {
            _actionTriggered = false;
        }

        if (!_actionTriggered)
        {
            return;
        }

        TrackTrigger trgr = (TrackTrigger)SupportedTriggers[1];

        if (trgr.Success)
        {
            // Rotation:
            {
                Quaternion angles = trgr.RotationQuaternion;
                
				//Smoothing
				if (SmoothingFactor > 0)
				{	
					angles =  _rotationSmoothingUtility.ProcessSmoothing(SmoothingType, SmoothingFactor, angles);
				}
				
				Vector3 eulerAngles = angles.eulerAngles;
				
                if (!float.IsNaN(eulerAngles.x) && !float.IsNaN(eulerAngles.y) && !float.IsNaN(eulerAngles.z))
                {
                    eulerAngles.x = -eulerAngles.x;
                    eulerAngles.y = -eulerAngles.y;

                    //invert
                    if (InvertTransform.Rotation.X)
                    {
                        eulerAngles.x = -eulerAngles.x;
                    }

                    if (InvertTransform.Rotation.Y)
                    {
                        eulerAngles.y = -eulerAngles.y;
                    }

                    if (InvertTransform.Rotation.Z)
                    {
                        eulerAngles.z = -eulerAngles.z;
                    }

                    //flags
                    if (Constraints.Rotation.X)
                    {
                        eulerAngles.x = this.gameObject.transform.localRotation.eulerAngles.x;
                    }

                    if (Constraints.Rotation.Y)
                    {
                        eulerAngles.y = this.gameObject.transform.localRotation.eulerAngles.y;
                    }

                    if (Constraints.Rotation.Z)
                    {
                        eulerAngles.z = this.gameObject.transform.localRotation.eulerAngles.z;
                    }

                    if (EffectPhysics)
                    {
                        this.gameObject.GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(eulerAngles));
                    }
                    else
                    {
                        this.gameObject.transform.localRotation = Quaternion.Euler(eulerAngles);
                    }
                }
            }

            // Translation:
            {
                Vector3 vec = trgr.Position;				
				
                // Be sure we have valid values:
                if (VirtualWorldBoxDimensions.x <= 0)
                {
                    VirtualWorldBoxDimensions.x = 1;
                }

                if (VirtualWorldBoxDimensions.y <= 0)
                {
                    VirtualWorldBoxDimensions.y = 1;
                }

                if (VirtualWorldBoxDimensions.z <= 0)
                {
                    VirtualWorldBoxDimensions.z = 1;
                }

                // Get the relative position in the virtual box:
                float left = VirtualWorldBoxCenter.x - (VirtualWorldBoxDimensions.x) / 2;
                float top = VirtualWorldBoxCenter.y - (VirtualWorldBoxDimensions.y) / 2;
                float back = VirtualWorldBoxCenter.z - (VirtualWorldBoxDimensions.z) / 2;

                vec.x = (vec.x * VirtualWorldBoxDimensions.x);
                vec.y = (vec.y * VirtualWorldBoxDimensions.y);
                vec.z = (vec.z * VirtualWorldBoxDimensions.z);

                //invert
                if (InvertTransform.Position.X)
                {
                    vec.x = left + VirtualWorldBoxDimensions.x - vec.x;
                }
                else
                {
                    vec.x += left;
                }

                if (InvertTransform.Position.Y)
                {
                    vec.y = top + VirtualWorldBoxDimensions.y - vec.y;
                }
                else
                {
                    vec.y += top;
                }

                if (InvertTransform.Position.Z)
                {
                    vec.z = back + VirtualWorldBoxDimensions.z - vec.z;
                }
                else
                {
                    vec.z += back;
                }

                // Use the flags to indicate which axis are active
                if (Constraints.Position.X)
                {
                    vec.x = this.gameObject.transform.localPosition.x;
                }

                if (Constraints.Position.Y)
                {
                    vec.y = this.gameObject.transform.localPosition.y;
                }

                if (Constraints.Position.Z)
                {
                    vec.z = this.gameObject.transform.localPosition.z;
                }

                // smoothing:
				if (SmoothingFactor > 0)
				{
					vec = _translationSmoothingUtility.ProcessSmoothing(SmoothingType, SmoothingFactor, vec);
				}

                // Set new position
                if (EffectPhysics)
                {
                    this.gameObject.GetComponent<Rigidbody>().MovePosition(vec);
                }
                else
                {
                    this.gameObject.transform.localPosition = vec;
                }
            }
        }
    }

    /// <summary>
	/// Gets the average of the given list and add new number to the list
	/// </summary>
	/// <returns>
	/// The average and add new number.
	/// </returns>
	/// <param name='list'>
	/// List.
	/// </param>
	/// <param name='number'>
	/// Number.
	/// </param>
	private float GetAverageAndAddNewNumber(List<float> list, float number)
	{
		int size = list.Count;
		if ( size < 2 )
		{
			return number;
		}
		
		float sum = 0;
		
		for (int i =0; i < size-1; i++)
		{			
			sum+=list[i];
			list[i] = (float)list[i+1];			
		}
		sum+=list[size-1];
		list[size-1] = number;		
		
		return (sum/size);
    }
		
	
    #endregion
	
	#region Nested Types
	
	/// <summary>
	/// Default trackig modes that can be selected by SetDefaultsTo
	/// </summary>
	public enum Defaults
	{
		FaceTracking,
		HandTracking,
		ObjectTracking
	}
	
	
	#endregion
	
	#region Menu
	#if UNITY_EDITOR
	
	/// <summary>
	/// Adds the action to the RealSense Unity Toolkit menu.
	/// </summary>
	[UnityEditor.MenuItem ("RealSense Unity Toolkit/Add Action/Tracking")]
	static void AddThisAction () 
	{
		AddAction<TrackingAction>();
	} 
	
	#endif
	#endregion
	
}
 