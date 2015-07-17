using UnityEngine;
using System.Collections;

public class LaunchMissile : MonoBehaviour {

    [HideInInspector] public Vector3 targetPos;
    private bool isFiring = false;
    PlayerShipStats playerShipStatsScript;
    RSSelectorRaycast rayCastScript;
    public float distanceToTarget = 555;
    GameObject temp;


	void Start () {
        temp = new GameObject();
        playerShipStatsScript = GetComponentInParent<PlayerShipStats>();
        rayCastScript = GetComponent<RSSelectorRaycast>();
	}
	
	// Update is called once per frame
	void Update () {

        targetPos = rayCastScript.handRay.GetPoint(distanceToTarget);

        if (GameObject.FindGameObjectWithTag("Missile") != null)
        {
            // animate the icon to let player know he cannot fire another missile until the other one explodes
        }
        else
        {
            if (isFiring)
            {
                playerShipStatsScript.fire = true;
                isFiring = false;
            }
        }
	}

    void sendFireMessage()
    {
        if (playerShipStatsScript.pirateShip.activeSelf == true)
        {
            temp.transform.position = targetPos;
            playerShipStatsScript.mlScript.target = temp;
            isFiring = true;
        }
    }
}
