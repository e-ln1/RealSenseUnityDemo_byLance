/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;

public abstract class BlobUtilityClass
{
	
	public enum TrackingBlobPoint
	{
		RightPoint,
		LeftPoint,
		TopPoint,
		BottomPoint,
		CenterPoint,
		ClosestPoint
	}
	
	public static PXCMPointI32 GetTrackedPoint(PXCMBlobExtractor.BlobData blobData, BlobUtilityClass.TrackingBlobPoint blobPointToTrack)
		{
			PXCMPointI32 point = new PXCMPointI32();
			
			switch (blobPointToTrack)
			{
			case TrackingBlobPoint.BottomPoint:
				point = blobData.bottomPoint;
				break;
			case TrackingBlobPoint.TopPoint:
				point = blobData.topPoint;
				break;
			case TrackingBlobPoint.RightPoint:
				point = blobData.rightPoint;
				break;
			case TrackingBlobPoint.LeftPoint:
				point = blobData.leftPoint;
				break;
			case TrackingBlobPoint.CenterPoint:
				//converting to int
				point.x = (int)blobData.centerPoint.x;
				point.y = (int)blobData.centerPoint.y;
				break;
			case TrackingBlobPoint.ClosestPoint:
				point = blobData.closestPoint;
				break;
			}
			
			return point;
		}
	
}