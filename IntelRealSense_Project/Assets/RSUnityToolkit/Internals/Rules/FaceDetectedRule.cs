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
    /// Face Detected rule - Process Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class FaceDetectedRule : BaseRule
    {
		
		#region Public fields
		
		/// <summary>
		/// Should return true for every detected face?
		/// </summary>
		public bool RiseForEveryDetectedFace = true;
		
		/// <summary>
		/// Minimum number of faces.
		/// </summary>
		public int NumberOfFaces = 1;
		
		#endregion
		
        #region Private fields

        private int _numberOfDetectedFaces = 0;

        #endregion

        #region C'tor
        public FaceDetectedRule(): base()
        {
            FriendlyName = "Face Detected";
        }

        #endregion

        #region Public Methods

        protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Face);
            return true;
        }
		
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Face);
		}

        override public string GetIconPath()
        {
            return @"RulesIcons/face-detected";
        }

        override public string GetRuleDescription()
        {
            return "Fires whenever a face is detected";
        }

        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Face))
            {
                trigger.ErrorDetected = true;
                return false;
            }

            if (!(trigger is EventTrigger))
            {
                trigger.ErrorDetected = true;
                return false;
            }

            bool success = false;
        
            EventTrigger specificTrigger = (EventTrigger)trigger;
            specificTrigger.Source = this.name;

            if (SenseToolkitManager.Instance.Initialized
                &&
                SenseToolkitManager.Instance.FaceModuleOutput != null)
            {
				int currentNumberOfFaces = SenseToolkitManager.Instance.FaceModuleOutput.QueryNumberOfDetectedFaces();
                if ( currentNumberOfFaces > _numberOfDetectedFaces )
                {						
					if (currentNumberOfFaces >= NumberOfFaces)
					{
						if (currentNumberOfFaces == NumberOfFaces || RiseForEveryDetectedFace )
						{
                    		success = true;
						}
					}
                }
                _numberOfDetectedFaces = currentNumberOfFaces;

            }
            else
            {
				_numberOfDetectedFaces = 0;
                success = false;
            }

        	return success;

		}

        #endregion
    }
}
