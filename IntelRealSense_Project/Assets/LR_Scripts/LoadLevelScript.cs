using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using UnityEngine.Events;

public class LoadRestartScript : MonoBehaviour {

    public GameObject VoiceRec;
    //UnityAction 

	public void StartOverDemoLevel()
	{
		Application.LoadLevel("SettingUpVoiceTests");
	}
}
