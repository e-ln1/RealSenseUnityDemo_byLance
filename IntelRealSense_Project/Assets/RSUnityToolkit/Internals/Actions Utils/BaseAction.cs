/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RSUnityToolkit;
using System.Linq;

/// <summary>
/// Action base - Base class for all Actions. This component holds the basic functionality to all actions in the toolkit
/// </summary>
public abstract class BaseAction : MonoBehaviour
{		
    #region Public Fields
		
	/// <summary>
	/// The supported triggers for this action
	/// </summary>
    [SerializeField]
	[HideInInspector]	
    public Trigger[] SupportedTriggers = null;
	
	/// <summary>
	/// The custom tag.
	/// </summary>
    [HideInInspector]
    public string CustomTag = "";
	
	/// <summary>
	/// The custom action identifier.
	/// </summary>
    [HideInInspector]
    public int CustomActionId = -1;

    #endregion

    #region Private Fields
			
	private List<System.Type> _supportedTriggers = null;
	
    private GameObject _prefabCopyGameObject = null;

    #endregion

    #region C'tor

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAction"/> class.
    /// </summary>
    protected BaseAction()
    {
		#if UNITY_EDITOR
        // Only call it if this is a new instance - i.e. duplicate or copy&paste
        if (this.GetInstanceID() < 0)
        {
            // Delay this call so it will happen after copy serialization
            UnityEditor.EditorApplication.delayCall += LateInit;
        }
		#endif
    }

    #endregion
	
	#region Public Methods
	
	/// <summary>
    /// This method is called when the Custom Editor is updating. To be overriden if implemented action wants to have a logic during the design time.
    /// </summary>
	public virtual void UpdateInspector()
	{		
	}
	
	/// <summary>
	/// Adds a hidden behaviour.
	/// </summary>	
	public HiddenBehaviour AddHiddenComponent(System.Type type)
	{
		HiddenBehaviour t= (HiddenBehaviour)gameObject.AddComponent(type);
		t.hideFlags = HideFlags.HideInInspector;
		t.ActionOwner = this;
		return t;
	}
	
	/// <summary>
	/// Adds a hidden component.
	/// </summary>	
	public T AddHiddenComponent<T>() where T : HiddenBehaviour
	{		
		T t = gameObject.AddComponent<T>();
		t.hideFlags = HideFlags.HideInInspector;
		t.ActionOwner = this;
		return t;
	}
	
	/// <summary>
	/// Adds a hidden behaviour.
	/// </summary>	
	public HiddenBehaviour AddHiddenComponent(HiddenBehaviour oldComponent)
	{		
		HiddenBehaviour t = (HiddenBehaviour)gameObject.AddComponent(oldComponent.GetType());
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.CopySerialized( oldComponent, t );   
		#endif
		
		t.hideFlags = HideFlags.HideInInspector;
		t.ActionOwner = this;
		return t;
	}
	
	/// <summary>
	/// Cleans the supported triggers.
	/// </summary>
	public void CleanSupportedTriggers()
	{
		if (SupportedTriggers == null)
		{
			return;
		}
		foreach (Trigger t in SupportedTriggers)
		{
			if (t!=null)
			{
				t.CleanRules();
				DestroyImmediate(t , true);
			}
		}
	}
	
    /// <summary>
    /// Determines whether this instance supports custom triggers (meaning user can add more triggers)
    /// </summary>
    /// <returns>
    /// <c>true</c> if this instance supports custom triggers; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool IsSupportCustomTriggers()
    {
        return true;
    }

    /// <summary>
    /// Gets the action description - for inspector
    /// </summary>
    /// <returns>
    /// The action description.
    /// </returns>
    public virtual string GetActionDescription()
    {
        return "";
    }

    /// <summary>
    /// Initializes the supported triggers for that action. This is based on the SetSupportedTriggers() method.
    /// </summary>
    public void InitializeSupportedTriggers()
    {
        if (SupportedTriggers == null)
        {
			CleanSupportedTriggers();
            SetDefaultTriggers();

            if (SupportedTriggers != null)
            {
                for (int i = 0; i < SupportedTriggers.Length; i++)
                {               
					SupportedTriggers[i].CleanRules();                  
                    SetDefaultTriggerValues(i, SupportedTriggers[i]);
                }
            }
        }
    }

    /// <summary>
    /// Sets default values for triggers. To be overriden if implemented action wants to have default rules.
    /// </summary>
    public virtual void SetDefaultTriggerValues(int index, Trigger trigger)
    {
    }
	
	/// <summary>
	/// Gets the supprted triggers.
	/// </summary>
	public List<System.Type> GetSupprtedTriggers()
	{
		if (_supportedTriggers != null)
		{
			return _supportedTriggers;
		}
		
		_supportedTriggers = new List<System.Type>();
		var attributes = this.GetType().GetCustomAttributes(typeof(Trigger.TriggerAtt), true);			
		
		System.Type[] types = System.Reflection.Assembly.GetAssembly(typeof(Trigger)).GetTypes();
        System.Type[] possible = (from System.Type type in types where ( typeof(Trigger) == type.BaseType ) select type).ToArray();
		
		// filter the Triggers with the attribute of the current action		
		foreach ( System.Type trigger in possible )
		{			
			if (attributes.Length==0)
			{//if no attributes set, allow every trigger
				_supportedTriggers.Add(trigger);
			}
			else 
			{//otherwise, add only the right triggers		
				foreach ( object att in attributes )
				{
					GameObject g = new GameObject("temp");                               
	                Trigger triggerTemp = (Trigger)g.AddComponent(trigger);             		
					if (att.GetType().Name == triggerTemp.TriggerAttributeName)
					{
						_supportedTriggers.Add(trigger);
					}
					DestroyImmediate(g);
				}
			}
		}		
		return _supportedTriggers;
	}
	
    #endregion

    #region Protected Methods
	
    /// <summary>
    /// Processes the trigger. Returns the trigger if one of its' rule returned a valid value. Otherwise, returns null.
    /// </summary>
    /// <returns>
    /// The trigger if one of its' rule returned a valid value. Otherwise, returns null.
    /// </returns>
    /// <typeparam name='T'>
    /// The 1st type parameter.
    /// </typeparam>
    protected T ProcessTrigger<T>() where T : Trigger
    {
        for (int i = 0; i < SupportedTriggers.Length; i++)
        {
            if (SupportedTriggers[i] is T)
            {
                if (SupportedTriggers[i].AssociatedAction == null)
                {
                    SupportedTriggers[i].AssociatedAction = this;
                }
                if (SupportedTriggers[i].ProcessRules())
                {
                    return (T)SupportedTriggers[i];
                }
                else
                {
                    return null;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Process all triggers in the SupportedTriggers collection
    /// </summary>
    protected void ProcessAllTriggers()
    {
        for (int i = 0; i < SupportedTriggers.Length; i++)
        {

            if (SupportedTriggers[i].AssociatedAction == null)
            {
                SupportedTriggers[i].AssociatedAction = this;
            }

            SupportedTriggers[i].ProcessRules();
        }
    }

    /// <summary>
    /// Sets the default triggers. To be overriden if implemented action wants to set predefiend triggers.
    /// </summary>
    protected virtual void SetDefaultTriggers()
    {
    }

	#region Menu
	#if UNITY_EDITOR
	
	/// <summary>
	/// Adds an action.
	/// </summary>	
	protected static void AddAction<T> () where T : BaseAction 
	{
		if(!UnityEditor.Selection.activeGameObject) {
			Debug.LogError("Please select at least one GameObject first");
			return;
		}
		foreach (var gameObject in UnityEditor.Selection.gameObjects)
		{
			gameObject.gameObject.AddComponent<T>();
		}
	} 
	
	#endif
	#endregion

    #endregion	
	
    #region Private Methods

    /// <summary>
    /// finalizing copy.
    /// </summary>
    private void FinishCopyPrefabValues()
    {
		#if UNITY_EDITOR
        this.gameObject.tag = "Untagged";
        _prefabCopyGameObject.tag = "Untagged";
        foreach (BaseAction ac in _prefabCopyGameObject.GetComponents<BaseAction>())
        {
            ac.CustomTag = "";
        }
        UnityEditor.AssetDatabase.SaveAssets();
		#endif
    }

    /// <summary>
    /// Copy prefab values if needed
    /// </summary>
    private void CopyPrefabValues()
    {
		#if UNITY_EDITOR
        object[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        foreach (object thisObject in allObjects)
        {
            if (((GameObject)thisObject).GetInstanceID() != this.gameObject.GetInstanceID())
            {
                if (((GameObject)thisObject).tag == this.gameObject.tag && UnityEditor.PrefabUtility.GetPrefabType((GameObject)thisObject) != UnityEditor.PrefabType.Prefab)
                {
                    _prefabCopyGameObject = (GameObject)thisObject;

                    foreach (BaseAction ac in _prefabCopyGameObject.GetComponents<BaseAction>())
                    {
                        if (ac.GetType() == this.GetType() && ac.CustomTag == "")
                        {
                            ac.CustomTag = "done";
                            UnityEditor.EditorApplication.delayCall += FinishCopyPrefabValues;
                            CopyTriggersValuesIfNeeded(ac.SupportedTriggers);
                            return;
                        }
                    }
                }
            }
        }
		#endif
    }

    /// <summary>
    /// Initialize action - this is important mainly for prefabs instanciation and copy-paste/duplication
    /// </summary>
    private void LateInit()
    {
		#if UNITY_EDITOR

        if (this == null)
        {
            // in the case we destroyed this object
            return;
        }
		bool isPrefabInstance = UnityEditor.PrefabUtility.GetPrefabType(gameObject) == UnityEditor.PrefabType.PrefabInstance;
		
		if (isPrefabInstance)
		{
			if (SupportedTriggers != null)
			{
				foreach ( Trigger t in SupportedTriggers)
				{
					t.hideFlags = HideFlags.HideInInspector;
					
					if (t.Rules!=null)
					{
						foreach (BaseRule r in t.Rules)
						{
							r.hideFlags = HideFlags.HideInInspector;
						}
					}
				}
			}
		}			
		#endif		
        CopyTriggersValuesIfNeeded(null);
    }

    /// <summary>
    /// Copies the triggers values if needed - when duplicating or copy/paste a component
    /// </summary>
    private void CopyTriggersValuesIfNeeded(Trigger[] triggers)
    {
        if (triggers == null)
        {
            triggers = SupportedTriggers;
        }

        if (triggers != null)
        {
            int size = triggers.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                if (triggers[i] == null)
                {
                    SupportedTriggers = null;
                    InitializeSupportedTriggers();
                    triggers = SupportedTriggers;
                }

                Trigger t = triggers[i];
				
				if (triggers[i].ActionOwner != this)
				{
                	SupportedTriggers[i] = (Trigger)AddHiddenComponent(t);
				}
				
                int ruleSize = triggers[i].Rules.GetLength(0);
                for (int j = 0; j < ruleSize; j++)
                {
                    if (triggers[i].Rules[j] == null)
                    {
                        break;
                    }	
					if (triggers[i].Rules[j].ActionOwner != this)
					{
						SupportedTriggers[i].Rules[j]  = (BaseRule)AddHiddenComponent(triggers[i].Rules[j]); 
					}
                }
            }
        }
    }

    #endregion
	
    #region Unity's overriden methods

    /// <summary>
    /// Called when user resets the action. 
    /// </summary>
    void Reset()
    {        				
        CustomActionId = this.gameObject.GetComponents<BaseAction>().Length;

		CleanSupportedTriggers();
        SupportedTriggers = null;
        InitializeSupportedTriggers();
		
    }
	
	#if UNITY_EDITOR	
	[UnityEditor.MenuItem("CONTEXT/BaseAction/Reset")]
	static void DoReset (UnityEditor.MenuCommand  command) 
	{
		((BaseAction)command.context).CleanSupportedTriggers();
		
		GameObject g = new GameObject("temp");
		BaseAction b = (BaseAction)g.AddComponent(command.context.GetType());
		UnityEditor.EditorUtility.CopySerialized( b , command.context);   		
		DestroyImmediate(g);
		((BaseAction)command.context).Reset();		
	}
	#endif
	
	/// <summary>
	/// Disables the triggers on Disable.
	/// </summary>
	void OnDisable()
	{
		if (SupportedTriggers == null)
		{
			return;
		}
		foreach (var trigger in SupportedTriggers)
		{
			trigger.Disable();
		}
	}

    /// <summary>
    /// Called when action starts
    /// </summary>
    public void Start()
    {
        var senseManager = GameObject.FindObjectOfType(typeof(SenseToolkitManager));
        if (senseManager == null)
        {
            Debug.LogWarning("Sense Manager Object not found and was added automatically");
			senseManager = (GameObject)Instantiate(Resources.Load("SenseManager"));            
            senseManager.name = "SenseManager";
        }

        InitializeSupportedTriggers();

        //Init to rules
        foreach (Trigger trgr in SupportedTriggers)
        {
            trgr.AssociatedAction = this;
            trgr.InitRules();
        }
    }

    #endregion
   
	#region Nested Types
	
	/// <summary>
	/// Trigger attribute - base class for all triggers attributes.
	/// </summary>
	public class ShowAtFirst :  System.Attribute
	{
	}
	
	#endregion
}   
 