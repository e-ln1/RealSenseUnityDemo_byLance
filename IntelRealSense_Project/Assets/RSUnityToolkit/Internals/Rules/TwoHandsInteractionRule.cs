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
    /// Two hands interaction rule: Implements two hands interaction - scale and rotation
    /// </summary>
    [AddComponentMenu("")]
	[RotationTrigger.RotationTriggerAtt]
    [ScaleTrigger.ScaleTriggerAtt]
    public class TwoHandsInteractionRule : BaseRule
    {
        #region Public Properties

        /// <summary>
        /// [Currently not supported!] A minimum distance between hands/fingers to start rotation around the X axis. 
        /// </summary>
        public float DistanceThresholdX = 0f;

        /// <summary>
        /// A minimum distance between hands/fingers to start rotation around the Y axis. 
        /// </summary>
        public float DistanceThresholdY = 0f;

        /// <summary>
        /// A minimum distance between hands/fingers to start rotation around the Z axis. 
        /// </summary>
        public float DistanceThresholdZ = 0f;

        #endregion

        #region Private Properties

        private float _roll, _pitch, _yaw, _scale;

        #endregion

        #region C'tor

        public TwoHandsInteractionRule(): base()
        {
            FriendlyName = "Two Hands Scale and Rotate";
        }

        #endregion

        #region Public Methods

       	override public string GetIconPath()
		{
			return @"RulesIcons/two-hands-tracking";			
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
            return "Track the user's hands to calculate rotation values and scale value";
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

            if (!(trigger is RotationTrigger || trigger is ScaleTrigger))
            {
                trigger.ErrorDetected = true;
                return false;
            }

            #region Rotation
            //Rotation
            if (trigger is RotationTrigger)
            {
                //AcquireFrame
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
                                RotationTrigger rotationTrig = (RotationTrigger)trigger;
                                float yCurDiff = rightHand.QueryMassCenterWorld().y * 100 - leftHand.QueryMassCenterWorld().y * 100;
                                float zCurDiff = rightHand.QueryMassCenterWorld().z * 100 - leftHand.QueryMassCenterWorld().z * 100;

                                //Initialization
                                if (rotationTrig.Restart)
                                {
                                    _roll = yCurDiff;
                                    _pitch = (float)(Math.Atan2(yCurDiff, zCurDiff));
                                    _yaw = zCurDiff;
                                    rotationTrig.Restart = false;
                                }

                                rotationTrig.Roll = yCurDiff - _roll;
                                rotationTrig.Roll *= -1;
                                if (Math.Abs(rotationTrig.Roll) < DistanceThresholdX)
                                {
                                    rotationTrig.Roll = 0;
                                }

                                rotationTrig.Pitch = (float)(Math.Atan2(yCurDiff, zCurDiff) - _pitch);
                                if (Math.Abs(rotationTrig.Pitch) < DistanceThresholdZ)
                                {
                                    rotationTrig.Pitch = 0;
                                }

                                rotationTrig.Yaw = zCurDiff - _yaw;



                                if (Math.Abs(rotationTrig.Yaw) < DistanceThresholdY)
                                {
                                    rotationTrig.Yaw = 0;
                                }

                                if ((rotationTrig.Roll == rotationTrig.Yaw) && (rotationTrig.Yaw == rotationTrig.Pitch) && (rotationTrig.Pitch == 0))
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            #endregion

            #region Scale
            //Scale
            if (trigger is ScaleTrigger)
            {
                //AcquireFrame
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
                                ScaleTrigger scaleTrig = (ScaleTrigger)trigger;
                                float xCurDiff = rightHand.QueryMassCenterWorld().x * 100 - leftHand.QueryMassCenterWorld().x * 100;

                                //Initialization
                                if (scaleTrig.Restart)
                                {
                                    _scale = xCurDiff;
                                    scaleTrig.Restart = false;
                                    scaleTrig.Scale = 0;
                                }
                                else
                                {
                                    scaleTrig.Scale = _scale - xCurDiff;
                                }
                                if (Math.Abs(scaleTrig.Scale) < DistanceThresholdX)
                                {
                                    scaleTrig.Scale = 0;

                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            #endregion

            return false;
        }
        #endregion
    }
}