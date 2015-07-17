using UnityEngine;
using System.Collections;

public class CameraLookAtNPC : MonoBehaviour {

	public Transform npc;

	void Start() {
	}

	// Update is called once per frame
	void Update () {
		transform.LookAt(npc.TransformPoint(Vector3.zero));
	}
}
