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
    /// Facial Expression Detected rule - Process Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]	
    public class FacialExpressionDetectedRule : BaseRule
    {	
		#region Public fields
		
		/// <summary>
		/// The facial expression to detect.
		/// </summary>
		public PXCMFaceData.ExpressionsData.FaceExpression FacialExpression = PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE;
		
		/// <summary>
		/// Minimal Intensity
		/// </summary>
		public int FireOverIntensity = 30;
		
		/// <summary>
		/// The index of the face.
		/// </summary>
		public int FaceIndex = 0;
		
		#endregion
		
        #region C'tor
        public FacialExpressionDetectedRule(): base()
        {
            FriendlyName = "Facial Expression Detected";
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
            return "Fires whenever a facial expressions is detected";
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
			
            trigger.Source = this.name;            
            if (SenseToolkitManager.Instance.Initialized
                &&
                SenseToolkitManager.Instance.FaceModuleOutput != null)
            {
				int currentNumberOfFaces = SenseToolkitManager.Instance.FaceModuleOutput.QueryNumberOfDetectedFaces();
				
				if (currentNumberOfFaces > FaceIndex) {
					PXCMFaceData.Face face = SenseToolkitManager.Instance.FaceModuleOutput.QueryFaceByIndex(FaceIndex);
					PXCMFaceData.ExpressionsData expressionData = face.QueryExpressions();
					PXCMFaceData.ExpressionsData.FaceExpressionResult faceExpressionResult;
					expressionData.QueryExpression(FacialExpression, out faceExpressionResult);
					
					if (faceExpressionResult.intensity >= FireOverIntensity) {
	        			success = true;						
					}
				}
            }
            else
            {
                success = false;
            }            
			
            return success;
        }

        #endregion
    }
}
