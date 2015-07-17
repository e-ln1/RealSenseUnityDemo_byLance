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
    public class EventTrigger : Trigger
    {
		
		public class EventTriggerAtt : TriggerAtt
		{
		} 

        public string EventSource { get; set; }// from where it comes

        public object[] Params { get; set; }
		
		protected override string GetAttributeName()
		{
			return typeof(EventTriggerAtt).Name;
		}
		
		protected override string GetFriendlyName()
		{
			return "Event Source";
		}


    }
}