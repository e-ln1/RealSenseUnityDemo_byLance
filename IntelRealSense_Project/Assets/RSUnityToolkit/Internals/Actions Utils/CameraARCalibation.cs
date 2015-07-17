/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using System.Collections;
using RSUnityToolkit;

public class CameraARCalibation : MonoBehaviour
{
	
	#region Public Fields
	
	public Camera CameraObj = null;
	public float MaxDepthThreshold = 65;
	
	#endregion 
	
	#region Private Fields
	
	private bool init = false;
	private float[] _depthArray = null;
	
	#endregion
	
	// Use this for initialization
	void Start ()
	{
		if ( CameraObj == null )
		{
			CameraObj = this.gameObject.GetComponent<Camera>();	
		}
	}
	
	void OnEnable()
	{
		
		if (SenseToolkitManager.Instance != null)
		{
			SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.VideoDepthStream);
			SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.VideoColorStream);
			init = true;
		}
	
	}
	
	void OnDisable()
	{
		SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.VideoDepthStream);
		SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.VideoColorStream);
	}

	// Update is called once per frame
	void Update ()
	{
		if (!init)
		{
			OnEnable();			
		}
		
		if (SenseToolkitManager.Instance.Initialized &&
                    SenseToolkitManager.Instance.ImageRgbOutput != null && 
					SenseToolkitManager.Instance.ImageDepthOutput != null)
		{
			
			PXCMImage.ImageData depthData;
			int width = SenseToolkitManager.Instance.ImageDepthOutput.info.width;
			int height = SenseToolkitManager.Instance.ImageDepthOutput.info.height;
			
			// 1. Get the depth image
            SenseToolkitManager.Instance.ImageDepthOutput.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH_F32, out depthData);
			if (_depthArray == null)
			{
				_depthArray = new float[width * height];
			}
			depthData.ToFloatArray(0,_depthArray);	
			
			PXCMPoint3DF32[] pos_uvz = new PXCMPoint3DF32[1]{new PXCMPoint3DF32(){x = 0, y= 0 , z = 0}};			
			
			// 2. Find a "good" pixel (not too far)
			
			int x = width / 4;
			int y = height / 4;		
			int xx = 5;
			int yy = 5;
			while (pos_uvz[0].z == 0 || (pos_uvz[0].z / 10) > MaxDepthThreshold)
			{
				x += xx;
				if ( x >= 3 * width / 4 )
				{
					x = width / 4 ;
					y += yy;
					
					if (y >= 3 * height / 4)
					{
						SenseToolkitManager.Instance.ImageDepthOutput.ReleaseAccess(depthData);
						return;
					}
				}
				
				pos_uvz[0].x = x;
				pos_uvz[0].y = y;
				pos_uvz[0].z = _depthArray[x + y * width];
			}
			
			SenseToolkitManager.Instance.ImageDepthOutput.ReleaseAccess(depthData);
			
			// 3. Projet it to the color image
			
			PXCMPointF32[] pos_ij = new PXCMPointF32[1]{new PXCMPointF32()};
			
			var sts = SenseToolkitManager.Instance.Projection.MapDepthToColor( pos_uvz, pos_ij);
			if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
				return;
			}
			
			// 4. Project it to the Real World coordinates
			PXCMPoint3DF32[] pos3d = new PXCMPoint3DF32[1]{new PXCMPoint3DF32()};
			sts = SenseToolkitManager.Instance.Projection.ProjectDepthToCamera( pos_uvz, pos3d );
			
			pos3d[0].x /= -10;
			pos3d[0].y /= 10;
			pos3d[0].z /= 10;
			
            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR || pos3d[0].x == 0 || pos3d[0].y == 0)
            {
				return;
			}
			
			
            Vector3 vec = new Vector3();							
            vec.x = pos3d[0].x;
            vec.y = pos3d[0].y;
            vec.z = pos3d[0].z;					
			
			// 5. Normalize the color pixel location
			Vector3 vecScreen = new Vector3();
			vecScreen.x = (SenseToolkitManager.Instance.ImageRgbOutput.info.width - pos_ij[0].x) / SenseToolkitManager.Instance.ImageRgbOutput.info.width;
			vecScreen.y = (SenseToolkitManager.Instance.ImageRgbOutput.info.height - pos_ij[0].y) /  SenseToolkitManager.Instance.ImageRgbOutput.info.height;
				
			// 6. Calibrate the world point to the screen point			
			Calibrate(vec, vecScreen);
			            
        }
	
	}
	
	
	/// <summary>
	/// Calibrate the CameraObj so the worldPoint and the screenPoint are aligned
	/// </summary>
	/// <param name='worldPoint'>
	/// World point.
	/// </param>
	/// <param name='screenPoint'>
	/// Screen point.
	/// </param>
	public void Calibrate(Vector3 worldPoint, Vector2 screenPoint)
	{      	
		screenPoint.x = screenPoint.x * CameraObj.pixelWidth;
		screenPoint.y = screenPoint.y * CameraObj.pixelHeight;
		      
		Vector3 calculatedScreenPoint = CameraObj.WorldToScreenPoint(worldPoint);
		
		//Calibrate X axis		
		int safetyCounter = 0;		
		while (Mathf.Abs(calculatedScreenPoint.x - screenPoint.x) > 1f)
		{
			if (calculatedScreenPoint.x > screenPoint.x)
			{	
				Vector3 pos = CameraObj.transform.position;
				pos.x+=	1 - screenPoint.x/calculatedScreenPoint.x;
				CameraObj.transform.position = pos;
			}	
			else 
			{
				Vector3 pos = CameraObj.transform.position;
				pos.x-=	1 - calculatedScreenPoint.x/screenPoint.x;
				CameraObj.transform.position = pos;
			}
			calculatedScreenPoint = CameraObj.WorldToScreenPoint(worldPoint);
			
			safetyCounter++;			
			if (safetyCounter>100)
			{				
				return;
			}
		}
		
		//Calibrate Y axis		
		safetyCounter = 0;		
		while (Mathf.Abs(calculatedScreenPoint.y - screenPoint.y) > 1f)
		{
			if (calculatedScreenPoint.y > screenPoint.y)
			{				
				Vector3 pos = CameraObj.transform.position;
				pos.y+= 1 - screenPoint.y/calculatedScreenPoint.y;		
				CameraObj.transform.position = pos;
			}	
			else 
			{
				Vector3 pos = CameraObj.transform.position;
				pos.y-=	1 - calculatedScreenPoint.y/screenPoint.y;		
				CameraObj.transform.position = pos;
			}
			calculatedScreenPoint = CameraObj.WorldToScreenPoint(worldPoint);
			
			safetyCounter++;			
			if (safetyCounter>100)
			{
				return;
			}
		}
		
		// If we are done, disable this scrupt. No need to do this for every frame.
		if (Mathf.Abs(calculatedScreenPoint.x - screenPoint.x) < 1f)
		{
			if (Mathf.Abs(calculatedScreenPoint.y - screenPoint.y) < 1f)
			{
				this.enabled = false;
			}
		}

     return;
   }
}

