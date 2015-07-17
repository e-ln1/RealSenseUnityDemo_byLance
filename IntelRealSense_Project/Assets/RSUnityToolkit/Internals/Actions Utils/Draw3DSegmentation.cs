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

/// <summary>
/// This class draw the video feed of the selected image type on the associated game object
/// </summary>
public class Draw3DSegmentation : MonoBehaviour {	
	
	#region Public Fields
		
	#endregion
	
	#region Private Fields
	
	private PXCMSizeI32 size=new PXCMSizeI32();
	
	private Texture2D _texture=null;	
	
	#endregion
	
	#region Private methods
	
	/// <summary>
	/// Sets the sense option according to the Stream field
	/// </summary>
	private void SetSenseOptions()
	{		
		SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.VideoSegmentation);
	}
	
	/// <summary>
	/// Unsets the sense option according to the Stream field
	/// </summary>
	private void UnsetSenseOptions()
	{		
		SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.VideoSegmentation);	
	}
	
	/// <summary>
	/// Sets the texture for the associated game object with the given image
	/// </summary>
	/// <param name='image'>
	/// Image.
	/// </param>
	private void SetTexture(PXCMImage image) 
    {
		if (image == null)
		{
			return;
		}
		
		if (_texture==null) {
			/* Save size and preallocate the Texture2D */
			size.width=image.info.width;
			size.height=image.info.height;
			
			_texture = new Texture2D((int)size.width, (int)size.height, TextureFormat.ARGB32, false);
			
			/* Associate the texture to the game object */			
			GetComponent<Renderer>().sharedMaterial.mainTexture = _texture;						
			
		}  
		
		/* Retrieve the image data */
		PXCMImage.ImageData data;
		pxcmStatus sts=image.AcquireAccess(PXCMImage.Access.ACCESS_READ,PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32,out data);
		if ( sts >= pxcmStatus.PXCM_STATUS_NO_ERROR) 
		{
			data.ToTexture2D(0, _texture);
			
			image.ReleaseAccess(data);
			
			/* and display it */
			_texture.Apply();			
		}
	}
	
	#endregion
	
	#region Unity's overridden methods
	
	// Initializaition
	void Start () 
	{
		var senseManager = GameObject.FindObjectOfType(typeof(SenseToolkitManager));
		if (senseManager == null)
		{
			Debug.LogWarning("Sense Manager Object not found and was added automatically");			
			senseManager = (GameObject)Instantiate(Resources.Load("SenseManager"));
			senseManager.name = "SenseManager";
		}
		
		/* Reset image size */
		size.width=size.height=0;		
		
		SetSenseOptions();
	}
	
	// Update is called once per frame
	void Update () 
	{ 
		if (SenseToolkitManager.Instance == null)
		{
			return;
		}
		
		if (SenseToolkitManager.Instance.IsSenseOptionSet(SenseOption.SenseOptionID.VideoSegmentation))
		{					
			if ( !SenseToolkitManager.Instance.Initialized ) return;
			
			if (SenseToolkitManager.Instance.Initialized)
			{			
				PXCMImage image = null;
			
				image = SenseToolkitManager.Instance.Image3DSegmentationOutput;			
				
                SetTexture(image);					
			}
		}		
	}
	
	// On Enable Set Sense Options
	void OnEnable()
	{
		if (SenseToolkitManager.Instance == null)
		{
			return;
		}
		
		SetSenseOptions();
	}
	
	// On Disable unset Sense Options
	void OnDisable()
	{
		UnsetSenseOptions();
	}
	
	#endregion	
}
