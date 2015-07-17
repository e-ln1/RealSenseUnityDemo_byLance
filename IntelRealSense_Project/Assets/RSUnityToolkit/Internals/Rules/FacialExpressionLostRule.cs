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
    /// Facial Expression Lost rule - Process Event trigger
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]	
    public class FacialExpressionLostRule : BaseRule
    {
		
		#region Public fields
		
		/// <summary>
		/// The facial expression to detect
		/// </summary>
		public PXCMFaceData.ExpressionsData.FaceExpression FacialExpression = PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE;
		
		/// <summary>
		/// The Minimal intensity.
		/// </summary>
		public int FireOverIntensity = 30;
		
		/// <summary>
		/// The index of the face.
		/// </summary>
		public int FaceIndex = 0;
		
		/// <summary>
		/// Time lost threshold - after how many seconds fire the lost event
		/// </summary>
		public float LostThreshold = 0.75f;
		
		#endregion
		
		#region Private Fields
				
        private bool _lastFrameDetected = false;
		
		private float _lastTimeDetected = -1;

        #endregion
		
        #region C'tor
        public FacialExpressionLostRule(): base()
        {
            FriendlyName = "Facial Expression Lost";
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
            return @"RulesIcons/face-lost";
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
	        			_lastFrameDetected = true;	                                    																	
						_lastTimeDetected = Time.timeSinceLevelLoad;
                    	return false;					
					}
				}
            }
            else
            {
                success = false;
            }  
			
			if (Time.timeSinceLevelLoad - _lastTimeDetected > LostThreshold) 
			{
				if (_lastFrameDetected)
            	{
                    _lastFrameDetected = false;
                    success = true;
	            }
			}        
			
            return success;
        }

        #endregion
    }
}
