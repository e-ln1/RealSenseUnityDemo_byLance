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
    public class RotationTrigger : Trigger
    {
		
		public class RotationTriggerAtt : TriggerAtt
		{
		}
		
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Roll { get; set; }
		
		public bool Restart = false;
		
		protected override string GetAttributeName()
		{
			return typeof(RotationTriggerAtt).Name;
		}
		
		protected override string GetFriendlyName()
		{
			return "Rotation Trigger";
		}

/*		 public override void SetDefaults(BaseAction actionOwner)
        {
            Rules = new BaseRule[1];
            Rules[0] = actionOwner.AddHiddenComponent<TwoHandsInteractionRule>();
        }*/
    }
}