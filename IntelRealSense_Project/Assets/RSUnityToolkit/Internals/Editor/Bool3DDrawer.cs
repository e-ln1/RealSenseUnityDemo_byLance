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

[CustomPropertyDrawer(typeof(Bool3D))]
public class Bool3DDrawer : PropertyDrawer {
	
	 public override void OnGUI (Rect position,
                                SerializedProperty property,
                                GUIContent label) 
	{
		EditorGUIUtility.LookLikeControls();
				
		EditorGUI.BeginProperty (position, label, property);
		
		position.x+=4;
		
		// Draw label				
		EditorGUI.PrefixLabel( position, GUIUtility.GetControlID (FocusType.Passive), label);
		
		Rect rect = new Rect(position);
		rect.y+=2;
		rect.width = 30;
		rect.height = 15;

		SerializedProperty rotationX = property.FindPropertyRelative("X");
		SerializedProperty rotationY = property.FindPropertyRelative("Y");
		SerializedProperty rotationZ = property.FindPropertyRelative("Z");
		
		rect.x+=70;
		rotationX.boolValue = EditorGUI.Toggle(rect, rotationX.boolValue);
		rect.x+=15;
		EditorGUI.LabelField(rect,new GUIContent("X"));
		
		rect.x+=20;
		rotationY.boolValue = EditorGUI.Toggle(rect, rotationY.boolValue);		
		rect.x+=15;
		EditorGUI.LabelField(rect,new GUIContent("Y"));
		
		rect.x+=20;
		rotationZ.boolValue = EditorGUI.Toggle(rect, rotationZ.boolValue);		
		rect.x+=15;
		EditorGUI.LabelField(rect,new GUIContent("Z"));
		
		EditorGUI.EndProperty ();
	}
}

 