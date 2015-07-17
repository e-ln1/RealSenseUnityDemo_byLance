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
[CustomEditor(typeof(SenseToolkitManager), true)]
public class SenseToolkitManagerEditor: Editor
{
	private bool _speechFoldoutOpen;
	
    #region Unity's override methods
 
	// This function is called every editor gui update. In here we are diong our magic to show all to the user in a nice way.
	public override void OnInspectorGUI()
    { 	
		SenseToolkitManager senseToolkitManager = (SenseToolkitManager)target;
		
		DrawDefaultInspector();
		
		if (!Application.isPlaying)
		{
			senseToolkitManager.SpeechManager.InitalizeSpeech();
		}
			
		_speechFoldoutOpen = GUIUtils.Foldout(_speechFoldoutOpen, new GUIContent("Speech Settings"));

		if ( _speechFoldoutOpen )
		{
			EditorGUI.indentLevel++;
			
			senseToolkitManager.SpeechManager.ActiveSource = EditorGUILayout.Popup("Available Sources: ", senseToolkitManager.SpeechManager.ActiveSource, senseToolkitManager.SpeechManager.AvailableSources);
			
			senseToolkitManager.SpeechManager.ActiveModule = EditorGUILayout.Popup("Available Modules: ", senseToolkitManager.SpeechManager.ActiveModule, senseToolkitManager.SpeechManager.AvailableModules);
			
			senseToolkitManager.SpeechManager.ActiveLanguage = EditorGUILayout.Popup("Available Languages: ", senseToolkitManager.SpeechManager.ActiveLanguage, senseToolkitManager.SpeechManager.AvailableLanguages);

			EditorGUI.indentLevel--;
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty(serializedObject.targetObject);
		}
		
	}
	
	#endregion
	
}
 
