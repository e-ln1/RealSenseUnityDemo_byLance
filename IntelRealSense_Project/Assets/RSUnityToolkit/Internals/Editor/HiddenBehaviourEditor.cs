/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/
using UnityEngine;
using System.Collections; 
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using RSUnityToolkit;

[CanEditMultipleObjects]
[CustomEditor(typeof(HiddenBehaviour), true)]
public class HiddenBehaviourEditor: Editor
{
    #region Unity's override methods
 
	public void OnEnable()
	{
		target.hideFlags = HideFlags.HideInInspector;
	}
		
	#endregion
	
} 