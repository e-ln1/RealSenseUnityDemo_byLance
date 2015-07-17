using UnityEngine;
using System.Collections;

public class GameEventsOnRails : MonoBehaviour {

	public GameObject playerDashboard;
	PlayerShipStats playerScript;
	public GameObject pirateShip;
	PirateShipMovement pirateScript;
	public GameObject npc_CommScreen;  // assigned in the inspector
	public GameObject introText;
	public GameObject playButton;
	public float instructionTime = 0.5f;
	public GameObject instructionsPanel;
	public GameObject rsSelector;
	public GameObject VoiceRecognitionObject;
	RSSelectorRaycast rsSelectorScript;
	int canvasCameraLayerMask;
	bool hasSelectedCheckBox = false;

	public Animator missionSuccessAC;

	// Use this for initialization
	void Start () {
		pirateScript = pirateShip.GetComponent<PirateShipMovement>();
		playerScript = playerDashboard.GetComponent<PlayerShipStats>();
		npc_CommScreen.SetActive(true);
		introText.SetActive(true);
		Invoke("setPlayActive", instructionTime);
		rsSelectorScript = rsSelector.GetComponent<RSSelectorRaycast>();
		canvasCameraLayerMask = LayerMask.GetMask ("RealSenseInteractiveUI");
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hitInfo;
		if (Physics.Raycast(rsSelectorScript.handRay, out hitInfo, Mathf.Infinity, canvasCameraLayerMask))
		{
			if(!hasSelectedCheckBox)
			{
				playButton.GetComponent<Animator>().Play ("PlayButtonStartsGame");
				StartCoroutine("waitForAnimation");
				hasSelectedCheckBox = true;
			}
			//Debug.Log (hitInfo.distance);
		}

		if(pirateScript.isDestroyed)
		{
			missionSuccessAC.Play("MissionSuccessScreenOverlay");
			VoiceRecognitionObject.GetComponent<VoiceRec>();
			VoiceRecognitionObject.SetActive(true);
		}

		if (playerScript.currentHullIntegrity <= 0)  // GameOver death sequence for the player
		{
			//GameOver anim is in PlayerShipStats script
			VoiceRecognitionObject.SetActive(true);
		}
	}

	IEnumerator waitForAnimation()
	{
		yield return new WaitForSeconds(3.0f);
		instructionsPanel.SetActive(false);
		playButton.SetActive(false);
		npc_CommScreen.SetActive(false);
		Invoke("setPirateShipActive", 4.0f);
		yield return null;
	}

	void setPlayActive()
	{
		playButton.SetActive(true);
		playButton.GetComponent<Animator>().Play ("PlayButtonAnim");

	}

	void setPirateShipActive()
	{
		pirateShip.SetActive(true);
		pirateScript.isActive = true;
	}

	public void OnApplicationQuit()
	{

	}
}
