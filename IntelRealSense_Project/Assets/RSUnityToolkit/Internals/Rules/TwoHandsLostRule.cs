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

namespace RSUnityToolkit
{
    /// <summary>
    /// Two hands lost rule: Implements event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class TwoHandsLostRule : BaseRule
    {
        #region Public Properties

        #endregion

        #region Private Properties

        private bool _twoHandsDetected = false;

        #endregion

        #region C'tor

        public TwoHandsLostRule(): base()
        {
            FriendlyName = "Two Hands Lost";
        }

        #endregion

        #region Public Methods

        protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Hand);
            return true;
        }
		
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Hand);
		}
		
		override public string GetIconPath()
		{
			return @"RulesIcons/two-hands-lost";
		}
		
        public override string GetRuleDescription()
        {
            return "Fires whenever two hands were previously detected and lost";
        }

        public override bool Process(Trigger trigger)
        {
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

            if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.HandDataOutput != null)
            {
                if (SenseToolkitManager.Instance.HandDataOutput.QueryNumberOfHands() > 1)
                {
                    PXCMHandData.IHand leftHand = null;
                    PXCMHandData.IHand rightHand = null;


                    //Query both hands
                    if (SenseToolkitManager.Instance.HandDataOutput.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_LEFT_HANDS, 0, out leftHand) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        if (SenseToolkitManager.Instance.HandDataOutput.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_RIGHT_HANDS, 0, out rightHand) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                        {
                            if (!_twoHandsDetected)
                            {
                                _twoHandsDetected = true;
                            }
                            return false;
                        }
                    }
                }
            }


            if (_twoHandsDetected)
            {
                _twoHandsDetected = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}