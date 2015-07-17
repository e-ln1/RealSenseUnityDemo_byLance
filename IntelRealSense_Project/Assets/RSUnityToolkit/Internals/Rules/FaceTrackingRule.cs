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
    /// Face tracking rule - Process Track triggers
    /// </summary>
    [TrackTrigger.TrackTriggerAtt]
    [AddComponentMenu("")]
	public class FaceTrackingRule : BaseRule
    {
        #region Public Fields

        /// <summary>
        /// The landmark to track. (in the group)
        /// </summary>
        public PXCMFaceData.LandmarkType LandmarkToTrack = PXCMFaceData.LandmarkType.LANDMARK_NOSE_TIP;

        /// <summary>
        /// The real world box center. In centimiters.
        /// </summary>
        public Vector3 RealWorldBoxCenter = new Vector3(0, 0, 50);

        /// <summary>
        /// The real world box dimensions. In Centimiters.
        /// </summary>
        public Vector3 RealWorldBoxDimensions = new Vector3(100, 100, 100);
		
		/// <summary>
		/// The index of the face.
		/// </summary>
		public int FaceIndex = 0;
		
        #endregion

        #region Private Fields

        #endregion

        #region C'tors
        public FaceTrackingRule(): base()
        {
            FriendlyName = "Face Tracking";
        }
        #endregion

        #region Public Methods

        override public string GetIconPath()
        {
            return @"RulesIcons/face-tracking";
        }

        protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Face);
            return true;
        }
		
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.Face);
		}

        override public string GetRuleDescription()
        {
            return "Tracks face landmark's position and orientation";
        }

        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;

            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.Face))
            {
                trigger.ErrorDetected = true;
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

            if (SenseToolkitManager.Instance.Initialized
                    &&
                    SenseToolkitManager.Instance.FaceModuleOutput != null)
            {

				if (SenseToolkitManager.Instance.FaceModuleOutput.QueryNumberOfDetectedFaces() == 0)
				{
					return false;					
				}
                
				PXCMFaceData.Face singleFaceOutput = null; 

                singleFaceOutput = SenseToolkitManager.Instance.FaceModuleOutput.QueryFaceByIndex(FaceIndex);
				
								
                if (singleFaceOutput != null && singleFaceOutput.QueryUserID() >= 0)
                {				
                    // Process Tracking
                    if (trigger is TrackTrigger)
                    {
                        TrackTrigger specificTrigger = (TrackTrigger)trigger;

                        var landmarksData = singleFaceOutput.QueryLandmarks();
						if (landmarksData == null)
						{
							return false;
						}
						
                        int landmarkId = landmarksData.QueryPointIndex(LandmarkToTrack);

                        PXCMFaceData.LandmarkPoint point = null;

                        landmarksData.QueryPoint(landmarkId, out point);

                        // Translation
                        if (point != null)
                        {
                            Vector3 vec = new Vector3();							
                            vec.x = point.world.x * 100;
                            vec.y = point.world.y * 100;
                            vec.z = point.world.z * 100;					
								
							if ( vec.x + vec.y + vec.z == 0)
							{
								return false;
							}
								
                            // Clamp and normalize to the Real World Box
                            TrackingUtilityClass.ClampToRealWorldInputBox(ref vec, RealWorldBoxCenter, RealWorldBoxDimensions);
                            TrackingUtilityClass.Normalize(ref vec, RealWorldBoxCenter, RealWorldBoxDimensions);
							
							if (!float.IsNaN(vec.x) && !float.IsNaN(vec.y) && !float.IsNaN(vec.z))
                            {
                            	specificTrigger.Position = vec;			
								success = true;
							}						
							
                        }						

                        //Rotation
                        PXCMFaceData.PoseData poseData = singleFaceOutput.QueryPose();
                        if (poseData != null)
                        {
                            PXCMFaceData.PoseEulerAngles angles;                            
							if (poseData.QueryPoseAngles(out angles))
                            {
                                if (!float.IsNaN(angles.pitch) && !float.IsNaN(angles.yaw) && !float.IsNaN(angles.roll))
                                {
                                    Quaternion q = Quaternion.Euler(-angles.pitch, angles.yaw, -angles.roll);

                        			specificTrigger.RotationQuaternion = q;
		                        										
                                    success = true;
                                }                               
                            }

                        }
                    }

                }

                return success;
            }

            return success;

        }

        #endregion

    }
}
