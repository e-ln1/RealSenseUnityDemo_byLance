/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using System.Collections;


namespace RSUnityToolkit
{
    /// <summary>
    /// Rule base - Base class for all rules. This component holds the basic functionality to all rules in the toolkit
    /// </summary>
    [AddComponentMenu("")]
	[System.Serializable]
    public abstract class BaseRule : HiddenBehaviour
    {
        #region Public Fields
        /// <summary>
        /// Humanly understandble name of the trigger to be written in the inspector
        /// </summary>
        [HideInInspector]
        public string FriendlyName = "";

        /// <summary>
        /// Set/Get if that rule is enabled.
        /// </summary>
        [SerializeField]
        public bool Enabled = true;

        /// <summary>
        /// Used in the inspector
        /// </summary>
        [HideInInspector]
        public bool FoldoutOpen = false;
		
		/// <summary>
		/// The associated toolkit action.
		/// </summary>
		[HideInInspector]
        public BaseAction AssociatedAction = null;

        #endregion
		
		#region C'tors
		public BaseRule() : base()
		{
			
		}
		#endregion
		
        #region Private fields
   
		/// <summary>
        /// Flag indicates if this rule is already initialized
        /// </summary>
        private bool _initialized = false;

        #endregion

        #region Public Properties
       
		/// <summary>
		/// Gets or sets a value indicating whether this instance is enabled.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool IsEnabled
        {
            get
            {
                return Enabled;
            }
            set
            {
				if ( Application.isPlaying && !value)
				{
					Disable();
				}
                Enabled = value;
            }

        }
		
		/// <summary>
		/// Gets or sets a value indicating whether this instance is initialized.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
		/// </value>
        public bool IsInitialized
        {
            get
            {
                return _initialized;
            }
            protected set
            {
                _initialized = value;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the rule description - to be used in the inspector
        /// </summary>
        /// <returns>
        /// The rule description.
        /// </returns>
        public abstract string GetRuleDescription();

        /// <summary>
        /// Process the rule with the specified trigger.
        /// </summary>
        /// <param name='trigger'>
        /// If set to <c>true</c> trigger.
        /// </param>
        public abstract bool Process(Trigger trigger);
		
		/// <summary>
        /// Inititializes the rule
        /// </summary>
		public void Init()
		{
			if (!IsInitialized)
			{
				IsInitialized = OnRuleEnabled();
			}
		}
		
		/// <summary>
		/// Returns the path to the icon.
		/// </summary>		
        public virtual string GetIconPath()
        {
            return "";
        }
       
		/// <summary>
		/// Disable this instance.
		/// </summary>
		public void Disable()
		{
			if (IsInitialized)
			{
				OnRuleDisabled();
				IsInitialized = false;
			}
		}
	
        #endregion
		
		#region Protected Methods	
		
		/// <summary>
		/// Raises the rule enabled event.
		/// </summary>
        protected abstract bool OnRuleEnabled();
		
		
		/// <summary>
		/// Raises the rule disabled event.
		/// </summary>
		protected virtual void OnRuleDisabled()
		{

		}

		
		#endregion
    }
}