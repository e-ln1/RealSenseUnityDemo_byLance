using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
//using System.Collections.Generic;
using UnityEngine.UI;

public class RSSelectorRaycast : MonoBehaviour {

    [HideInInspector] public Ray handRay;

	public float distance = 500;
	int canvasCameraLayerMask;
	GameObject playObject;
	Animator playButtonAC;
	Animator selectorHandAC;

	public GameObject instructionalGameController;

	bool hasSelectedPlay = false;
	bool hasFiredOneShotEvent = false;
	[HideInInspector] public bool isReadyToInitializeNarrativeCanvas = false;
	//float waitTime = 2.0f;

		// Use this for initialization
	void Start () {
		canvasCameraLayerMask = LayerMask.GetMask ("RealSenseInteractiveUI");
		playObject = GameObject.FindWithTag("PlayButton");
		playButtonAC = playObject.GetComponent<Animator>();
		selectorHandAC = GetComponent<Animator>();
	}

	void Update () {
		transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);		
		Vector3 fwd = transform.TransformDirection(Camera.main.transform.forward);
		handRay = new Ray(transform.position, fwd);
		RaycastHit hitInfo;
        //RaycastHit2D hitInfo2D;
        //if (Physics.Raycast(handRay, ))
		if(!hasSelectedPlay)
		{
			if (Physics.Raycast(handRay, out hitInfo, Mathf.Infinity, canvasCameraLayerMask) && (!hasFiredOneShotEvent)) {
				//Debug.Log ("Raycast has been cast");
				//Debug.DrawRay(fwd.direction, hit.point, Color.yellow);
				//Debug.Log(hitInfo.distance);
				//if hitInfo
				//if (hitInfo.
				// else if "OtherSpacecraft"
				// else if "InventoryObject"
				// else if ...
				// for the demo I've just set up this raycast to identify the Play Button on the Camera canvas overlay
				playButtonAC.Play("PlayButtonStartsGame");
				hasSelectedPlay = true;
			}
		}
		if (hasSelectedPlay && (!hasFiredOneShotEvent))
		{
			StartCoroutine("waitForAnimation");
			hasFiredOneShotEvent = true;
		}

	}

	IEnumerator waitForAnimation()
	{
		yield return new WaitForSeconds(3.0f);
		playObject.SetActive(false);
		selectorHandAC.SetTrigger("TargetingSprite");
		isReadyToInitializeNarrativeCanvas = true;
		//Debug.Log ("ready to fire off NPC dialoge boxes");
		Invoke ("startTheInstructionalGameController", 1.0f);
		yield return null;
	}

	void startTheInstructionalGameController()
	{
		if(instructionalGameController != null)
		{
			instructionalGameController.SetActive(true);
		}
		else
		{
			Debug.Log ("You forgot to assign the InstructionalGameController object.");
		}
	}

	void OnDrawGizmos() {
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		Gizmos.color = Color.cyan;
		Gizmos.DrawRay(transform.position, fwd * 500);
		
	}
	
}
