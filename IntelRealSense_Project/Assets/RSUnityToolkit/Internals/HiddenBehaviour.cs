using UnityEngine;
using System.Collections;

[AddComponentMenu("")]
public class HiddenBehaviour : MonoBehaviour
{

	[HideInInspector]
	public BaseAction ActionOwner = null;
	
	public HiddenBehaviour()
	{

	}
	
}

