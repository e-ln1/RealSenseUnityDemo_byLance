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

[CustomPropertyDrawer(typeof(MinMax2D))]
public class MinMax2DDrawer : PropertyDrawer {
	
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
		rect.width = 50;
		rect.height = 15;
		
		SerializedProperty float1 = property.FindPropertyRelative("Min");
		SerializedProperty float2 = property.FindPropertyRelative("Max");
				
		rect.x+=80;
		rect.width = 70;
		EditorGUI.LabelField(rect,new GUIContent("Min"));
		
		rect.x+=35;
		rect.width = 50;
		float1.floatValue = EditorGUI.FloatField(rect, float1.floatValue);		
		
		rect.x+=60;		
		rect.width = 65;
		EditorGUI.LabelField(rect,new GUIContent("Max"));
		
		rect.x+=30;
		rect.width = 50;
		float2.floatValue = EditorGUI.FloatField(rect, float2.floatValue);		
		
		EditorGUI.EndProperty ();
	
	}
}

 