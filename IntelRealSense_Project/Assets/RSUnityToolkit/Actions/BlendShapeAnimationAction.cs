/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

//This Action Is Available to Unity 4.3+ Only
#if (UNITY_4_2_2 || UNITY_4_2_1 || UNITY_4_2_0 || UNITY_4_1_5 || UNITY_4_1_4 || UNITY_4_1_3 || UNITY_4_1_2 || UNITY_4_1_0 || UNITY_4_0_1 || UNITY_4_0_0 || UNITY_3_5_7 || UNITY_3_5_6 || UNITY_3_5_5 || UNITY_3_5_4 || UNITY_3_5_3 || UNITY_3_5_2 || UNITY_3_5_1 || UNITY_3_5_0 || UNITY_3_4_2 || UNITY_3_4_1 || UNITY_3_4_0)
#define PRE_UNITY_4_3
#endif

#if !PRE_UNITY_4_3


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RSUnityToolkit;

/// <summary>
/// Activate action: Activate the game objects on trigger
/// </summary>
public class BlendShapeAnimationAction : BaseAction
{
	#region Constants
	
	private const float RETURN_FROM_FAILURE_SMOOTH_FACTOR = 0.3f;
	private const float FAILURE_SMOOTH_FACTOR = 0.1f;
	private const float MIN_SMOOTH_BLEND_SHAPE = 2f;

	#endregion
	
	#region Public Fields

	[SerializeField, HideInInspector]
	public string[] _blendShapeNames;

	[SerializeField, HideInInspector]
	public string[] _blendShapeExpressions;

	#endregion

	#region Public Static Fields
	
	public static bool IsBlendShapeListVisible = false;
	
	#endregion

	#region Private Fields

	private SkinnedMeshRenderer _renderer = null;
	private bool[] _executeSmoothing;

	#endregion

	#region Public methods
	
	override public string GetActionDescription()
	{ 
		return "....";
	}

	override public void SetDefaultTriggerValues(int index, Trigger trigger)
	{		
		
		switch (index)
		{
		case 0:
			((AnimationTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<FacialExpressionAnimationRule>() };
			break;
//		case 1:
//			((TrackTrigger)trigger).Rules = new BaseRule[1] { AddHiddenComponent<FaceTrackingRule>() };
//			break;
		}
	}
	
		
    override public bool IsSupportCustomTriggers()
    {
        return false;
    }
	
	#endregion
	
	#region Protected methods
	
	override protected void SetDefaultTriggers()
	{	
//		SupportedTriggers = new Trigger[2]
//		{
//			AddHiddenComponent<AnimationTrigger>(),
//			AddHiddenComponent<TrackTrigger>()
//		};			

		SupportedTriggers = new Trigger[1]
		{
			AddHiddenComponent<AnimationTrigger>()
		};			
	}

	#endregion
	
	#region Private Methods

	void Awake()
	{
		_renderer = gameObject.GetComponent<SkinnedMeshRenderer>();

		_executeSmoothing = new bool[_blendShapeNames.Length];
	}

	void Update()
	{	 
		ProcessAllTriggers();
		
		AnimationTrigger animationTrigger = SupportedTriggers[0] as AnimationTrigger;
		if (animationTrigger == null)
		{
			return;
		}

		UpdateFace(animationTrigger);	
	}

	private void UpdateFace(AnimationTrigger animationTrigger)
	{
		for (int i = 0; i < _blendShapeExpressions.Length; i++)
		{
			if ((_blendShapeExpressions[i].Length > 0) && (animationTrigger.Animations.ContainsKey(_blendShapeExpressions[i])))
			{
				SetBlendShapeValue(animationTrigger.Animations[_blendShapeExpressions[i]], i, ref _executeSmoothing[i], animationTrigger.IsAnimationDataValid);
			}
		}
	}
	
	private void SetBlendShapeValue(float rawValue, int blendShapeIndex, ref bool executeSmoothing, bool isAnimationDataValid)
	{
		if (_renderer && blendShapeIndex >= 0)
		{
			float value = CalcBlendShapeValue(rawValue, blendShapeIndex, ref executeSmoothing, isAnimationDataValid);
			_renderer.SetBlendShapeWeight(blendShapeIndex, value);
			
			return;
		}
		
		return;
	}
	
	private float CalcBlendShapeValue(float rawValue, int blendShapeIndex, ref bool executeSmoothing, bool isAnimationDataValid)
	{
		float value = 0f;
		float currentValue = _renderer.GetBlendShapeWeight(blendShapeIndex);
		
		if (isAnimationDataValid)
		{
			if (executeSmoothing)
			{
				value = rawValue;
				if (Mathf.Abs(value - currentValue) > MIN_SMOOTH_BLEND_SHAPE)
				{
					value = Mathf.Lerp(currentValue, value, RETURN_FROM_FAILURE_SMOOTH_FACTOR);
				}
				else
				{
					executeSmoothing = false;
				}
			}
			else
			{
				value = rawValue;
			}
		}
		else
		{
			executeSmoothing = true;
			
			value = Mathf.Lerp(currentValue, 0, FAILURE_SMOOTH_FACTOR);
		}
		
		return value;
	}

	#endregion

	#region Menu
#if UNITY_EDITOR
	
	[UnityEditor.MenuItem ("RealSense Unity Toolkit/Add Action/Facial Animation Action")]
	static void AddThisAction () 
	{
		AddAction<BlendShapeAnimationAction>();
	} 
	
#endif
	#endregion
}
  
#endif