/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/

using System;

[System.Serializable]
public class ScaleConstraints : System.Object
{
	public MinMax2D XScale;
	public MinMax2D YScale;
	public MinMax2D ZScale;
	
	public Bool3D Freeze;
	
	#region Ctor
	
	public ScaleConstraints() : base()
	{
		XScale = new MinMax2D();
		YScale = new MinMax2D();
		ZScale = new MinMax2D();
		
		Freeze = new Bool3D();
	}
	
	#endregion
	
}


