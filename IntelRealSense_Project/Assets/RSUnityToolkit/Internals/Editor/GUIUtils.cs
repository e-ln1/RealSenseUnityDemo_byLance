/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using UnityEditor;
static class GUIUtils
{
	
	#region Private Fields
	
    private static GUIStyle _openFoldoutStyle;
    private static GUIStyle _closedFoldoutStyle;
    private static bool _initialized;
 
	#endregion
	
	#region Private Methods
	
	/// <summary>
	/// Initialize the first time
	/// </summary>
    private static void Init()
    {
		if (!_initialized) 
		{		
	        _openFoldoutStyle = new GUIStyle(GUI.skin.FindStyle("Foldout"));
	        _openFoldoutStyle.fontStyle = (FontStyle.Normal);
	        _openFoldoutStyle.stretchHeight = true;
	        _closedFoldoutStyle = new GUIStyle(_openFoldoutStyle);
	        _openFoldoutStyle.normal = _openFoldoutStyle.onNormal;
	        _openFoldoutStyle.active = _openFoldoutStyle.onActive;
	        _initialized = true;
		}
    }
	
	#endregion
 
	#region Public Methods
    
	/// <summary>
	/// Create a Foldout
	/// </summary>
	/// <param name='open'>
	/// If set to <c>true</c> open.
	/// </param>
	/// <param name='text'>
	/// pre-text
	/// </param>
	public static bool Foldout(bool open, string text) 
	{ 
		return Foldout(open, new GUIContent(text)); 
	}
    
	/// <summary>
	/// Create a Foldout
	/// </summary>
	/// <param name='open'>
	/// If set to <c>true</c> open.
	/// </param>
	/// <param name='text'>
	/// pre-text
	/// </param>
	public static bool Foldout(bool open, GUIContent text)
    {		
        Init();
		
		var evt = Event.current;
   
		EventType evtType = evt.type;
			
        if (open)
        {
            GUILayout.BeginHorizontal();            
            if (GUILayout.Button(text, _openFoldoutStyle, GUILayout.Height(20), GUILayout.ExpandWidth(true)) && evt.button == 0)
            {
                GUI.FocusControl("");
                GUI.changed = false; 
                GUI.changed = true;
                return false;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();        
            if (GUILayout.Button(text, _closedFoldoutStyle, GUILayout.Height(20)) && evt.button == 0)
            {
                GUI.FocusControl("");
                GUI.changed = false;
                GUI.changed = true;
                return true;
            }
            GUILayout.EndHorizontal();
        }
		
		if (evt.button != 0)
		{
			Event.current = new Event(evt);
			Event.current.type = evtType;
		}
		
        return open;
    }

	#endregion
}