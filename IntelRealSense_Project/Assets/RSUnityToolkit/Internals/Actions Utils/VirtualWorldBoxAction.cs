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
using RSUnityToolkit;

/// <summary>
/// Virtual world box action draws gizmos when needed
/// </summary>
public abstract class VirtualWorldBoxAction : BaseAction {
	
	/// <summary>
	/// Enum that indicates the reference for the center of the box
	/// </summary>
	public enum BoxCenterRef {
		Self,
		World,
		GameObject,
		Custom
	};
	
	#region Public Fields
	
	/// <summary>
	/// The virtual world box center.
	/// </summary>
	[SerializeField]
	public Vector3 VirtualWorldBoxCenter = new Vector3(0,0,50);
	
	/// <summary>
	/// The virtual world box dimensions.
	/// </summary>
	[SerializeField]
	public Vector3 VirtualWorldBoxDimensions = new Vector3(100,100,100);	
	
	public BoxCenterRef CenterReference = BoxCenterRef.Self;
	
	public GameObject ReferenceGameObject = null;
	
	#endregion
	
	#region Private Fields
	
	//On Start
	new void Start()
	{
		base.Start();
		
		if (CenterReference == BoxCenterRef.Self)
		{
			VirtualWorldBoxCenter = gameObject.transform.localPosition;
		}
		updateVirtualWorldBoxCenter();
	}
	
	public void updateVirtualWorldBoxCenter()
	{
		switch (CenterReference)
		{
		case BoxCenterRef.Self:	
			if (!Application.isPlaying)
			{
				VirtualWorldBoxCenter = gameObject.transform.localPosition;
			}
			break;
		case BoxCenterRef.World:			
			if (gameObject.transform.parent != null)
			{
				VirtualWorldBoxCenter = gameObject.transform.parent.InverseTransformPoint(Vector3.zero);
			}
			else 
			{
				VirtualWorldBoxCenter = Vector3.zero;
			}
			break;
		case BoxCenterRef.GameObject:
			if (gameObject.transform.parent != null)
			{
				VirtualWorldBoxCenter = gameObject.transform.parent.InverseTransformPoint(ReferenceGameObject.transform.position);
			}
			else 
			{
				VirtualWorldBoxCenter = ReferenceGameObject.transform.position;
			}
			break;
		}
	}
	
	// used to draw gizmos
	private void OnDrawGizmosSelected()
	{
		updateVirtualWorldBoxCenter();
		if (transform.parent != null)
		{
			Gizmos.matrix = transform.parent.localToWorldMatrix;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(VirtualWorldBoxCenter, VirtualWorldBoxDimensions);
    }

	#endregion
}
 
