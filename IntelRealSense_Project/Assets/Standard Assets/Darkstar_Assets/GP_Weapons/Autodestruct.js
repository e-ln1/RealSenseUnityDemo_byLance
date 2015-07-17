#pragma strict

var timeDelay : double = 2;
var timer : double = 0;
var isInstance : boolean = true;
var beginCountdown : boolean = false;
var explodeOnDestruct : boolean = true;

function Start () 
{
	if (isInstance)
	{
		beginCountdown = true;
	}
}

function Update () 
{
	if (beginCountdown)
	{
		timer += Time.deltaTime;
		if (timer >= timeDelay)
		{
			if (explodeOnDestruct)
				Instantiate(Resources.Load("Missed Target FX"), gameObject.transform.position, Quaternion.identity);
				
			Destroy (gameObject);
			beginCountdown = false;
		}
	}
}