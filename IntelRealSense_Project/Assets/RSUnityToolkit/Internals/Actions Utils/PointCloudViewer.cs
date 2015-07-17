/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/

using UnityEngine;
using System.Collections;
using RSUnityToolkit;

/// <summary>
/// Point cloud viewer 
/// </summary>
public class PointCloudViewer : MonoBehaviour {
	
	#region Public Fields
	
	/// <summary>
	/// The point cloud material
	/// </summary>
	public Material PointCloudMaterial = null;	
	
	/// <summary>
	/// The max depth value.
	/// </summary>
	public float MaxDepthVal = 90f;
	
	/// <summary>
	/// When enabled UV Map will be used to show color image on the point cloud
	/// </summary>
	public bool UseUVMap = true;
	
	#endregion
	
	#region Private Fields
	
	private Mesh _mesh;	
	private Vector3[] _vertices = null;		
	private Vector2[] _uv 		= null;
	private Vector4[] _tangents = null;
	private int[] _triangles 	= null;
	
	private bool _removeBackTriangles = true;
	
	private bool _lastUseUVMAP = true;
	
	private DrawImages _drawImagesComponent;
	
	#endregion
	
	#region Private methods
	
	/// <summary>
	/// Sets the sense option according to the Stream field
	/// </summary>
	private void SetSenseOptions()
	{				
		SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.PointCloud);	
		if (UseUVMap)
		{
			SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.UVMap);	
		}
			
	}
	/// <summary>
	/// Unsets the sense option according to the Stream field
	/// </summary>	
	private void UnsetSenseOptions()
	{			
		SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.PointCloud);	
		
		if (_lastUseUVMAP)
		{
			SenseToolkitManager.Instance.UnsetSenseOption(SenseOption.SenseOptionID.UVMap);		
		}
	}
	
	#endregion
	
	#region Unity's overridden methods
	
	// Use this for initialization
	void Start () 
	{	
		var senseManager = GameObject.FindObjectOfType(typeof(SenseToolkitManager));
		if (senseManager == null)
		{
			Debug.LogWarning("Sense Manager Object not found and was added automatically");			
			senseManager = (GameObject)Instantiate(Resources.Load("SenseManager"));
			senseManager.name = "SenseManager";
		}
		
		SetSenseOptions();
		
		this.gameObject.AddComponent< MeshFilter > ();
		if (this.GetComponent<MeshRenderer>() == null)
		{
			this.gameObject.AddComponent< MeshRenderer > ();	
		}
		
		if (PointCloudMaterial!=null)
		{
			this.gameObject.GetComponent<Renderer>().material = PointCloudMaterial;
		}
		
		_drawImagesComponent = this.gameObject.AddComponent<DrawImages>();
		
		if (UseUVMap)
		{
			_drawImagesComponent.enabled = true;
		}
		else 
		{
			_drawImagesComponent.enabled = false;
		}

	}
	
	// Update is called once per frame
	void Update () 
	{	
		if (_lastUseUVMAP != UseUVMap)
		{
			UnsetSenseOptions();
			SetSenseOptions();
			if (UseUVMap)
			{
				_drawImagesComponent.enabled = true;
			}
			else 
			{
				_drawImagesComponent.enabled = false;
			}
			_lastUseUVMAP = UseUVMap;
		}
		
		if (SenseToolkitManager.Instance.PointCloud != null)
		{		
			if (_mesh == null)
			{
				// Retrieve a mesh instance
				_mesh = this.gameObject.GetComponent<MeshFilter> ().mesh;	

			}
			int _gridSize = 3;
			
			int width = SenseToolkitManager.Instance.ImageDepthOutput.info.width/_gridSize;
			int height = SenseToolkitManager.Instance.ImageDepthOutput.info.height/_gridSize;
			
			// Build vertices and UVs
			if (_vertices == null)
			{
				_vertices = new Vector3[width * height];
			}		
							
			if (_tangents == null)
			{
				_tangents = new Vector4[width * height];
			}	
			
			if (_uv == null)
			{
				_uv = new Vector2[width * height];
			}
					
			int i = 0;
			for (int y=0; y <  height; y++) 
			{
				for (int x=0; x <  width - 1; x++)  
				{				
				
					int j = y * width * _gridSize * _gridSize + x * _gridSize;
					
					_vertices [i].x  = SenseToolkitManager.Instance.PointCloud[j].x / 10;
					_vertices [i].y  = SenseToolkitManager.Instance.PointCloud[j].y / 10;
					_vertices [i].z  = -SenseToolkitManager.Instance.PointCloud[j].z / 10;					
					
					if (UseUVMap) 
					{
						_uv[i].x = SenseToolkitManager.Instance.UvMap[j].x ;
						_uv[i].y = SenseToolkitManager.Instance.UvMap[j].y ;
					}
					
					i++;
				}
			}
			
			// Assign them to the mesh
			_mesh.vertices = _vertices;
			_mesh.uv = _uv;
			
			// Build triangle indices: 3 indices into vertex array for each triangle
			if (_triangles == null)
			{
				_triangles = new int[(height - 1) * (width - 1) *6];			
			}
			
			bool backGroundTriangles = false;
			int index =0;
			for ( int y = 0; y < height - 1 ; y++ ) 
			{
				for ( int x = 0; x < width - 1; x++ ) 
				{	
					if (_removeBackTriangles) {
						backGroundTriangles = (
							( Mathf.Abs(_vertices[y * width + x].z) > MaxDepthVal ) || 
							( Mathf.Abs(_vertices[y * width + x + 1].z) > MaxDepthVal ) || 
							( Mathf.Abs(_vertices[(y + 1) * width + x].z )> MaxDepthVal ) || 
							( Mathf.Abs(_vertices[(y + 1) * width + x + 1].z) > MaxDepthVal ) 
						);

						backGroundTriangles = backGroundTriangles || (
							( Mathf.Abs(_vertices[y * width + x].z)  == 0 ) || 
							( Mathf.Abs(_vertices[y * width + x + 1].z) == 0 ) || 
							( Mathf.Abs(_vertices[(y + 1) * width + x].z ) == 0 ) || 
							( Mathf.Abs(_vertices[(y + 1) * width + x + 1].z) == 0 ) 
						);
					}
					if (!backGroundTriangles) {
						// For each grid cell output two triangles						
						_triangles [index++] = (y * width) + x;
						_triangles [index++] = ((y + 1) * width) + x;
						_triangles [index++] = (y * width) + x + 1;
						
						_triangles [index++] = ((y + 1) * width) + x;
						_triangles [index++] = ((y + 1) * width) + x + 1;
						_triangles [index++] = (y * width) + x + 1;
					}
					
					
				}
			}
			
			for ( ; index < (height - 1) * (width - 1) * 6 ; index++)
			{
				_triangles[index] = 0;
			}
			
			
			_mesh.triangles = _triangles;
			
			// Auto-calculate vertex normals from the mesh
			_mesh.Optimize();
			_mesh.RecalculateNormals ();			
		}
	}
	
	//On enable set sense options
	void OnEnable()
	{
		if (SenseToolkitManager.Instance == null)
		{
			return;
		}
		
		SetSenseOptions();
	}
	
	//On disable unset sense options
	void OnDisable()
	{
		UnsetSenseOptions();
	}
	
	#endregion
}