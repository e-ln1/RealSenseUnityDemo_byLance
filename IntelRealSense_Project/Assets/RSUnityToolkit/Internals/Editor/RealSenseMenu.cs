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

/// <summary>
/// Create the general RealSense menu.
/// </summary>
public class RealSenseMenu : EditorWindow {

	[MenuItem ("RealSense Unity Toolkit/Add To Scene/Sense Toolkit Manager")]
	static void AddSenseToolkitManager () {
		
		var senseManager = GameObject.FindObjectOfType(typeof(SenseToolkitManager));
        if (senseManager == null)
		{
			senseManager = (GameObject)PrefabUtility.InstantiatePrefab(UnityEditor.AssetDatabase.LoadMainAssetAtPath(SenseToolkitManager.AssetPrefabFolder + "SenseManager.prefab"));            
        }
		else 
		{
			Debug.LogWarning("Only one instance of Sense Manager Object can be added per scene");
		}
		
	}       
	
	[MenuItem ("RealSense Unity Toolkit/Add To Scene/Camera Image Plane")]
	static void AddCameraImagePlane () {
		PrefabUtility.InstantiatePrefab(UnityEditor.AssetDatabase.LoadMainAssetAtPath(SenseToolkitManager.AssetPrefabFolder + "Image.prefab"));            

	}      

	[MenuItem ("RealSense Unity Toolkit/Add To Scene/Sense AR Object")]
	static void AddSenseAR () {
		PrefabUtility.InstantiatePrefab(UnityEditor.AssetDatabase.LoadMainAssetAtPath(SenseToolkitManager.AssetPrefabFolder + "Sense AR.prefab"));            
	} 
}
	