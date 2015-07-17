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
using System.Linq;

namespace RSUnityToolkit
{
	/// <summary>
	/// Base Trigger class for all triggers. This component holds the basic functionality to all trigger classes
	/// </summary>
	[System.Serializable]
	[AddComponentMenu("")]
  	public abstract class Trigger : HiddenBehaviour
    {
		
		#region Nested Types
		/// <summary>
		/// Trigger attribute - base class for all triggers attributes.
		/// </summary>
		public class TriggerAtt :  System.Attribute
		{
		}
		#endregion

        #region Private Fields

        private Trigger _tempTrigger = null;

        #endregion

        #region Public fields

        public bool ErrorDetected = false;
		
		/// <summary>
		/// Humanly understandble name of the trigger to be written in the inspector
		/// </summary>
        [SerializeField]
        public string FriendlyName;
		
		/// <summary>
		/// The list of associated rules. This will be initialized automatically
		/// </summary>
        [SerializeField]
        public BaseRule[] Rules;
		
		/// <summary>
		/// The name of the trigger attribute. To be set in the contructor of the implemented trigger
		/// </summary>
        [SerializeField]
        public string TriggerAttributeName = "";
		
		/// <summary>
		/// Used in the inspector
		/// </summary>
        [HideInInspector]
        public bool FoldoutOpen = true;
		
		/// <summary>
		/// The associated action. This will be initialized automatically by the Action
		/// </summary>
        [HideInInspector]
        public BaseAction AssociatedAction = null;
		
		[HideInInspector]
		public string Source = "";
		
        public bool Success = false;

		#endregion 
		
		#region Public Methods
		
		/// <summary>
		/// Destroys all rules associated to the trigger.
		/// </summary>
		public void CleanRules()
		{
			if (Rules == null)
			{
				return;
			}
			foreach (BaseRule r in Rules)
			{
				DestroyImmediate(r, true);
			}
		}
		
		/// <summary>
		/// Returns a list of types of all rules that implement this trigger
		/// </summary>
		public List<System.Type> GetSupportedRules()
		{
		
			System.Type[] types = System.Reflection.Assembly.GetAssembly(typeof(BaseRule)).GetTypes();
            System.Type[] possible = (from System.Type type in types where ( typeof(BaseRule) == type.BaseType ) select type).ToArray();
			
			List<System.Type> possible1 = new List<System.Type>();
			
			// filter the rules with the attribute of the current trigger
			
			foreach ( System.Type rule in possible )
			{
				var attributes = rule.GetCustomAttributes(typeof(Trigger.TriggerAtt), true);
			
				foreach ( object att in attributes )
				{
					if (att.GetType().Name == TriggerAttributeName)
					{
						possible1.Add(rule);
					}
				}
			}
			
			return possible1;
		}

		/// <summary>
		/// Initializes all the rules. This is optional as we are initializing the rules if needed also during ProcessRules()
		/// </summary>
        public void InitRules()
        {
			foreach (BaseRule rule in Rules)
            {
				if (rule == null)
				{
					continue;
				}
                if (rule.IsEnabled && !rule.IsInitialized)
                {
                	rule.Init();
                }
            }
        }

		
		/// <summary>
		/// Processes all the rules in the trigger. In the case a rule is not initialized, we initialize it as well.
		/// </summary>
		/// <returns>
		/// The rules.
		/// </returns>
        public bool ProcessRules()
        {
            if (_tempTrigger == null)
            {
                _tempTrigger = (Trigger)ActionOwner.AddHiddenComponent(this.GetType());
            }

            this.Success = false;
			if (SenseToolkitManager.Instance != null  && SenseToolkitManager.Instance.Initialized)
			{			
		        foreach (BaseRule rule in Rules)
		        {		
					if (rule == null)
					{
						return false;
					}
					
					if (rule.IsEnabled && !rule.IsInitialized)
	            	{
	                	rule.Init();
	                }
	                if (rule.IsEnabled && rule.IsInitialized)
	                {
                        if (!this.Success)
                        {
                            if (rule.Process(this) && !this.ErrorDetected)
                            {
                                // here we can add logical equation...
                                this.Success = true;
								
								this.Source = rule.FriendlyName;
                            }
                        }
                        else
                        {
                            rule.Process(_tempTrigger);
                        }                                                	                    
	                }
	            }
			}

            return this.Success;
        }
			
		/// <summary>
		/// Disables all associated rules
		/// </summary>
		public virtual void Disable()
		{
			if (Rules == null)
			{
				return;
			}
			foreach (var rule in Rules)
			{
				if (rule.IsInitialized)
				{
					rule.Disable();
				}
			}
		}		
		
		#endregion

        #region C'tors

        public Trigger() : base()
		{
			TriggerAttributeName = GetAttributeName();
			FriendlyName = GetFriendlyName();      
		}
		 
        #endregion

        #region Protected Methods
		
		protected abstract string GetAttributeName();
		
		protected abstract string GetFriendlyName();

		#endregion

	
    }
}