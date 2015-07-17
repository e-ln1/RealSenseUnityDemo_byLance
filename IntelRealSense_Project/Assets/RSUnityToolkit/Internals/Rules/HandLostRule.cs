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

namespace RSUnityToolkit
{
    /// <summary>
    /// Hand lost rule - Process Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class HandLostRule : BaseRule
    {
        #region Public Fields

        /// <summary>
        /// Which hand to detect
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
        
		#endregion

        #region Private Fields

        /// <summary>
        /// Store if in the last frame a hand was detected.
        /// </summary>
        private bool _lastFrameDetected = false;
		private int _uniqueID = -1;
		
        #endregion

        #region C'tor

        public HandLostRule(): base()
        {
            FriendlyName = "Hand Lost";
        }

        #endregion

        #region Public Methods

        override public string GetIconPath()
        {
            return @"RulesIcons/hand-lost";
        }

        override public string GetRuleDescription()
        {
            return "Fires whenever a hand was previously detected and lost";
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

            if (!(trigger is EventTrigger))
            {
                trigger.ErrorDetected = true;
                return false;
            }


            EventTrigger specificTrigger = (EventTrigger)trigger;
            specificTrigger.Source = this.name;


            bool success = false;

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
                return false;
            }

            return success;

        }

        #endregion


    }

}