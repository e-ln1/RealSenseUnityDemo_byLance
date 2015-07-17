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
    /// Blob lost rule - Process Event trigger
    /// </summary>	
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class BlobLostRule : BaseRule
    {
        #region Public Fields
		
		/// <summary>
        /// The real world box center. In centimiters.
        /// </summary>
        public Vector3 RealWorldBoxCenter = new Vector3(0, 0, 50);

        /// <summary>
        /// The real world box dimensions. In Centimiters.
        /// </summary>
        public Vector3 RealWorldBoxDimensions = new Vector3(100, 100, 100);
	
		
		public BlobUtilityClass.TrackingBlobPoint BlobPointToTrack = BlobUtilityClass.TrackingBlobPoint.ClosestPoint;
		

       	public float MaxDistance = 50;
		
		public int NumberOfBlobs = 1;
		
        #endregion

        #region Private Fields

        /// <summary>
        /// Store if in the last frame a blob was detected.
        /// </summary>
        private bool _lastFrameDetected = false;
		
				
		private float[] _depthArray;
		private PXCMPoint3DF32[] _pos_uvz;
     	private PXCMPoint3DF32[] _pos3d;
		
        #endregion

        #region C'tor

        public BlobLostRule(): base()
        {
            FriendlyName = "Blob Lost";
        }

        #endregion

        #region Public Methods

        override public string GetIconPath()
        {
            return @"RulesIcons/object-lost";
        }

        override public string GetRuleDescription()
        {
            return "Fires whenever a blob is previously detected and then lost";
        }

        protected override bool OnRuleEnabled()
        {
            SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.VideoDepthStream);
            return true;
        }
		
		protected override void OnRuleDisabled()
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.VideoDepthStream);
		}
		
        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            if (!SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.VideoDepthStream))
            {
                trigger.ErrorDetected = true;
                Debug.LogError("Depth Stream Not Set");
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
			
			
            if (SenseToolkitManager.Instance.Initialized && SenseToolkitManager.Instance.BlobExtractor != null && SenseToolkitManager.Instance.ImageDepthOutput != null)
            {	
				// Setting max distance for this rule and process the image
				SenseToolkitManager.Instance.BlobExtractor.SetMaxDistance(MaxDistance * 10);
				
				var sts = SenseToolkitManager.Instance.BlobExtractor.ProcessImage(SenseToolkitManager.Instance.ImageDepthOutput);				
				
                if (sts == pxcmStatus.PXCM_STATUS_NO_ERROR && SenseToolkitManager.Instance.BlobExtractor.QueryNumberOfBlobs()  >= NumberOfBlobs)						
				{					
					int numberOfBlobsDetected = SenseToolkitManager.Instance.BlobExtractor.QueryNumberOfBlobs();
					
					int blobsRightlyDetected = 0;
				
					PXCMBlobExtractor.BlobData blobData = new PXCMBlobExtractor.BlobData();
					
					// For each detected blob, project the tracked point to the real world and 
					//  check that it is in our real world box
					for (int i = 0; i < numberOfBlobsDetected; i++)
					{
						PXCMImage.ImageInfo info = SenseToolkitManager.Instance.ImageDepthOutput.QueryInfo();
						info.format = PXCMImage.PixelFormat.PIXEL_FORMAT_Y8;
						PXCMImage new_image = SenseToolkitManager.Instance.SenseManager.session.CreateImage(info);
						
						// Process Tracking                    
						SenseToolkitManager.Instance.BlobExtractor.QueryBlobData(i, new_image, out blobData);					
						
						new_image.Dispose();
						
						PXCMPointI32 trackedPoint = BlobUtilityClass.GetTrackedPoint(blobData, BlobPointToTrack);
						
						PXCMImage.ImageData data;
						SenseToolkitManager.Instance.ImageDepthOutput.AcquireAccess(PXCMImage.Access.ACCESS_READ,PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH_F32, out data);
					
						if (_depthArray == null)
						{
							_depthArray = new float[SenseToolkitManager.Instance.ImageDepthOutput.info.width * SenseToolkitManager.Instance.ImageDepthOutput.info.height];
						}
						data.ToFloatArray(0, _depthArray);
						
						float depth = _depthArray[(int)trackedPoint.x  +  (int)trackedPoint.y * SenseToolkitManager.Instance.ImageDepthOutput.info.width];
						
						if (_pos_uvz == null )
						{
							_pos_uvz = new PXCMPoint3DF32[1]{new PXCMPoint3DF32()};
						}
						_pos_uvz[0].x = trackedPoint.x;
						_pos_uvz[0].y = trackedPoint.y;
						_pos_uvz[0].z = depth;
						
						if (_pos3d == null)
						{
							_pos3d = new PXCMPoint3DF32[1]{new PXCMPoint3DF32()};
						}
						
						SenseToolkitManager.Instance.Projection.ProjectDepthToCamera(_pos_uvz, _pos3d);
						
						Vector3 position = new Vector3();
	                    position.x = -_pos3d[0].x/10;
	                    position.y = _pos3d[0].y/10 ;
	                    position.z = _pos3d[0].z/10 ;
						
						SenseToolkitManager.Instance.ImageDepthOutput.ReleaseAccess(data);
	
	                    TrackingUtilityClass.ClampToRealWorldInputBox(ref position, RealWorldBoxCenter, RealWorldBoxDimensions);
	                    TrackingUtilityClass.Normalize(ref position, RealWorldBoxCenter, RealWorldBoxDimensions);
	
						if (!float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z))
	                    {
	                    	if (position.x > 0 && position.x < 1 &&
								position.y > 0 && position.y < 1 &&
								position.z > 0 && position.z < 1)
							{
								blobsRightlyDetected++;
								
								if (blobsRightlyDetected == NumberOfBlobs)
			                    {
			                       break;							
			                    }			                    
							}							
						}
						else
						{
							_lastFrameDetected = false;
							return false;
						} 
					}
					
					if (blobsRightlyDetected < NumberOfBlobs) 
                    {
						if (_lastFrameDetected)
						{
                        	success = true;
						}
						_lastFrameDetected = false;
                    }
					else 
					{
						_lastFrameDetected = true;
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
            else
            {
                return false;
            }

            return success;

        }

        #endregion


    }

}