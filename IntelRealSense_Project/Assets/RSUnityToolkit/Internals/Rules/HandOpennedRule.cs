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
    /// Hand Openned Rule: Implements Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class HandOpennedRule : BaseRule
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
		
		public float OpennessFactor = 50;
		
        #endregion

        #region C'tor

        public HandOpennedRule(): base()
        {
            FriendlyName = "Hand Openned";
        }

        #endregion

        #region Private Fields
		
		private int _uniqueID = -1;
        private bool _lastFrameDetected = false;
		
        #endregion

        #region Public Methods

        override public string GetIconPath()
        {
            return @"RulesIcons/gesture-lost";
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

        public override string GetRuleDescription()
        {
            return "Fires upon recognition of hand closed below the Openness factor";
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

            if (!(trigger is EventTrigger))
            {
                trigger.ErrorDetected = true;
                return false;
            }
			
			bool success = false;
			
            //AcquireFrame
            if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.HandDataOutput != null)
            {
                PXCMHandData.IHand data = null;
				
                if (SenseToolkitManager.Instance.HandDataOutput.QueryNumberOfHands() > 0 
					&& 
					(
					(ContinuousTracking && SenseToolkitManager.Instance.HandDataOutput.QueryHandDataById (_uniqueID, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
					||
					SenseToolkitManager.Instance.HandDataOutput.QueryHandData(WhichHand, HandIndex, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
					)
                {
					_uniqueID = data.QueryUniqueId();
					
					PXCMHandData.JointData jointD;
					data.QueryTrackedJoint(PXCMHandData.JointType.JOINT_PINKY_JT2, out jointD);
					
					if (jointD.confidence < 100)
					{
						return false;
					}
					
					if ( data.QueryOpenness() <= OpennessFactor )
					{	                   
	                    _lastFrameDetected = true;
					}
					else 
					{
						if (_lastFrameDetected)
	                    {
	                        success = true;
	                    }
	
	                    _lastFrameDetected = false;
					}
                }
                else
                {
                    if (_lastFrameDetected)
                    {
                        success = true;
                    }

                    _lastFrameDetected = false;
                }

            }
            return success;
        }
        #endregion
    }
}