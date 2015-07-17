#pragma strict

var startWidth : double;
var target : GameObject;
var chargeLevel : double;
private var distance : Vector3;
var timer : double;
private var fireTime : double;
private var fireLength : double;
var fire : boolean = false;
private var beam : LineRenderer;
var burst : GameObject;
private var charging : boolean = false;
var charge : GameObject;


function Start () 
{
	if (gameObject.GetComponent.<LineRenderer>() != null)
	{
		beam = gameObject.GetComponent.<LineRenderer>();
		beam.SetVertexCount(2);
		beam.material = Resources.Load("Energy Beam") as Material;
		beam.SetWidth(startWidth, startWidth);
		beam.enabled = false;
	}
	else
	{
		gameObject.AddComponent(LineRenderer);
		beam = gameObject.GetComponent.<LineRenderer>();
		beam.SetVertexCount(2);
		beam.material = Resources.Load("Energy Beam Material") as Material;
		beam.SetWidth(startWidth, startWidth);
		beam.enabled = false;
	}
	fireTime = chargeLevel + 1.25;
	fireLength = fireTime + 3;
}

function Update () {

	if (fire && target != null)		// Execute when an enemy is targeted and fired upon
	{
		timer += Time.deltaTime;	// Keeps time for firing animation sequence
		
		//Create energy ball
		if (charge == null)
		{
			charge = Resources.Load("Beam Charge") as GameObject;
			charge = Instantiate(charge, gameObject.transform.position, Quaternion.identity);
			charge.GetComponent(Autodestruct).timeDelay = fireTime;
		}
		
		// Energy ball expands
		else if (timer < chargeLevel)
		{
			charge.transform.localScale += Vector3(0.12,0.12,0.12);
		}
		// Energy ball contracts
		else if (timer >= chargeLevel && timer < fireTime && (charge.transform.localScale.x > 0))
		{
			charge.transform.localScale -= Vector3(0.5,0.5,0.5);
		}
		// Energy beam fires
		else if (timer >= fireTime && timer < fireLength)
		{
			charge.transform.position = gameObject.transform.position;
			if (burst == null)
			{
				burst = Resources.Load("Energy Beam Strike FX") as GameObject;
				burst = Instantiate(burst, target.transform.position, Quaternion.identity);
			}
			beam.enabled = true;
			beam.SetPosition(0, gameObject.transform.position);
			beam.SetPosition(1, target.transform.position);
		}
		// Energy beam stops firing.
		else if (timer >= fireLength)
		{
			fire = false;
			timer = 0;
		}
	}
	else if (!fire)
		beam.enabled = false;
}