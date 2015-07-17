using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class testEventManager : MonoBehaviour {
	
	private UnityAction someListener;
	
	void Awake ()
	{
		someListener = new UnityAction (SomeFunction);
	}
	
	void OnEnable ()
	{
		Debug.Log ("Listener object was enabled");
		EventManager.StartListening ("test", someListener);
		EventManager.StartListening ("Spawn", SomeOtherFunction);
		EventManager.StartListening ("Destroy", SomeThirdFunction);
	}
	
	void OnDisable ()
	{
		EventManager.StopListening ("test", someListener);
		EventManager.StopListening ("Spawn", SomeOtherFunction);
		EventManager.StopListening ("Destroy", SomeThirdFunction);
	}
	
	void SomeFunction ()
	{
		Debug.Log ("Some Function called TEST was called!");
	}
	
	void SomeOtherFunction ()
	{
		Debug.Log ("Some Other Function SPAWN was called!");
	}
	
	void SomeThirdFunction ()
	{
		Debug.Log ("Some Third Function DESTROY was called!");
	}
}
