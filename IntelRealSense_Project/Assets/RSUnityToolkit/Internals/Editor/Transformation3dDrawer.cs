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

[CustomPropertyDrawer(typeof(Transformation3D))]
public class Transformation3dDrawer : PropertyDrawer {
	
	#region Private Fields
	
	private bool _folded;
	
	#endregion
	
	public override void OnGUI (Rect position,
                                SerializedProperty property,
                                GUIContent label) 
	{
		// Using BeginProperty / EndProperty on the parent property means that		
		EditorGUI.BeginProperty (position, label, property);
		 
		EditorGUIUtility.LookLikeControls();
		
		string[] ver = UnityEditorInternal.InternalEditorUtility.GetFullUnityVersion().Substring(0,5).Replace('.',' ').Split(' ');
		if (int.Parse(ver[0]) == 4 && int.Parse(ver[1]) <=2)
		{
			position.x-=8;
		}
		
		// Draw label				
		_folded = EditorGUI.Foldout(position,_folded,label);	

		if (_folded)
		{
			position.height = 100;
			position.y +=18;
			position.x +=18; 
			EditorGUI.PropertyField(position, property.FindPropertyRelative("Position"));
			
			position.y +=18;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("Rotation"));
			GUILayout.Space(35);
			
		}
		
		EditorGUI.EndProperty ();
		
		
	}
}

 