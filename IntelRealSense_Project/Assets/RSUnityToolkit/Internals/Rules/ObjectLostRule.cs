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
    /// Object Lost rule - Process Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class ObjectLostRule : BaseRule
    {
		#region Public fields
		
		/// <summary>
		/// The path to the image
		/// </summary>
		public string Tracker2DPath = "";
		
		/// <summary>
		/// Should return true for every lost object?
		/// </summary>
		public bool RiseForEveryLostObject = true;
		
		/// <summary>
		/// The minimal number of objects.
		/// </summary>
		public int NumberOfObjects = 1;
		
		#endregion
		
        #region Private fields

        private int _numberOfDetectedObjects = 0;		
		private int _trackerID  = -1;
		
        #endregion
		
        #region C'tor
        public ObjectLostRule(): base()
        {
            FriendlyName = "Object Lost";
        }

        #endregion

        #region Public Methods


        override public string GetIconPath()
        {
            return @"RulesIcons/object-lost";
        }

        protected override bool OnRuleEnabled()
        {
			_trackerID = -1;
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Object);
            return true;
        }
		
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Object);
			_trackerID = -1;
		}

        override public string GetRuleDescription()
        {
            return "Fires whenever an object was previously detected and lost";
        }

        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Object))
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

			var tracker = SenseToolkitManager.Instance.SenseManager.QueryTracker();
            if (SenseToolkitManager.Instance.Initialized && tracker != null)
            {				
				if (_trackerID < 0 && Tracker2DPath != "" )
				{
					if (tracker.Set2DTrackFromFile(Tracker2DPath, out _trackerID) < pxcmStatus.PXCM_STATUS_NO_ERROR)
					{
						trigger.ErrorDetected = true;
		                Debug.LogError("Cannot set 2D image. Make sure it is a valid image (png)");
		                return false;
					}
					
				}
				
				PXCMTracker.TrackingValues trackingValues;
				
				int currentNumberOfObjects = tracker.QueryNumberTrackingValues();
				
				if (Tracker2DPath == "")
				{
					if ( currentNumberOfObjects < _numberOfDetectedObjects )
	                {						
						if (currentNumberOfObjects >= NumberOfObjects - 1)
						{
							if (currentNumberOfObjects == NumberOfObjects - 1 || RiseForEveryLostObject )
							{
	                    		success = true;
							}
						}
	                }
					
					_numberOfDetectedObjects = currentNumberOfObjects;
				}
				else
				{				
					// for now assuming there is maximum 1 object
				
					if ( tracker.QueryTrackingValues(_trackerID, out trackingValues) >= pxcmStatus.PXCM_STATUS_NO_ERROR && trackingValues.state == PXCMTracker.ETrackingState.ETS_TRACKING)
					{
						// here we would check how many we found...
						
						currentNumberOfObjects = 1;	
						success = false;
					}		
					else 
					{
						if (_numberOfDetectedObjects >= NumberOfObjects )
						{
							currentNumberOfObjects = 0;
							success = true;
						}
					}

				}	    
				
				_numberOfDetectedObjects = currentNumberOfObjects;
                     
            }
            return success;
        }

        #endregion
    }
}
