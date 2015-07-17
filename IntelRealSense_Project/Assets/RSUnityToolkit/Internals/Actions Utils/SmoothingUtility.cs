/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/

using System;
using UnityEngine;
using RSUnityToolkit;

[System.Serializable]
public class SmoothingUtility : System.Object
{
	private SmoothingTypes _type3D;	
	private SmoothingTypes _type2D;	
	private SmoothingTypes _type1D;	
	
	private float _factor3D;	
	private float _factor2D;	
	private float _factor1D;	
	
	private PXCMDataSmoothing _dataSmoothing;
	
	private PXCMDataSmoothing.Smoother3D _smoother3D;	
	private PXCMDataSmoothing.Smoother2D _smoother2D;	
	private PXCMDataSmoothing.Smoother1D _smoother1D;
	
	private bool _initialized = false;
	
	public enum SmoothingTypes
	{
		Spring,
		Stabilizer,
		Weighted,
		Quadratic
	}
	
	public SmoothingUtility()
	{		
					
	}
	
	public void Dispose()
	{
		if (_smoother1D != null)
		{
			_smoother1D.Dispose();
		}
		
		if (_smoother2D != null)
		{
			_smoother2D.Dispose();
		}
		
		if (_smoother3D != null)
		{
			_smoother3D.Dispose();
		}
		
		if (_dataSmoothing != null)
		{
			_dataSmoothing.Dispose();
		}
	}
	
	~SmoothingUtility()
	{
		
	}
	
	
	public bool Init()
	{
		if (!_initialized && SenseToolkitManager.Instance != null)
		{
			SenseToolkitManager.Instance.AddDisposeFunction(Dispose);
			_initialized = true;
		}
		
		return _initialized;
	}
	
	public Quaternion ProcessSmoothing(SmoothingTypes type, float factor, Quaternion vec)
	{
		Init();
		Vector3 vec3 = new Vector3(vec.x, vec.y, vec.z);
		vec3 = ProcessSmoothing(type, factor, vec3);
		
		float w = vec.w;
		w = ProcessSmoothing(type, factor, w);
		
		return new Quaternion(vec3.x, vec3.y, vec3.z, w);
	}
	
	public Vector3 ProcessSmoothing(SmoothingTypes type, float factor, Vector3 vec)
	{
		Init();
		if (_dataSmoothing == null)
		{
			SenseToolkitManager.Instance.SenseManager.session.CreateImpl<PXCMDataSmoothing>(out _dataSmoothing);
		}
		if (_smoother3D == null || _type3D != type || factor != _factor3D)
		{
			if (_smoother3D != null)
			{
				_smoother3D.Dispose();
			}
			CreateSmootherType(type, factor, out _smoother3D);
			_type3D = type;
			_factor3D = factor;
		}
		
		PXCMPoint3DF32 point = new PXCMPoint3DF32(){x = vec.x, y = vec.y, z = vec.z};
		_smoother3D.AddSample(point);
		point = _smoother3D.GetSample();
		
		return new Vector3(point.x, point.y, point.z);
	}	
	
	public Vector2 ProcessSmoothing(SmoothingTypes type, float factor, Vector2 vec)
	{
		Init();
		if (_dataSmoothing == null)
		{
			SenseToolkitManager.Instance.SenseManager.session.CreateImpl<PXCMDataSmoothing>(out _dataSmoothing);
		}
		if (_smoother2D == null || _type2D != type || factor != _factor2D)
		{
			if (_smoother2D != null)
			{
				_smoother2D.Dispose();
			}
			CreateSmootherType(type, factor, out _smoother2D);
			_type2D = type;
			_factor2D = factor;
		}
		
		PXCMPointF32 point = new PXCMPointF32(){x = vec.x, y = vec.y};
		_smoother2D.AddSample(point);
		point = _smoother2D.GetSample();
		
		return new Vector2(point.x, point.y);
	}	
	
	public float ProcessSmoothing(SmoothingTypes type, float factor, float sample)
	{
		Init();
		if (_dataSmoothing == null)
		{
			SenseToolkitManager.Instance.SenseManager.session.CreateImpl<PXCMDataSmoothing>(out _dataSmoothing);
		}
		if (_smoother1D == null || _type1D != type || factor != _factor1D)
		{
			if (_smoother1D != null)
			{
				_smoother1D.Dispose();
			}
			CreateSmootherType(type, factor, out _smoother1D);
			_type1D = type;
			_factor1D = factor;
		}
			
		_smoother1D.AddSample(sample);
		sample = _smoother1D.GetSample();
		
		return sample;
	}
	
	private void CreateSmootherType(SmoothingTypes type, float factor, out PXCMDataSmoothing.Smoother3D smoother)
	{
		switch (type)
		{		
		case SmoothingTypes.Quadratic:
			smoother = _dataSmoothing.Create3DQuadratic(factor);
			break;
		case SmoothingTypes.Stabilizer:
			smoother = _dataSmoothing.Create3DStabilizer(7, factor);
			break;
		case SmoothingTypes.Weighted:
			smoother = _dataSmoothing.Create3DWeighted((int)factor);
			break;
		case SmoothingTypes.Spring:
		default:
			smoother = _dataSmoothing.Create3DSpring(factor);
			break;
		}	
		
	}
	
	private void CreateSmootherType(SmoothingTypes type, float factor, out PXCMDataSmoothing.Smoother2D smoother)
	{
		switch (type)
		{		
		case SmoothingTypes.Quadratic:
			smoother = _dataSmoothing.Create2DQuadratic(factor);
			break; 
		case SmoothingTypes.Stabilizer:
			smoother = _dataSmoothing.Create2DStabilizer(7, factor);
			break;
		case SmoothingTypes.Weighted:
			smoother = _dataSmoothing.Create2DWeighted((int)factor);
			break;
		case SmoothingTypes.Spring:
		default:
			smoother = _dataSmoothing.Create2DSpring(factor);
			break;
		}		
	}
	
	private void CreateSmootherType(SmoothingTypes type, float factor, out PXCMDataSmoothing.Smoother1D smoother)
	{
		switch (type)
		{
		case SmoothingTypes.Quadratic:
			smoother = _dataSmoothing.Create1DQuadratic(factor);
			break;
		case SmoothingTypes.Stabilizer:
			smoother = _dataSmoothing.Create1DStabilizer(7, factor);
			break;
		case SmoothingTypes.Weighted:
			smoother = _dataSmoothing.Create1DWeighted((int)factor);
			break;
		case SmoothingTypes.Spring:
		default:
			smoother = _dataSmoothing.Create1DSpring(factor);
			break;
		}		
	}
}


