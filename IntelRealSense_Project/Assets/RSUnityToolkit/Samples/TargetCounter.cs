/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/
using UnityEngine;
using System.Collections;

public class TargetCounter : MonoBehaviour {
	
	public int Points;
	
	private GUIStyle _style;
	
	// Use this for initialization
	void Start () {
		Points = 0;
		_style = new GUIStyle();
		_style.fontSize = 35;
		_style.fontStyle = FontStyle.Bold;
		_style.normal.textColor = UnityEngine.Color.white;		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionExit(Collision collision) {
		foreach (ContactPoint contact in collision.contacts) {
			if (contact.otherCollider != null && contact.otherCollider.name != null && contact.otherCollider.name.StartsWith("Sphere"))
			{
				Points++;
			}
			
		}
	}
	
	void OnTriggerExit(Collider other){
    	if (other.gameObject.name.StartsWith("Sphere")){
       		Points++;
       } 
    }
	
	void OnGUI()
	{
		GUILayout.Label("Points = "  + Points, _style);
	}
}
