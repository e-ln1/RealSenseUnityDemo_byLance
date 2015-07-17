using UnityEngine;
using System.Collections;

public class testEventTrigger : MonoBehaviour {
	
	
	void Update () {
		if (Input.GetKeyDown ("q"))
		{
			EventManager.TriggerEvent ("test");
			Debug.Log ("q pressed");
		}
		
		if (Input.GetKeyDown ("o"))
		{
			EventManager.TriggerEvent ("Spawn");
		}
		
		if (Input.GetKeyDown ("p"))
		{
			EventManager.TriggerEvent ("Destroy");
		}
		
		if (Input.GetKeyDown ("x"))
		{
			EventManager.TriggerEvent ("Junk");
		}
	}
}