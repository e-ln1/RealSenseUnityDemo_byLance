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
/// Action base custom editor. This class is reponsible to draw the custom controls in order to edit the action, triggers and rules.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(BaseAction), true)]
public class BaseActionEditor: Editor
{

    #region Private Fields

	private SerializedProperty _supportedTriggers;

	private GUIStyle _descBoxStyle = null;
	private GUIStyle _bigBoxStyle = null;					
	private GUIStyle _popupStyle = null;

    private static Dictionary<string, string> _friendlyNames = new Dictionary<string, string>();

    private BaseRule _ruleToReset = null;
    private BaseRule _ruleToRemove = null;
    private System.Type _ruleToAdd = null;

    private Trigger _triggerToAddRuleTo = null;
    private Trigger _triggerToReset = null;
    private Trigger _triggerToRemove = null;
    private System.Type _triggerToAdd = null;
	
	private List<string> _showFirstProp = new List<string>();	
    #endregion

 
    #region Unity's override methods
	
    public void OnEnable()
	{			
		_descBoxStyle = null;
		_supportedTriggers = serializedObject.FindProperty ("SupportedTriggers");  	 
	
		((BaseAction)target).InitializeSupportedTriggers();		
	} 
	
	// This function is called every editor gui update. In here we are diong our magic to show all to the user in a nice way.
	public override void OnInspectorGUI()
    { 		
		//Setting styles
		if ( _descBoxStyle == null)
		{	
			_descBoxStyle = new GUIStyle(GUI.skin.FindStyle("Box"));
			_descBoxStyle.alignment = TextAnchor.UpperLeft;
			_descBoxStyle.normal.textColor = Color.white;
			_descBoxStyle.stretchWidth = true; 
		 
			_bigBoxStyle = new GUIStyle(GUI.skin.FindStyle("Box"));
			_bigBoxStyle.alignment = TextAnchor.UpperLeft;
			_bigBoxStyle.normal.textColor = Color.white;
			_bigBoxStyle.stretchHeight = true; 		
			_bigBoxStyle.normal.background = (Texture2D) Resources.Load("SourceBack");
		
			_popupStyle = new GUIStyle(GUI.skin.FindStyle("Popup"));
	    	_popupStyle.fontStyle = (FontStyle.Normal);
	    	_popupStyle.stretchHeight = true;	
		}

        EditorGUIUtility.labelWidth = 0;
        EditorGUIUtility.fieldWidth = 0;
		
		serializedObject.Update(); // update the serialized object since the last time this method was called.
		 
        BaseAction myTarget = (BaseAction)target;       	  
		
		myTarget.UpdateInspector();

		SerializedProperty script = serializedObject.FindProperty("m_Script");	
		
		EditorGUILayout.PropertyField(script, new GUIContent("Script",myTarget.GetActionDescription()));			      	
		
		
		//Show first public visible fields with the Attribute "ShowAtfirst"
		SerializedProperty prop = serializedObject.GetIterator();
					
		prop.NextVisible(true ); 
       	do 
		{
			if (ShouldShowFirst(prop))
			{
				EditorGUILayout.PropertyField(prop,true);				
			}
        } while (prop.NextVisible(false ));
		
		//then show Triggers
		
		
		Rect rect = EditorGUILayout.BeginVertical(); 

		GUI.Box (rect,"",_bigBoxStyle);
		
		if (myTarget.IsSupportCustomTriggers())
		{ 

			GUILayout.Space(5);
			
			// Add Trigger row - Combo-box + Add Button
			Rect addTriggerRect = EditorGUILayout.BeginHorizontal();
			{				
				if (_triggerToAdd != null)
				{
					// If user pressed on "Add" button -  add the selected trigger to the action's supported triggers					
					_supportedTriggers.InsertArrayElementAtIndex(0);				
					_supportedTriggers.GetArrayElementAtIndex(0).objectReferenceValue = myTarget.AddHiddenComponent(_triggerToAdd);
					
					((Trigger)_supportedTriggers.GetArrayElementAtIndex(0).objectReferenceValue).CleanRules();
			
					_triggerToAdd = null;
				} 
				
            	GUILayout.Box("Add Trigger", _popupStyle );
				
				var evt = Event.current;
		        if (evt.type == EventType.mouseDown)
		        {
					//Getting all Triggers				
					List<System.Type> possibleTriggers = myTarget.GetSupprtedTriggers();
					
		            var mousePos = evt.mousePosition;
		            if (addTriggerRect.Contains (mousePos))
		            {
						// Now create the menu, add items and show it
						var menu  = new GenericMenu ();
						for (int i = 0; i < possibleTriggers.Count; i++)
						{
							
                            string triggerFriendlyName = "";
                            if (!_friendlyNames.ContainsKey(possibleTriggers[i].FullName))
                            {
								GameObject g = new GameObject("temp");
                                Trigger triggerTemp = (Trigger) g.AddComponent(possibleTriggers[i]); 							
                                _friendlyNames.Add(possibleTriggers[i].FullName, triggerTemp.FriendlyName);
								DestroyImmediate(g);
                            }

                            triggerFriendlyName = _friendlyNames[possibleTriggers[i].FullName];

                            
                            menu.AddItem(new GUIContent(triggerFriendlyName), false, AddTrigger, possibleTriggers[i]);
						}
						
						menu.ShowAsContext ();
		                evt.Use();
		            }
		        }
			}
			EditorGUILayout.EndHorizontal();			
		} 

		// go over all the Action's supported triggers and add them to the inspector.
		for (int i =0; i < _supportedTriggers.arraySize; i++)
		{
			//Get Trigger and update rules
			Trigger trigger = (Trigger)_supportedTriggers.GetArrayElementAtIndex(i).objectReferenceValue;		
			if (trigger == null)
			{
				((BaseAction)target).InitializeSupportedTriggers();				
				EditorGUILayout.EndVertical();
				return;	
			}	
			
			// Trigger Row - Foldout + Reset trigger button + Delete trigger
			Rect triggerRect = EditorGUILayout.BeginHorizontal();
			{ 		
				trigger.FoldoutOpen = GUIUtils.Foldout(trigger.FoldoutOpen, trigger.FriendlyName);	
				if (trigger.FoldoutOpen)
				{
					List<System.Type> supprtedRules1 = trigger.GetSupportedRules();								
					Rect rectRuleAddButton = EditorGUILayout.BeginHorizontal();
		            	
					GUILayout.Box("Add", _popupStyle, GUILayout.Width(50) );
					EditorGUILayout.EndHorizontal();
					
					GUILayout.FlexibleSpace();												
					
					var evt = Event.current;
				    if (evt.type == EventType.mouseDown)
				    {
				        var mousePos = evt.mousePosition;
				        if (rectRuleAddButton.Contains (mousePos))
				        {
				 		// Now create the menu, add items and show it
				 		var menu1  = new GenericMenu ();
				 		for (int ri = 0; ri < supprtedRules1.Count(); ri++)
				 		{
                            string ruleFriendlyName = "";
                            if (!_friendlyNames.ContainsKey(supprtedRules1[ri].FullName))
                            {									
								GameObject g = new GameObject("temp");                               
                                BaseRule ruleTemp = (BaseRule)g.AddComponent(supprtedRules1[ri]);
                                _friendlyNames.Add(supprtedRules1[ri].FullName, ruleTemp.FriendlyName);
								DestroyImmediate(g);									                               
                            }
                            
                            ruleFriendlyName = _friendlyNames[supprtedRules1[ri].FullName];
				 			
				 			menu1.AddItem (new GUIContent (ruleFriendlyName), false, AddRule, supprtedRules1[ri]);
				 		}
				 		
				 		menu1.ShowAsContext ();
				        evt.Use();
				 		
				 		_triggerToAddRuleTo = trigger;
				        }
				    }
				}
			}			
			EditorGUILayout.EndHorizontal();		
			
			SetTriggersContextMenu(triggerRect, myTarget, trigger);
			
			// show trigger's rules if opened
			if (trigger.FoldoutOpen)
			{  			  
				SerializedObject obj =  new SerializedObject(_supportedTriggers.GetArrayElementAtIndex(i).objectReferenceValue);
				SerializedProperty rules = obj.FindProperty("Rules");
				
				if (true)
				{ 
					// Add Trigger row - Combo-box + Add Button
					EditorGUILayout.BeginHorizontal();
					{
						
						//Adding combo-box control to the inspector with the list of triggers.			
						if ( _triggerToAddRuleTo == trigger &&  _ruleToAdd != null)
						{
							// If user pressed on "Add" button -  add the selected trigger to the action's supported triggers					
							rules.InsertArrayElementAtIndex(0);				
							rules.GetArrayElementAtIndex(0).objectReferenceValue = myTarget.AddHiddenComponent(_ruleToAdd);
							
							// initialize rules					
							_ruleToAdd = null;
							_triggerToAddRuleTo = null;
						} 
						
					}
					EditorGUILayout.EndHorizontal();	
					GUILayout.Space(5);
				} 
				

							
				for (int j = 0; j < rules.arraySize; j++)
				{
					BaseRule rule = (BaseRule)rules.GetArrayElementAtIndex(j).objectReferenceValue;
					DrawRuleInspector(rule);
					
					//Reset Rule
					if (_ruleToReset != null && _ruleToReset == rule)
					{			
						rule.ActionOwner = null;
						rules.GetArrayElementAtIndex(j).objectReferenceValue = myTarget.AddHiddenComponent(_ruleToReset.GetType());		
						// Save folded state
						((BaseRule)rules.GetArrayElementAtIndex(j).objectReferenceValue).FoldoutOpen = rule.FoldoutOpen;

						_ruleToReset = null;
					}
					
					//Remove Rule
					if (_ruleToRemove != null && _ruleToRemove == rule)
					{
						// delete the trigger and reorganize the array
						var r = rules.GetArrayElementAtIndex(j).objectReferenceValue;
						rules.DeleteArrayElementAtIndex(j);
						((BaseRule)r).ActionOwner = null;
						for (int k = j; k < rules.arraySize - 1; k++)
						{
							rules.MoveArrayElement(k+1, k);
						}
						rules.arraySize--;
						_ruleToRemove = null;
					}
							
				}
				
				//Save changes to the rule
				obj.ApplyModifiedProperties();
			}
			
			// Reset Trigger					
			if (_triggerToReset != null && _triggerToReset == trigger)
			{	
				
				trigger.ActionOwner = null;
				foreach (BaseRule r in trigger.Rules)
				{
					r.ActionOwner = null;
				}
				_supportedTriggers.GetArrayElementAtIndex(i).objectReferenceValue = myTarget.AddHiddenComponent(trigger.GetType());		
				
				Trigger oldTrigger = trigger;
				trigger = (Trigger)_supportedTriggers.GetArrayElementAtIndex(i).objectReferenceValue;
				 
				trigger.CleanRules();
				//trigger.SetDefaults(myTarget);					
				myTarget.SetDefaultTriggerValues(i, trigger);					
	
				// Save folded state
				trigger.FoldoutOpen = oldTrigger.FoldoutOpen;
				
				
				_triggerToReset = null;		
				break;
			}
					
			// Remove Trigger	
			if (_triggerToRemove != null && _triggerToRemove == trigger)
			{	
				// delete the trigger and reorganize the array
				
				_supportedTriggers.DeleteArrayElementAtIndex(i);
				trigger.ActionOwner = null;
				if (trigger.Rules != null)
				{
					foreach (BaseRule r in trigger.Rules)
					{
						if (r != null)
						{
							r.ActionOwner = null;
						}
					}
				}
				for (int j = i; j < _supportedTriggers.arraySize - 1; j++)
				{
					_supportedTriggers.MoveArrayElement(j+1, j);
				}
				_supportedTriggers.arraySize--;
				break;
			}
		}       
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
		
		// Draw the rest of the control except several predefined fields or the fields which are marked as show first
		prop = serializedObject.GetIterator();
					
		prop.NextVisible(true ); 
       	do 
		{
			if (prop.name != "m_Script" && !ShouldShowFirst(prop))
			{
				EditorGUILayout.PropertyField(prop,true);
			}
        } while (prop.NextVisible(false ));
		
		
		//Save changes to the Action Script
		serializedObject.ApplyModifiedProperties (); 	

	}

    #endregion


    #region Private methods	
		
	/// <summary>
	/// Returns true or false if to show the this property before the Triggers or not.
	/// </summary>
	/// <param name='prop'>
	/// property in question.
	/// </param>
	private bool ShouldShowFirst(SerializedProperty prop)
	{
		if (_showFirstProp.Contains(prop.name))
		{
			return true;
		}
		BaseAction myTarget = (BaseAction)target;       	  
		var o = myTarget.GetType().GetFields( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance ).FirstOrDefault(i => i.Name == prop.name);
		if (o != null)
		{
			var attributes = o.GetCustomAttributes(typeof(BaseAction.ShowAtFirst), false);
			if (attributes.Length > 0)
			{
				_showFirstProp.Add(prop.name);
				return true;
			}
		}
		
		return false; 		
	}
	
	/// <summary>
	/// Sets the rules context menu (add, remove, etc.)
	/// </summary>
	/// <param name='contextRect'>
	/// Context active rectangle for mouse click
	/// </param>
	/// <param name='rule'>
	/// Rule in question
	/// </param>
    private void SetRulesContextMenu(Rect contextRect, BaseRule rule)
	{
        var evt = Event.current;

        if (evt.type == EventType.ContextClick)
        {
            var mousePos = evt.mousePosition;
            if (contextRect.Contains (mousePos))
            {
				// Now create the menu, add items and show it
				var menu  = new GenericMenu ();
				
				menu.AddItem (new GUIContent ("Reset"), false, ResetRule, rule);
				menu.AddItem (new GUIContent ("Remove"), false, RemoveRule, rule);
				
				menu.ShowAsContext ();
                evt.Use();
            }
        }
	}
	
	/// <summary>
	/// Sets the triggers context menu.
	/// </summary>
	/// <param name='contextRect'>
	/// Context active rectangle for mouse click
	/// </param>
	/// <param name='action'>
	/// Action that the trigger is linked to
	/// </param>
	/// <param name='trigger'>
	/// the Trigger in question
	/// </param>
    private void SetTriggersContextMenu(Rect contextRect, BaseAction action, Trigger trigger)
	{
        var evt = Event.current;
			
        if (evt.type == EventType.ContextClick)
		{
            var mousePos = evt.mousePosition;
            if (contextRect.Contains (mousePos))
            {
				// Now create the menu, add items and show it
				var menu  = new GenericMenu ();
				
				menu.AddItem (new GUIContent ("Reset"), false, ResetTrigger, trigger);
				
				if( action.IsSupportCustomTriggers() )
				{
					menu.AddItem (new GUIContent ("Remove"), false, RemoveTrigger, trigger);
				}
				
				menu.ShowAsContext ();
                evt.Use();
            }
        }
	}

	/// <summary>
	/// Set a rule to be added.
	/// </summary>
	/// <param name='obj'>
	/// Object - rule to add. Must derive from BaseRule
	/// </param>
    private void AddRule(object obj)
	{		
		System.Type ruleType = (System.Type)obj;
		_ruleToAdd = ruleType;
	}
	
	/// <summary>
	/// Set a trigger to be added
	/// </summary>
	/// <param name='obj'>
	/// Object - trigger to add. Must derive from Trigger class.
	/// </param>
    private void AddTrigger(object obj)
	{	
		// If user pressed on "Add" button -  add the selected trigger to the action's supported triggers
		
		System.Type triggerType = (System.Type)obj;
		_triggerToAdd = triggerType;								 
	
	}
	
	/// <summary>
	/// Set the Rule to reset.
	/// </summary>
	/// <param name='obj'>
	/// Object - rule to reset. Must derive from BaseRule.
	/// </param>
    private void ResetRule(object obj)
    {
		_ruleToReset = (BaseRule)obj;
	}
	
	/// <summary>
	/// Set the rule to remove
	/// </summary>
	/// <param name='obj'>
	/// Object - rule to remove. Must derive from BaseRule.
	/// </param>
    private void RemoveRule(object obj)
    {
		_ruleToRemove = (BaseRule)obj;
	}
	
	/// <summary>
	/// Set the trigger to reset
	/// </summary>
	/// <param name='obj'>
	/// Object - trigger to reset. Must derive from Trigger class.
	/// </param>
    private void ResetTrigger(object obj)
    {
		_triggerToReset = (Trigger)obj;
	}
	
	/// <summary>
	/// Set the trigger to remove
	/// </summary>
	/// <param name='obj'>
	/// Object - trigger to remove. Must derive from Trigger class.
	/// </param>
    private void RemoveTrigger(object obj)
    {
		_triggerToRemove = (Trigger)obj;
	}

	/// <summary>
	/// Draws the rule inspector.
	/// </summary>
	/// <param name='obj'>
	/// Object - the serialized object
	/// </param>
    private void DrawRuleInspector(Object obj)
	{
		SerializedObject serObj = new SerializedObject(obj);

		//rule row - enable/disable rule foldout + reset button 
		Rect ruleTopRect = EditorGUILayout.BeginHorizontal();
		{			
			Rect imageRect = new Rect(ruleTopRect);
			imageRect.width = 20; 
			imageRect.x=35;
			Texture2D icon = (Texture2D) Resources.Load(((BaseRule)obj).GetIconPath());
			if (icon != null)
			{
				EditorGUI.DrawPreviewTexture(imageRect,(Texture2D) Resources.Load(((BaseRule)obj).GetIconPath()));
			}
			((BaseRule)obj).IsEnabled = EditorGUILayout.Toggle(((BaseRule)obj).Enabled ,GUILayout.Width(30));				
										
			GUIContent placeHolderContent = new GUIContent("", ((BaseRule)obj).GetRuleDescription());
			string[] ver = UnityEditorInternal.InternalEditorUtility.GetFullUnityVersion().Substring(0,5).Replace('.',' ').Split(' ');
			if (int.Parse(ver[0]) == 4 && int.Parse(ver[1]) <=2)
			{
				EditorGUILayout.LabelField(placeHolderContent, GUILayout.Width(25));
			}
			else 
			{
				EditorGUILayout.LabelField(placeHolderContent, GUILayout.Width(0));	
			}
			
			
			((BaseRule)obj).FoldoutOpen = GUIUtils.Foldout(((BaseRule)obj).FoldoutOpen, new GUIContent(((BaseRule)obj).FriendlyName, ((BaseRule)obj).GetRuleDescription()));
							
		}
		EditorGUILayout.EndHorizontal();
		SetRulesContextMenu(ruleTopRect,(BaseRule)obj); 
		
		
		if ( ((BaseRule)obj).FoldoutOpen )
		{
			EditorGUI.indentLevel++;
			EditorGUI.indentLevel++;
			EditorGUI.indentLevel++;							
			
	       	if(serObj != null)
			{
		       	SerializedProperty prop = serObj.GetIterator();
				
				prop.NextVisible(true ); 
		       	do  
				{
					if(prop.name != "m_Script" && prop.name != "Enabled")
					{
						EditorGUILayout.PropertyField(prop,true);
		            }
		        } while (prop.NextVisible(false ));
	        	prop.Reset();
	       	}
			else 
			{
				EditorGUILayout.PrefixLabel("Rule Null ");
			}	
			EditorGUI.indentLevel--;
			EditorGUI.indentLevel--;
			EditorGUI.indentLevel--;       
		}
			
		if (GUI.changed)
		{
			EditorUtility.SetDirty(serObj.targetObject);
		}
		
		//save changes to serialized object
		serObj.ApplyModifiedProperties();
	}
    	 
    #endregion


}
 