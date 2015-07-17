using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RelayScript : MonoBehaviour {

	private Slider slider;
	public GameObject rightHandRS;
	private TrackingAction rightHandTracking;
	private Vector3 rightHandInitPos;
	private Vector3 rightHandRunPos;
	private float rightHandHorzScale;

	void Awake(){
		slider = GetComponent<Slider>();
		slider.value = 0.0f;
	}

	void Start() {
		//rightHandRS = GameObject.Find("RightHandRS");
		rightHandTracking = (TrackingAction)rightHandRS.GetComponent("TrackingAction");
		rightHandHorzScale = rightHandTracking.VirtualWorldBoxDimensions.x;
		rightHandInitPos = rightHandRS.transform.position;
		rightHandRunPos = new Vector3(0.0f,0.0f,0.0f);
	}

	void Update() {
			rightHandRunPos = rightHandRS.transform.position;
			slider.value = 0.5f + ((rightHandRunPos.x - rightHandInitPos.x)/rightHandHorzScale);
	}
}
