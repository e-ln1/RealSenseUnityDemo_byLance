/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/

using System;

[System.Serializable]
public class Transformation3D : System.Object
{
	
	public Bool3D Position;
	public Bool3D Rotation;
	
	#region Ctor
	
	public Transformation3D() : base()
	{
		Position = new Bool3D();
		Rotation = new Bool3D();
	}
	
	#endregion
	
}


