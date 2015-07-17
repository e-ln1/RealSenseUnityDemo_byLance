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

namespace RSUnityToolkit
{
	[AddComponentMenu("")]
    public class AnimationTrigger : Trigger
    {		
		public class AnimationTriggerAtt : TriggerAtt
		{
		}

		#region Public Fields
		
		public Dictionary<string, float> Animations = new Dictionary<string, float>();
		public bool IsAnimationDataValid = false;

		#endregion

		protected override string GetAttributeName()
		{
			return typeof(AnimationTriggerAtt).Name;
		}
		
		protected override string GetFriendlyName()
		{
			return "Animation Trigger";
		}

    }
}