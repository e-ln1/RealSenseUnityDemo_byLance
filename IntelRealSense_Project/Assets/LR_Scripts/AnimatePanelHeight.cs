using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AnimatePanelHeight : MonoBehaviour {

	private Vector2 pointerOffset;
	private RectTransform panelRectTransform;

	void Awake() {
		Canvas canvas = GetComponentInParent<Canvas>();
		if (canvas != null) {
			panelRectTransform = transform as RectTransform;
		}
	}

	void Start () {
		closePanel();
	}

	public void openPanel () {
		if (panelRectTransform == null)
			return;

	//	RectTransform temp = panelRectTransform;
	//	temp.offsetMin = 0;
	//	temp.offsetMax = 22;
	//	panelRectTransform = temp;
	}

	public void closePanel () {
		if (panelRectTransform == null)
			return;
		
		RectTransform temp = panelRectTransform;
		temp.sizeDelta = new Vector2 (130f, 22f);
		panelRectTransform = temp;
	}
}

