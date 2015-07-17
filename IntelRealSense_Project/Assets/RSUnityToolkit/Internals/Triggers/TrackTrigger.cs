/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/
using UnityEngine;
using System.Collections;

namespace RSUnityToolkit
{
	[AddComponentMenu("")]
    public class TrackTrigger : Trigger
    {
		
		public class TrackTriggerAtt : TriggerAtt
		{
		}
		
        public Quaternion RotationQuaternion { get; set; }

        public Vector3 Position { get; set; }
		
		protected override string GetAttributeName()
		{
			return typeof(TrackTriggerAtt).Name;
		}
		
		protected override string GetFriendlyName()
		{
			return "Tracking Source";
		}
			

    }
}