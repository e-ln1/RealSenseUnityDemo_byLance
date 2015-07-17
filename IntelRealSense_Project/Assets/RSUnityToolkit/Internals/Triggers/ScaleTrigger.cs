/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/
using UnityEngine;
using System.Collections;

namespace RSUnityToolkit
{
	[AddComponentMenu("")]
    public class ScaleTrigger : Trigger
    {
		public class ScaleTriggerAtt : TriggerAtt
		{
		}
		
		public bool Restart = false;
		
		public float Scale { get; set; }
		
		protected override string GetAttributeName()
		{
			return typeof(ScaleTriggerAtt).Name;
		}
		
		protected override string GetFriendlyName()
		{
			return "Scale Trigger";
		}

/*        public override void SetDefaults(BaseAction actionOwner)
        {
            Rules = new BaseRule[1];
            Rules[0] = actionOwner.AddHiddenComponent<TwoHandsInteractionRule>();
        }*/
    }
}