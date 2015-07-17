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
[CustomEditor(typeof(VirtualWorldBoxAction), true)]
public class VirtualWorldBoxActionEditor: BaseActionEditor
{
    #region Unity's override methods
	 
	private void OnSceneGUI()
    {
        UnityEditor.Handles.color = Color.black;
 		
		Transform parent = ((VirtualWorldBoxAction)target).gameObject.transform.parent;       
		
		if (parent != null)
		{ 
			UnityEditor.Handles.matrix = parent.localToWorldMatrix;
		}	
		    
        ((VirtualWorldBoxAction)target).VirtualWorldBoxDimensions =
        UnityEditor.Handles.ScaleHandle (((VirtualWorldBoxAction)target).VirtualWorldBoxDimensions,
                        ((VirtualWorldBoxAction)target).VirtualWorldBoxCenter ,
                        Quaternion.identity,
                        HandleUtility.GetHandleSize (((VirtualWorldBoxAction)target).VirtualWorldBoxCenter  ) * 0.5f);							     
		
		
		if (!Application.isPlaying)
		{
			((VirtualWorldBoxAction)target).updateVirtualWorldBoxCenter();		
		}
		 
    }


	#endregion
	
}
 