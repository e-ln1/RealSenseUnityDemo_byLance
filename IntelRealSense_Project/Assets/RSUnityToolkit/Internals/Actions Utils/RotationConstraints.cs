/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/

using System;

[System.Serializable]
public class RotationConstraints : System.Object
{ 	
	public RotationMax2D XRotation;
	public RotationMax2D YRotation;
	public RotationMax2D ZRotation;
	
	public Bool3D Freeze;
	
	#region Ctor
	
	public RotationConstraints() : base()
	{
		XRotation = new RotationMax2D();
		YRotation = new RotationMax2D();
		ZRotation = new RotationMax2D();
		
		Freeze = new Bool3D();
	}
	
	#endregion
	
}


