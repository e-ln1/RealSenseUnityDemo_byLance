using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerShipStats : MonoBehaviour {

	//bool damaged;  // True when the player gets damaged.
	public int startingShield;
	public int currentShield;
	public Slider shieldSlider;
	public int startingHull;
	public int currentHullIntegrity;
	public Slider hullIntegritySlider;
	[HideInInspector] public MissileLauncher mlScript;
	public bool fire;

	public GameObject pirateShip;
	PirateShipStats pirateShipStats;
	public int attackDmg = 100;

    Animator dashboardAnim;

    GameObject UIScreenOverlay;
    Animator UIScreenOverlayAC;

    public bool isAwakingPirateShip = false;

	void Awake() {
		startingHull = (int)hullIntegritySlider.maxValue;
		startingShield = (int)shieldSlider.maxValue;
		mlScript = GetComponentInChildren<MissileLauncher>();

        dashboardAnim = GetComponent<Animator>();
        UIScreenOverlay = GameObject.FindGameObjectWithTag("UIScreenOverlay");
        UIScreenOverlayAC = UIScreenOverlay.GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () {
		// Setting up the references.
		//screenOverlay = 
		//anim = GetComponent <Animator> ();
		//playerAudio = GetComponent <AudioSource> ();
		//playerMovement = GetComponent <PlayerMovement> ();
		//playerShooting = GetComponentInChildren <PlayerShooting> ();

		// Set the initial health of the player.
		currentShield = startingShield;
		currentHullIntegrity = startingHull;
	}
	
	// Update is called once per frame
	void Update () {

        if (isAwakingPirateShip)
        {
            pirateShip = GameObject.FindGameObjectWithTag("OtherSpaceCraft");
            pirateShipStats = pirateShip.GetComponent<PirateShipStats>();  // not enabled on awake
            isAwakingPirateShip = false;
        }

		if(pirateShipStats != null)
		{
			if (fire)
			{
	            if (GameObject.FindGameObjectWithTag("Missile") == null)
	    			mlScript.fire = true;
				fire = false;
				//Invoke("fireDirectHitOnEnemy", 2f);  
                // Damaging the pirate ship is based upon OnTriggerEnter in PirateShipStats
			}
		}
	}

	public void TakeDamage (int dmgAmount)
	{
		// Set the damaged flag so the screen will flash.
		//damaged = true;

		if (currentShield > 0)
			// Reduce the current health by the damage amount.
			currentShield -= dmgAmount;
		
		// Set the shield slider
		shieldSlider.value = currentShield;
		
		// Play the hurt sound effect.
		//playerAudio.Play ();
		
		// If the ship has lost all shields...
		if(currentShield <= 0)
		{
			//int dmgToHull =  Mathf.Abs(currentShield);
			//if (currentShield == 0)
			//	dmgToHull = dmgAmount;
			//currentShield = 0;
			currentHullIntegrity -= dmgAmount;
		}

		hullIntegritySlider.value = currentHullIntegrity;

		if (currentHullIntegrity <= 0)  // GameOver death sequence for the player
		{
            Instantiate(Resources.Load("Rail Gun Strike FX"), Camera.main.transform.position, Quaternion.identity);
            dashboardAnim.SetTrigger("PlayerDead");
            UIScreenOverlayAC.SetTrigger("GameOver");
		}
	}

}
