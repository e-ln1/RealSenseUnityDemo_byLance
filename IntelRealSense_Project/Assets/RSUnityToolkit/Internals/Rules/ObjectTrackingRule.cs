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
    /// Object tracking rule - Processes Track Triggers
    /// </summary>
    [AddComponentMenu("")]
	[TrackTrigger.TrackTriggerAtt]
    public class ObjectTrackingRule : BaseRule
    {
        #region Public Fields
		
		/// <summary>
		/// The path to the image
		/// </summary>
      	public string Tracker2DPath = "";
		
		public float WidthInMM = 0;
		public float HeightInMM = 0;
		public bool IsExtensible = true;
		public float QualityThershold = 0;
		
        /// <summary>
        /// The real world box center. In centimiters.
        /// </summary>
        public Vector3 RealWorldBoxCenter = new Vector3(0, 0, 50);

        /// <summary>
        /// The real world box dimensions. In Centimiters.
        /// </summary>
        public Vector3 RealWorldBoxDimensions = new Vector3(100, 100, 100);

        #endregion

        #region Private Fields

		private int _trackerID  = -1;
		
        #endregion		
		
        #region C'tors

        public ObjectTrackingRule(): base()
        {
            FriendlyName = "Object Tracking";
        }

        #endregion

        #region Public Methods
		
        override public string GetIconPath()
        {
            return @"RulesIcons/object-tracking";
        }

        override public string GetRuleDescription()
        {
            return "Track an object's position and orientation";
        }

        protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Object);
            return true;
        }
			
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Object);
			_trackerID = -1;
		}
		
        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Object))
            {
                trigger.ErrorDetected = true;
                Debug.LogError("Object Tracking Module Not Set");
                return false;
            }

            if (!(trigger is TrackTrigger))
            {
                trigger.ErrorDetected = true;
                return false;
            }

            bool success = false;

            // make sure we have valid values

            if (RealWorldBoxDimensions.x <= 0)
            {
                RealWorldBoxDimensions.x = 1;
            }

            if (RealWorldBoxDimensions.y <= 0)
            {
                RealWorldBoxDimensions.y = 1;
            }

            if (RealWorldBoxDimensions.z <= 0)
            {
                RealWorldBoxDimensions.z = 1;
            }

			var tracker = SenseToolkitManager.Instance.SenseManager.QueryTracker();
            if (SenseToolkitManager.Instance.Initialized && tracker != null)
            {
				if (_trackerID < 0)
				{
					if (tracker.Set2DTrackFromFile(Tracker2DPath, out _trackerID, WidthInMM, HeightInMM, QualityThershold, IsExtensible) < pxcmStatus.PXCM_STATUS_NO_ERROR)
					{
						trigger.ErrorDetected = true;
		                Debug.LogError("Cannot set 2D image. Make sure it is a valid image (png)");
		                return false;
					}
					
				}
                PXCMTracker.TrackingValues trackingValues;

                if (tracker.QueryNumberTrackingValues() > 0)
                {
					if ( tracker.QueryTrackingValues(_trackerID, out trackingValues) < pxcmStatus.PXCM_STATUS_NO_ERROR || trackingValues.state != PXCMTracker.ETrackingState.ETS_TRACKING)
					{
						return false;
					}
                    // Process Tracking
                    if (trigger is TrackTrigger)
                    {

                        TrackTrigger specificTrigger = (TrackTrigger)trigger;
                        Vector3 position = new Vector3(trackingValues.translation.x, trackingValues.translation.y, trackingValues.translation.z);
					
                        position.x *= -0.1f;
                        position.y *= 0.1f;
                        position.z *= 0.1f;
						
                        TrackingUtilityClass.ClampToRealWorldInputBox(ref position, RealWorldBoxCenter, RealWorldBoxDimensions);
                        TrackingUtilityClass.Normalize(ref position, RealWorldBoxCenter, RealWorldBoxDimensions);
						
						if (!float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z))
                        {
                        	specificTrigger.Position = position;
						}
						else
						{
							return false;
						}

                        Quaternion q = new Quaternion(trackingValues.rotation.x, -trackingValues.rotation.y,
                            -trackingValues.rotation.z, trackingValues.rotation.w);

                        specificTrigger.RotationQuaternion = q;

                        success = true;
                    }
                }
            }
            else
            {
                return false;
            }

            return success;

        }

        #endregion
    }


}