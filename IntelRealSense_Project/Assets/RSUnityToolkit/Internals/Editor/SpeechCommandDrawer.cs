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

[CustomPropertyDrawer(typeof(SpeechCommand))]
public class SpeechCommandDrawer : PropertyDrawer {
	
	 public override void OnGUI (Rect position,
                                SerializedProperty property,
                                GUIContent label) 
	{
		EditorGUIUtility.LookLikeControls();
		
		EditorGUI.BeginProperty (position, label, property);		
		
		position.x=-115;
		// Draw label				

		//EditorGUI.PrefixLabel( position, GUIUtility.GetControlID (FocusType.Passive), label);
		
		Rect rect = new Rect(position);
		rect.y+=2;
		rect.width = 100;
		rect.height = 15;
		
		SerializedProperty text = property.FindPropertyRelative("Word");		
		SerializedProperty num = property.FindPropertyRelative("Confidence");				
				
		rect.x+=120;
		rect.width = 180;
		
		//EditorGUI.LabelField(rect,new GUIContent("Min"));
		EditorGUI.LabelField(rect,new GUIContent("Word"));
		
		rect.x+=40;
		rect.width = 155;
		//float1.floatValue = EditorGUI.FloatField(rect, float1.floatValue);		
		text.stringValue = EditorGUI.TextArea(rect,text.stringValue);
		//EditorGUI.LabelField(rect,new GUIContent(text.stringValue));
		
		rect.x+=95;		
		rect.width = 170;
		//EditorGUI.LabelField(rect,new GUIContent("Max"));
		EditorGUI.LabelField(rect,new GUIContent("Min Confidence"));
		
		rect.x+=105;
		rect.width = 95;
		//float2.floatValue = EditorGUI.FloatField(rect, float2.floatValue);	
		num.intValue = EditorGUI.IntField(rect, num.intValue);	
		
		EditorGUI.EndProperty ();
	
	}
}

 