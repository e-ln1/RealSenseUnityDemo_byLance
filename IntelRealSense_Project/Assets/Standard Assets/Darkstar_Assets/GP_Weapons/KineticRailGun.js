#pragma strict

@HideInInspector
var target : GameObject;
@HideInInspector
var fire : boolean = false;
var volleyCount : int = 1;			// LEAVE AT 1 UNTIL DELAY FUNCTION IS IMPLEMENTED (LINE 28)
var timeBetweenShots : double;
var timer : double;
var round : GameObject;


function Start () 
{
    round = Resources.Load("KRG Round") as GameObject;
    target = GameObject.FindWithTag("DashboardTarget");
}

function Update () 
{
	krgDirectHit();
	
}

function krgDirectHit() {
		if (fire)
	{
		//gameObject.transform.LookAt(target.transform);
		timer += Time.deltaTime;
		
		// Loop through volleyCount # KRG rounds
		for (var i : int = 0; i < volleyCount; i++)
		{
			var round = gameObject.Instantiate(round, gameObject.transform.position, Quaternion.identity);
			round.transform.LookAt(target.transform);
			round.GetComponent(KRGRound).target = this.target;
			// NEED A DELAY FUNCTION HERE
		}		
		
		
		
		fire = false;
	}
	else
	{
		timer = 0;
	}
}