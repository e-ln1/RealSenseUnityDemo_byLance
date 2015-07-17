using UnityEngine;
using System.Collections;

public class TogglePanelOnOff : MonoBehaviour {

	public void TogglePanel (GameObject panel) {
		panel.SetActive (!panel.activeSelf);
	}
}
