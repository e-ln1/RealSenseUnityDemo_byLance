using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PirateShipStats : MonoBehaviour {

	public float timeBetweenAttacks = 0.5f;     // The time in seconds between each attack.
	public int attackDamage = 100;               // The amount of health taken away per attack.
	
	
	//Animator anim;                              // Reference to the animator component.
	GameObject playerShip;                          // Reference to the player GameObject.
	PlayerShipStats playerShipStats;                  // Reference to the player's ship stats.
	public bool fire;
	float timer;                                // Timer for counting up to the next attack.
	private KineticRailGun krgScript;			// This custom type is red in MonoDevelop because C# is calling JavaScript
											// It works just fine, however
	public Slider PirateHealthSlider;
	[HideInInspector] public int currentHealth;
	int startingHealth;

    PirateShipMovement pmScript;

	void Awake ()
	{
		// Setting up the references.
		krgScript = GetComponent<KineticRailGun>();
		playerShip = GameObject.FindGameObjectWithTag ("Player");
		playerShipStats = playerShip.GetComponent<PlayerShipStats>();
        pmScript = GetComponent<PirateShipMovement>();
		startingHealth = (int)PirateHealthSlider.maxValue;

	}

	// Use this for initialization
	void Start () {
        playerShipStats.isAwakingPirateShip = true;
		currentHealth = startingHealth;
//        Debug.Log(currentHealth);
	}
	
	// Update is called once per frame
	void Update () {
		// Add the time since Update was last called to the timer.
		//timer += Time.deltaTime;

		if (fire)
		{
            if (GameObject.FindGameObjectWithTag("RailGunStrikeFX") != null)
            {
                GameObject temp = GameObject.FindGameObjectWithTag("RailGunStrikeFX");
                Destroy(temp);
            }
			krgScript.fire = true;
			fire = false;
            Invoke("fireDirectHit", 0.5f);
		}
	}

	void fireDirectHit ()
	{
		// Reset the timer.
		//timer = 0f;
		
		// If the player has health to lose...
		if(playerShipStats.currentHullIntegrity > 0)
		{
			// ... damage the player.
			playerShipStats.TakeDamage (attackDamage);
		}
	}

	public void PirateShipTakesDamage (int dmgAmount)
	{
		// Set the damaged flag so the screen will flash.
		//damaged = true;
		
		if (currentHealth > 0)
			// Reduce the current health by the damage amount.
			currentHealth -= dmgAmount;
		
		// Set the shield slider
		PirateHealthSlider.value = currentHealth;
		
		// Play the hurt sound effect.
		//playerAudio.Play ();
		
		// If the ship has lost all shields...
		if(currentHealth <= 0)
		{
            pmScript.isDestroyed = true;  // sets in motion the methods in PirateShipMovement that animate death
		}
	
	}
    
    void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Missile"))
        {
            PirateShipTakesDamage(playerShipStats.attackDmg);
            Instantiate(Resources.Load("Missile Strike FX"), transform.position, Quaternion.identity);
            Destroy(other.gameObject);
//            Debug.Log(currentHealth);
        }
    }
}
