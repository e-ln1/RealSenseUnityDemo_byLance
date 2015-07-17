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
    /// Gesture Deteted rule: Implements Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class GestureDetectedRule : BaseRule
    {
        #region Public Fields
		
		/// <summary>
		/// The gesture to track
		/// </summary>
        public MCTTypes.RSUnityToolkitGestures Gesture;

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
		
        #endregion

        #region C'tor

        public GestureDetectedRule(): base()
        {
            FriendlyName = "Gesture Detected";
        }

        #endregion

        #region Private Fields
		
		private int _uniqueID = -1;
        private bool _lastFrameDetected = false;
		
        #endregion

        #region Public Methods

        override public string GetIconPath()
        {
            return @"RulesIcons/gesture-detected";
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
            return "Fires upon gesture recognition";
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

            //AcquireFrame
            if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.HandDataOutput != null)
            {

                if (SenseToolkitManager.Instance.HandDataOutput.QueryNumberOfHands() > 0)
                {

                    int totalNumOfFiredGestures = SenseToolkitManager.Instance.HandDataOutput.QueryFiredGesturesNumber();
                    PXCMHandData.GestureData gestureData;

                    for (int i = 0; i < totalNumOfFiredGestures; i++)
                    {
                        if (SenseToolkitManager.Instance.HandDataOutput.QueryFiredGestureData(i, out gestureData) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                        {
							
							PXCMHandData.IHand handData = null;
                			if ((ContinuousTracking && SenseToolkitManager.Instance.HandDataOutput.QueryHandDataById (_uniqueID, out handData) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
									||
									SenseToolkitManager.Instance.HandDataOutput.QueryHandData(WhichHand, HandIndex, out handData) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
			                {
								_uniqueID = handData.QueryUniqueId();
								if (handData.QueryUniqueId() == gestureData.handId)
								{
									MCTTypes.RSUnityToolkitGestures firedGesture = MCTTypes.GetGesture(gestureData.name);
									                           
	                                if (((!Gesture.Equals(MCTTypes.RSUnityToolkitGestures.None)) && Gesture.Equals(firedGesture)))
	                                {						
	                                    if (!_lastFrameDetected)
	                                    {
	                                        _lastFrameDetected = true;
	                                        return true;
	                                    }
	                                    else
	                                    {
	                                        return false;
	                                    }
	                                }		                            
								}															
							}  
                        }
                    }

                    _lastFrameDetected = false;
                }
            }
            return false;
        }
        #endregion
    }
}