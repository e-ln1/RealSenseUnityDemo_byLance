#pragma strict

@HideInInspector
var fire : boolean = false;
@HideInInspector
var missile : GameObject;
@HideInInspector
var target : GameObject;

function Start () 
{
    missile = Resources.Load("Missile2", GameObject);
}

function Update () 
{
		missileDirectHit();
}

function missileDirectHit() {
    if (missile == null)
    {
        missile = Resources.Load("Missile2", GameObject);
    }
	if (fire)
	{
		missile = Instantiate(missile, transform.position, Quaternion.identity);
		missile.transform.LookAt(target.transform);
		//missile.GetComponent(Missile).target = target;
		//Debug.Log(missile.GetComponent(Missile).target.transform.position);
		fire = false;
	}
}