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
    /// Hand tracking rule - Processes Track Triggers
    /// </summary>
    [AddComponentMenu("")]
	[TrackTrigger.TrackTriggerAtt]
    public class HandTrackingRule : BaseRule
    {
        #region Public Fields

        /// <summary>
        /// Which hand to track
        /// </summary>
        public PXCMHandData.AccessOrderType WhichHand = PXCMHandData.AccessOrderType.ACCESS_ORDER_NEAR_TO_FAR;
		
		/// <summary>
		/// The index of the hand.
		/// </summary>
		public int HandIndex = 0;
		
		/// <summary>
		/// the index of the hand can change once more hands are visible. Should continuously track the detected hand no matter its' index
		/// </summary>
		public bool ContinuousTracking = false;
		
        /// <summary>
        /// The tracked joint.
        /// </summary>
        public PXCMHandData.JointType TrackedJoint = PXCMHandData.JointType.JOINT_WRIST;

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
		
		private int _uniqueID = -1;
        private Quaternion _zInvert = new Quaternion(0, 0, 1, 0f);
        private Quaternion _yInvert = new Quaternion(0, 1, 0, 0f);
		
        #endregion

        #region C'tors

        public HandTrackingRule(): base()
        {
            FriendlyName = "Hand Tracking";
        }

        #endregion

        #region Public Methods


        override public string GetIconPath()
        {
            return @"RulesIcons/hand-tracking";
        }

        override public string GetRuleDescription()
        {
            return "Track hand joint's position and orientation";
        }

        protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Hand);
            return true;
        }
			
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Hand);
		}
		
		
		
        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Hand))
            {
                trigger.ErrorDetected = true;
                Debug.LogError("Hand Analysis Module Not Set");
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


            if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.HandDataOutput != null)
            {

                PXCMHandData.IHand data = null;

                if (SenseToolkitManager.Instance.HandDataOutput.QueryNumberOfHands() > 0 && 
					((ContinuousTracking && SenseToolkitManager.Instance.HandDataOutput.QueryHandDataById (_uniqueID, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
					||
					SenseToolkitManager.Instance.HandDataOutput.QueryHandData(WhichHand, HandIndex, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
					)									
                {	
					
                    // Process Tracking
                    
					_uniqueID = data.QueryUniqueId();
                    PXCMHandData.JointData jointData;

                    data.QueryTrackedJoint(TrackedJoint, out jointData);

                    TrackTrigger specificTrigger = (TrackTrigger)trigger;
					
					PXCMPoint3DF32 point = jointData.positionWorld;
                    Vector3 position = new Vector3(point.x, point.y, point.z);

                    position.x *= -100;
                    position.y *= 100;
                    position.z *= 100;
					
					if ( position.x + position.y + position.z == 0)
					{
						return false;
					}

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

                    Quaternion q = new Quaternion(jointData.globalOrientation.x, jointData.globalOrientation.y,
                        jointData.globalOrientation.z, jointData.globalOrientation.w);

                    q = q * _zInvert * _yInvert;

                    specificTrigger.RotationQuaternion = q;

                    success = true;
                    
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