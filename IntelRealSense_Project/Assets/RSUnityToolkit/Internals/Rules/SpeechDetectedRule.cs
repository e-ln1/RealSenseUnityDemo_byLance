/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RSUnityToolkit
{
    /// <summary>
    /// Speech Deteted rule: Implements Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class SpeechDetectedRule : BaseRule
    {
        #region Public Fields
		
		/// <summary>
		/// When set to true updates the commands in the speech manager
		/// </summary>
		public bool UpdateCommands = false;
		
		/// <summary>
		/// The commands to detect and their minumum confidence
		/// </summary>
		///         
		public SpeechCommand[] SpeechCommands = new SpeechCommand[0];
		
        #endregion

        #region C'tor

        public SpeechDetectedRule(): base()
        {
            FriendlyName = "Speech Detected";
        }

        #endregion

        #region Private Fields
						
        private List<string> _lastCmds;
		
        #endregion

        #region Public Methods
				
        public override string GetIconPath()
        {
            return @"RulesIcons/speech-detected";
        }
		
        public override string GetRuleDescription()
        {
            return "Fires upon command recognition";
        }
				
        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Speech))
            {
                trigger.ErrorDetected = true;
                Debug.LogError("Speech Module Not Set");
                return false;
            }
			
			if (UpdateCommands == true)
			{				
				UpdateCommandsInSenseManager();
				Debug.Log("Updated Speech Commands");
				UpdateCommands = false;
			}
			
            if (!(trigger is EventTrigger))
            {
                trigger.ErrorDetected = true;
                return false;
            }
			
			if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.SpeechOutput != null)
			{
				for (int i=0; i<SpeechCommands.Length; i++)				
				{					
					if (SenseToolkitManager.Instance.SpeechOutput.ContainsKey(SpeechCommands[i].Word))
					{
						if (SenseToolkitManager.Instance.SpeechOutput[SpeechCommands[i].Word] > SpeechCommands[i].Confidence) 
						{						
							return true;
						}
					}
				}
			}            
            
            return false;
        }
		
        #endregion
		
		#region Protected Methods
		
		protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Speech);
			_lastCmds = new List<string>();
			
			foreach (SpeechCommand cmd in SpeechCommands)
			{
				SenseToolkitManager.Instance.AddSpeechCommand(cmd.Word);
				_lastCmds.Add(cmd.Word);
			}
				
            return true;

        }
		
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Speech);
		}
		
		#endregion
		
		#region Private Methods
		
		private void UpdateCommandsInSenseManager()
		{
			bool commandsChanged = false;
			for (int i=0; i<SpeechCommands.Length; i++)
			{
				//These commands were added
				if (!_lastCmds.Contains(SpeechCommands[i].Word))
				{
					SenseToolkitManager.Instance.AddSpeechCommand(SpeechCommands[i].Word);
					commandsChanged = true;
				}
				else
				{
					_lastCmds.Remove(SpeechCommands[i].Word);
				}
			}
			
			//These commands were removed
			for (int i=0; i<_lastCmds.Count; i++)
			{				
				SenseToolkitManager.Instance.RemoveSpeechCommand(_lastCmds[i]);
				commandsChanged = true;
			}
			
			if (commandsChanged)
			{
				_lastCmds.Clear();	
				foreach (SpeechCommand cmd in SpeechCommands)
				{					
					_lastCmds.Add(cmd.Word);
				}
			}			
		}
		
		#endregion
    }
}
