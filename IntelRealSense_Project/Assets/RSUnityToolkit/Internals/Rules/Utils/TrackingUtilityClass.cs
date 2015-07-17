/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;

public abstract class TrackingUtilityClass
{
	/// <summary>
	/// Clamps the Position vector to the given box.
	/// </summary>
	/// <param name='position'>
	/// Position.
	/// </param>
	/// <param name='boxCenter'>
	/// Box center.
	/// </param>
	/// <param name='boxDimensions'>
	/// Box dimensions.
	/// </param>
	public static void ClampToRealWorldInputBox(ref Vector3 position, Vector3 boxCenter, Vector3 boxDimensions)
	{
		position.x = Mathf.Max(boxCenter.x - (boxDimensions.x)/2, position.x);
		position.x = Mathf.Min(boxCenter.x + (boxDimensions.x)/2, position.x);
		
		position.y = Mathf.Max(boxCenter.y - (boxDimensions.y)/2, position.y);
		position.y = Mathf.Min(boxCenter.y + (boxDimensions.y)/2, position.y);
		
		
		position.z = Mathf.Max(boxCenter.z - (boxDimensions.z)/2, position.z);
		position.z = Mathf.Min(boxCenter.z + (boxDimensions.z)/2, position.z);
	}
	
	/// <summary>
	/// Normalize the specified position with respect to the given box
	/// </summary>
	/// <param name='position'>
	/// Position.
	/// </param>
	/// <param name='boxCenter'>
	/// Box center.
	/// </param>
	/// <param name='boxDimensions'>
	/// Box dimensions.
	/// </param>
	public static void Normalize(ref Vector3 position, Vector3 boxCenter, Vector3 boxDimensions)
	{
		float left = boxCenter.x - (boxDimensions.x)/2;
		float top = boxCenter.y - (boxDimensions.y)/2;
		float back = boxCenter.z - (boxDimensions.z)/2;
		
		position.x = (position.x - left)/ boxDimensions.x;
		
		position.y = (position.y - top)/ boxDimensions.y;
		
		position.z = (position.z - back)/ boxDimensions.z;
	}

	
}