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
    /// Face Lost rule - Process Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class FaceLostRule : BaseRule
    {
		#region Public fields
		
		/// <summary>
		/// Should return true for every lost face?
		/// </summary>
		public bool RiseForEveryLostFace = true;
		
		/// <summary>
		/// Minimum number of faces.
		/// </summary>
		public int NumberOfFaces = 1;
		
		#endregion
		
        #region Private fields

        private int _numberOfDetectedFaces = 0;

        #endregion
		
        #region C'tor
        public FaceLostRule(): base()
        {
            FriendlyName = "Face Lost";
        }

        #endregion

        #region Public Methods

        override public string GetIconPath()
        {
            return @"RulesIcons/face-lost";
        }

        protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Face);
            return true;
        }
		
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Face);
		}

        override public string GetRuleDescription()
        {
            return "Fires whenever a face was previously detected and lost";
        }

        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Face))
            {
                trigger.ErrorDetected = true;
                return false;
            }

            bool success = false;

            if (!(trigger is EventTrigger))
            {
                trigger.ErrorDetected = true;
                return false;
            }

            EventTrigger specificTrigger = (EventTrigger)trigger;
            specificTrigger.Source = this.name;


            if (SenseToolkitManager.Instance.Initialized
                &&
                SenseToolkitManager.Instance.FaceModuleOutput != null)
            {
				int currentNumberOfFaces = SenseToolkitManager.Instance.FaceModuleOutput.QueryNumberOfDetectedFaces();
                if ( currentNumberOfFaces < _numberOfDetectedFaces )
                {						
					if (currentNumberOfFaces >= NumberOfFaces - 1)
					{
						if (currentNumberOfFaces == NumberOfFaces - 1 || RiseForEveryLostFace )
						{
                    		success = true;
						}
					}
                }
                _numberOfDetectedFaces = currentNumberOfFaces;				              

            }

            return success;

        }

        #endregion
    }
}
