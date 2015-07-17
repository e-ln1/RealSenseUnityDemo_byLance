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
    public class TranslationTrigger : Trigger
    {
	
		public class TranslationTriggerAtt : TriggerAtt
		{
		}
		
		public bool Restart = false;
		
        public Vector3 Translation { get; set; }		 
		
		
		protected override string GetAttributeName()
		{
			return typeof(TranslationTriggerAtt).Name;
		}
		
		protected override string GetFriendlyName()
		{
			return "Translation Trigger";
		}
			
		

    }
}