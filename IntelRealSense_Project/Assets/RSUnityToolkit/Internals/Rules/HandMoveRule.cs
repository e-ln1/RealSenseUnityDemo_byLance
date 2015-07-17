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
    /// Hand Move rule - Processes Translation Triggers
    /// </summary>
    [AddComponentMenu("")]
	[TranslationTrigger.TranslationTriggerAtt]
    public class HandMoveRule : BaseRule
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
				
        public Vector3 Thresholds = new Vector3(0, 0, 0);

        #endregion

        #region Private Fields

        private Vector3 _referencePosition = new Vector3();
		private int _uniqueID = -1;
		
        #endregion

        #region C'tors

        public HandMoveRule(): base()
        {
            FriendlyName = "Hand Move";
        }

        #endregion

        #region Public Methods


        override public string GetIconPath()
        {
            return @"RulesIcons/hand-tracking";
        }

        override public string GetRuleDescription()
        {
            return "Fires whenever a hand is moved";
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
                Debug.LogError("Hand Analysis Module Not Set");
                trigger.ErrorDetected = true;
                return false;
            }

            if (!(trigger is TranslationTrigger))
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

                if (SenseToolkitManager.Instance.HandDataOutput.QueryNumberOfHands() > 0 
					&& 
					(
					(ContinuousTracking && SenseToolkitManager.Instance.HandDataOutput.QueryHandDataById (_uniqueID, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
					||
					SenseToolkitManager.Instance.HandDataOutput.QueryHandData(WhichHand, HandIndex, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
					)
                {
					_uniqueID = data.QueryUniqueId();    

                    // Process Translation
                    if (trigger is TranslationTrigger)
                    {
                        TranslationTrigger specificTrigger = (TranslationTrigger)trigger;

                        PXCMHandData.JointData jointData;
                        data.QueryTrackedJoint(TrackedJoint, out jointData);

                        Vector3 vecPos = new Vector3(-(jointData.positionWorld.x), (jointData.positionWorld.y), jointData.positionWorld.z);

                        vecPos.x *= 100;
                        vecPos.y *= 100;
                        vecPos.z *= 100;

                        TrackingUtilityClass.ClampToRealWorldInputBox(ref vecPos, RealWorldBoxCenter, RealWorldBoxDimensions);
                        TrackingUtilityClass.Normalize(ref vecPos, RealWorldBoxCenter, RealWorldBoxDimensions);

                        if (specificTrigger.Restart)
                        {
                            specificTrigger.Restart = false;
                            _referencePosition = vecPos;
                            return false;
                        }

                        specificTrigger.Translation = vecPos - _referencePosition;

                        Vector3 passCriteria = new Vector3();
                        passCriteria.x = Mathf.Abs(vecPos.x) - Mathf.Abs(Thresholds.x);
                        passCriteria.y = Mathf.Abs(vecPos.y) - Mathf.Abs(Thresholds.y);
                        passCriteria.z = Mathf.Abs(vecPos.z) - Mathf.Abs(Thresholds.z);

                        if (passCriteria.x > 0 || passCriteria.y > 0 || passCriteria.z > 0)
                        {
                            _referencePosition = vecPos;

                            success = true;
                        }
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