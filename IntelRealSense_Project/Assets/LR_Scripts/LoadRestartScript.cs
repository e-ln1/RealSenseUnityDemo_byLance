using UnityEngine;
using System.Collections;
//using UnityEngine.Events;

public class LoadLevelScript : MonoBehaviour {

    public GameObject VoiceRec;
    //UnityAction 

	public void LoadDemoLevel()
	{
        VoiceRec.GetComponent<VoiceRec>().OnApplicationQuit();
		Application.LoadLevel("RealSense_Testing");
	}
}
