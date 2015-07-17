/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

//This Action Is Available to Unity 4.3+ Only
#if (UNITY_4_2_2 || UNITY_4_2_1 || UNITY_4_2_0 || UNITY_4_1_5 || UNITY_4_1_4 || UNITY_4_1_3 || UNITY_4_1_2 || UNITY_4_1_0 || UNITY_4_0_1 || UNITY_4_0_0 || UNITY_3_5_7 || UNITY_3_5_6 || UNITY_3_5_5 || UNITY_3_5_4 || UNITY_3_5_3 || UNITY_3_5_2 || UNITY_3_5_1 || UNITY_3_5_0 || UNITY_3_4_2 || UNITY_3_4_1 || UNITY_3_4_0)
#define PRE_UNITY_4_3
#endif

#if !PRE_UNITY_4_3

using UnityEngine;
using UnityEditor;

/// <summary>
/// Facial animation action custom editor. This class is reponsible to draw the custom controls in order to map available blend shapes to the supported expressions.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(BlendShapeAnimationAction), true)]
public class BlendShapeAnimationActionEditor: BaseActionEditor
{
    #region Private Fields

	private SerializedProperty _blendShapeNames;
	private SerializedProperty _blendShapeExpressions;

    #endregion

    #region Unity's override methods
	
	new public void OnEnable()
	{
		base.OnEnable();

		_blendShapeNames = serializedObject.FindProperty("_blendShapeNames");
		_blendShapeExpressions = serializedObject.FindProperty("_blendShapeExpressions");
	}

	public override void OnInspectorGUI()
    { 	
		base.OnInspectorGUI();

		EditorGUILayout.Space();

		SkinnedMeshRenderer renderer = ((Component)target).gameObject.GetComponent<SkinnedMeshRenderer>();

		if (renderer != null)
		{
			int rendererBlendShapeCount = renderer.sharedMesh.blendShapeCount;

			if (rendererBlendShapeCount > 0)
			{
				string[] rendererBlendShapeNames = new string[rendererBlendShapeCount];
				string[] newBlendShapeExpressions = new string[rendererBlendShapeCount];

				// Getting the renderer's list of blend shape names
				for (int i = 0; i < rendererBlendShapeCount; i++)
				{
					rendererBlendShapeNames[i] = renderer.sharedMesh.GetBlendShapeName(i);
				}

				// Checking if the length of the renderer's list of blend shapes is any different than that of the previously-serialized list of blend shape names
				bool isBlendShapeNamesChanged = (rendererBlendShapeCount != _blendShapeNames.arraySize);

				if (!isBlendShapeNamesChanged)
				{
					for (int i = 0; i < rendererBlendShapeCount; i++)
					{
						// Checking if the current blend shape name in the renderer is any different than that of the previously-serialized blend shape name at the same index
						if (rendererBlendShapeNames[i] != _blendShapeNames.GetArrayElementAtIndex(i).stringValue)
						{
							isBlendShapeNamesChanged = true;
							break;
						}
					}
				}
				
				// Checking if the length of the renderer's list of blend shapes is any different than that of the previously-serialized list of blend shape expressions
				bool isBlendShapeExpressionsChanged = (rendererBlendShapeCount != _blendShapeExpressions.arraySize);

				EditorGUI.indentLevel++;

				BlendShapeAnimationAction.IsBlendShapeListVisible = EditorGUILayout.Foldout(BlendShapeAnimationAction.IsBlendShapeListVisible, "BlendShapes");

				if (BlendShapeAnimationAction.IsBlendShapeListVisible)
				{
					EditorGUI.indentLevel++;
				}
				
				for (int i = 0; i < rendererBlendShapeCount; i++)
				{
					// Getting the serialized expression of the current blend shape name
					string blendShapeExpression = GetBlendShapeExpressionByName(rendererBlendShapeNames[i]);

					if (BlendShapeAnimationAction.IsBlendShapeListVisible)
					{
						// Displaying the inspector text field
						newBlendShapeExpressions[i] = EditorGUILayout.TextField(rendererBlendShapeNames[i], blendShapeExpression);
					}
					else
					{
						newBlendShapeExpressions[i] = blendShapeExpression;
					}

					// Checking if the current blend shape expression is any different than that of the previously-serialized blend shape expression for the current blend shape name
					if (!isBlendShapeExpressionsChanged && (newBlendShapeExpressions[i] != blendShapeExpression))
					{
						isBlendShapeExpressionsChanged = true;
					}
				}

				if (isBlendShapeNamesChanged)
				{
					// Resetting the serialized blend shape names array
					_blendShapeNames.ClearArray();
					_blendShapeNames.arraySize = rendererBlendShapeCount;

					// Copying the changed list of blend shape names to the serialized array
					for (int i = 0; i < rendererBlendShapeCount; i++)
					{
						_blendShapeNames.GetArrayElementAtIndex(i).stringValue = rendererBlendShapeNames[i];
					}
				}

				if (isBlendShapeExpressionsChanged)
				{
					// Resetting the serialized blend shape expressions array
					_blendShapeExpressions.ClearArray();
					_blendShapeExpressions.arraySize = rendererBlendShapeCount;
				
					// Copying the changed list of blend shape expressions to the serialized array
					for (int i = 0; i < rendererBlendShapeCount; i++)
					{
						_blendShapeExpressions.GetArrayElementAtIndex(i).stringValue = newBlendShapeExpressions[i];
					}
				}

				serializedObject.ApplyModifiedProperties(); 	
			}
		}
	}

	private string GetBlendShapeExpressionByName(string blendShapeName)
	{
		for (int i = 0; i < _blendShapeNames.arraySize; i++)
		{
			if (_blendShapeNames.GetArrayElementAtIndex(i).stringValue == blendShapeName)
			{
				return _blendShapeExpressions.GetArrayElementAtIndex(i).stringValue;
			}
		}

		return "";
	}

    #endregion
}

#endif