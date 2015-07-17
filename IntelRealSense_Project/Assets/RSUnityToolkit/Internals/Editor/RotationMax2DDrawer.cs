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

[CustomPropertyDrawer(typeof(RotationMax2D))]
public class RotationMax2DDrawer : PropertyDrawer {
	
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
		
		SerializedProperty int1 = property.FindPropertyRelative("MaxNeg");
		SerializedProperty int2 = property.FindPropertyRelative("MaxPos");
				
		rect.x+=80;
		rect.width = 70;
		EditorGUI.LabelField(rect,new GUIContent("Max Neg"));
		
		rect.x+=65;
		rect.width = 50;
		int1.intValue = EditorGUI.IntField(rect, int1.intValue);		
		
		rect.x+=60;		
		rect.width = 65;
		EditorGUI.LabelField(rect,new GUIContent("Max Pos"));
		
		rect.x+=60;
		rect.width = 50;
		int2.intValue = EditorGUI.IntField(rect, int2.intValue);		
		
		EditorGUI.EndProperty ();
		
	}
}

 