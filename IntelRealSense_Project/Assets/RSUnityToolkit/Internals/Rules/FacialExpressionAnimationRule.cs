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
    /// Facial Expression rule - Process Animation trigger
    /// </summary>
    [AddComponentMenu("")]
	[AnimationTrigger.AnimationTriggerAtt]
    public class FacialExpressionAnimationRule : BaseRule
    {
		#region Constants
		
		private const float OPTIMAL_RELATIVE_FACE_HEIGHT = 55.5f;

		private const float OPTIMAL_EYEBROW_UP_MAX_DISTANCE = 40f;
		private const float EYEBROW_UP_INITIAL_DISTANCE = 23f;
		
		private const float OPTIMAL_EYE_CLOSE_MAX_DISTANCE = 4.5f;
		private const float EYE_CLOSE_INITIAL_DISTANCE = 10f;
		
		private const float OPTIMAL_MOUTH_OPEN_MAX_DISTANCE = 41f;
		private const float MOUTH_OPEN_INITIAL_DISTANCE = 8f;
		
		private const float NORMALIZE_MAX_FACIAL_EXPRESSION_VALUE = 100f;

		#endregion

		#region Public Fields

		public string[] AvailableExpressions;

		#endregion
		
        #region Private Fields

        #endregion

        #region C'tor
        public FacialExpressionAnimationRule(): base()
        {
            FriendlyName = "Facial Expression Animation";


			int numOfExpressions = System.Enum.GetValues(typeof(PXCMFaceData.ExpressionsData.FaceExpression)).Length;
			AvailableExpressions = new string[numOfExpressions];

			int i = 0;;
			foreach(PXCMFaceData.ExpressionsData.FaceExpression t in System.Enum.GetValues(typeof(PXCMFaceData.ExpressionsData.FaceExpression)))
			{
				AvailableExpressions[i] = t.ToString();
				i++;
			}
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
            return "Fires whenever a face expression is detected";
        }

        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Face))
            {
                trigger.ErrorDetected = true;
                return false;
            }

			AnimationTrigger animationTrigger = trigger as AnimationTrigger;
			if (animationTrigger == null)
            {
                trigger.ErrorDetected = true;
                return false;
            }

            bool success = false;

            if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.FaceModuleOutput != null)
            {
				success = UpdateFace(animationTrigger);
            }
            else
            {
                success = false;
            }

        	return success;
    	}
		
		#endregion
		
		#region Private Methods
		
		private bool UpdateFace(AnimationTrigger animationTrigger)
		{
			animationTrigger.IsAnimationDataValid = false;

			// Get the closest face
			PXCMFaceData.Face pxcmFace = SenseToolkitManager.Instance.FaceModuleOutput.QueryFaceByIndex(0);
			if (SenseToolkitManager.Instance.FaceModuleOutput.QueryNumberOfDetectedFaces() <= 0)
			{
				// PXCMFaceData.QueryFaceByIndex(0) failed.
				return false;
			}
			
			PXCMFaceData.LandmarksData pxcmLandmarksData = pxcmFace.QueryLandmarks();
			if (pxcmLandmarksData == null)
			{
				// PXCMFaceData.Face.QueryLandmarks() failed.
				return false;
			}
			
			Dictionary<PXCMFaceData.LandmarkType, PXCMFaceData.LandmarkPoint> landmarkPoints = new Dictionary<PXCMFaceData.LandmarkType, PXCMFaceData.LandmarkPoint>();
			PXCMFaceData.LandmarkType[] landmarkTypes = new PXCMFaceData.LandmarkType[] 
				{
					PXCMFaceData.LandmarkType.LANDMARK_NOSE_TIP,
					PXCMFaceData.LandmarkType.LANDMARK_NOSE_TOP,
					PXCMFaceData.LandmarkType.LANDMARK_EYEBROW_LEFT_CENTER,
					PXCMFaceData.LandmarkType.LANDMARK_EYE_LEFT_CENTER,
					PXCMFaceData.LandmarkType.LANDMARK_EYEBROW_RIGHT_CENTER,
					PXCMFaceData.LandmarkType.LANDMARK_EYE_RIGHT_CENTER,
					PXCMFaceData.LandmarkType.LANDMARK_UPPER_LIP_CENTER,
					PXCMFaceData.LandmarkType.LANDMARK_LOWER_LIP_CENTER,
					PXCMFaceData.LandmarkType.LANDMARK_EYELID_LEFT_TOP,
					PXCMFaceData.LandmarkType.LANDMARK_EYELID_LEFT_BOTTOM,
					PXCMFaceData.LandmarkType.LANDMARK_EYELID_RIGHT_TOP,
					PXCMFaceData.LandmarkType.LANDMARK_EYELID_RIGHT_BOTTOM
				};

			foreach (PXCMFaceData.LandmarkType landmarkType in landmarkTypes)
			{
				PXCMFaceData.LandmarkPoint landmarkPoint = GetLandmarkPoint(pxcmLandmarksData, landmarkType);

				if (landmarkPoint == null)
				{
					// PXCMFaceData.LandmarksData.QueryPointIndex() failed.
					return false;
				}

				landmarkPoints.Add(landmarkType, landmarkPoint);
			}

			PXCMFaceData.ExpressionsData pxcmExpressionsData = pxcmFace.QueryExpressions();
			if (pxcmExpressionsData == null)
			{
				// PXCMFaceData.Face.QueryExpressions() failed.
				return false;
			}
			
			animationTrigger.IsAnimationDataValid = true;

			PXCMCapture.Device.MirrorMode mirrorMode = PXCMCapture.Device.MirrorMode.MIRROR_MODE_DISABLED;

			PXCMCaptureManager pxcmCaptureManager = SenseToolkitManager.Instance.SenseManager.QueryCaptureManager();
			if (pxcmCaptureManager != null)
			{
				PXCMCapture.Device pxcmCaptureDevice = pxcmCaptureManager.QueryDevice();
				if (pxcmCaptureDevice != null)
				{
					mirrorMode = pxcmCaptureDevice.QueryMirrorMode();
				}
			}
			
			animationTrigger.Animations.Clear();

			float faceHeight = landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_NOSE_TIP].image.y - landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_NOSE_TOP].image.y;

			float leftEyebrowUp = GetNormalizedLandmarksDistance(landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_EYE_LEFT_CENTER].image.y, landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_EYEBROW_LEFT_CENTER].image.y, OPTIMAL_RELATIVE_FACE_HEIGHT, faceHeight, EYEBROW_UP_INITIAL_DISTANCE, OPTIMAL_EYEBROW_UP_MAX_DISTANCE, NORMALIZE_MAX_FACIAL_EXPRESSION_VALUE, true);
			float rightEyebrowUp = GetNormalizedLandmarksDistance(landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_EYE_RIGHT_CENTER].image.y, landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_EYEBROW_RIGHT_CENTER].image.y, OPTIMAL_RELATIVE_FACE_HEIGHT, faceHeight, EYEBROW_UP_INITIAL_DISTANCE, OPTIMAL_EYEBROW_UP_MAX_DISTANCE, NORMALIZE_MAX_FACIAL_EXPRESSION_VALUE, true);
			
			float leftEyeClose = GetNormalizedLandmarksDistance(landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_EYELID_LEFT_BOTTOM].image.y, landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_EYELID_LEFT_TOP].image.y, OPTIMAL_RELATIVE_FACE_HEIGHT, faceHeight, EYE_CLOSE_INITIAL_DISTANCE, OPTIMAL_EYE_CLOSE_MAX_DISTANCE, NORMALIZE_MAX_FACIAL_EXPRESSION_VALUE, true);
			float rightEyeClose = GetNormalizedLandmarksDistance(landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_EYELID_RIGHT_BOTTOM].image.y, landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_EYELID_RIGHT_TOP].image.y, OPTIMAL_RELATIVE_FACE_HEIGHT, faceHeight, EYE_CLOSE_INITIAL_DISTANCE, OPTIMAL_EYE_CLOSE_MAX_DISTANCE, NORMALIZE_MAX_FACIAL_EXPRESSION_VALUE, true);

			if (mirrorMode == PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL)
			{
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_BROW_RAISER_LEFT.ToString(), rightEyebrowUp);
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_BROW_RAISER_RIGHT.ToString(), leftEyebrowUp);
				
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_CLOSED_LEFT.ToString(), rightEyeClose);
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_CLOSED_RIGHT.ToString(), leftEyeClose);
			}
			else
			{
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_BROW_RAISER_LEFT.ToString(), leftEyebrowUp);
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_BROW_RAISER_RIGHT.ToString(), rightEyebrowUp);
				
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_CLOSED_LEFT.ToString(), leftEyeClose);
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_CLOSED_RIGHT.ToString(), rightEyeClose);
			}

			// Instead of LANDMARK_LOWER_LIP_CENTER, we need landmark 51 (lower lip upper center)
			// Instead of LANDMARK_UPPER_LIP_CENTER, we need landmark 47 (upper lip lower center)
			animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_MOUTH_OPEN.ToString(), GetNormalizedLandmarksDistance(landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_LOWER_LIP_CENTER].image.y, landmarkPoints[PXCMFaceData.LandmarkType.LANDMARK_UPPER_LIP_CENTER].image.y, OPTIMAL_RELATIVE_FACE_HEIGHT, faceHeight, MOUTH_OPEN_INITIAL_DISTANCE, OPTIMAL_MOUTH_OPEN_MAX_DISTANCE, NORMALIZE_MAX_FACIAL_EXPRESSION_VALUE, true));

			PXCMFaceData.ExpressionsData.FaceExpressionResult pxcmFaceExpressionResult = new PXCMFaceData.ExpressionsData.FaceExpressionResult();
			
			if (pxcmExpressionsData.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, out pxcmFaceExpressionResult))
			{
				animationTrigger.Animations.Add(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE.ToString(), (float)pxcmFaceExpressionResult.intensity);
			}
			else
			{
				// Error querying expression: EXPRESSION_SMILE.
				return false;
			}

			return true;
		}

		private PXCMFaceData.LandmarkPoint GetLandmarkPoint(PXCMFaceData.LandmarksData pxcmLandmarksData, PXCMFaceData.LandmarkType landmarkType)
		{
			PXCMFaceData.LandmarkPoint landmarkPoint = null;
			pxcmLandmarksData.QueryPoint(pxcmLandmarksData.QueryPointIndex(landmarkType), out landmarkPoint);
			return landmarkPoint;
		}

		private float GetNormalizedLandmarksDistance(float x1, float x2, float optimalFaceDistance, float faceDistance, float initialDistance, float optimalMaxDistance, float maxNormalizedValue, bool clampMinMax)
		{
			float normalizedValue = ((((x1 - x2) * optimalFaceDistance / faceDistance) - initialDistance) / (optimalMaxDistance - initialDistance) * maxNormalizedValue);
			if (clampMinMax) 
			{
				normalizedValue = Mathf.Clamp(normalizedValue, 0, maxNormalizedValue);
			}
			
			return normalizedValue;
		}

		#endregion
	}
}
