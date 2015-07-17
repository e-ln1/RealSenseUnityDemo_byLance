#pragma strict

var target : GameObject;
var distToTarget : float;
var minDistAllowed : float = 30;
var speed : float = 5;
//var sparkPrefab : GameObject;
var spark : GameObject;
//var fired : boolean;


function Start () 
{
	spark = Resources.Load("KRG Round Spark") as GameObject;
	spark = Instantiate(spark, gameObject.transform.position, Quaternion.identity);
	spark.transform.position.z += 50;
    
}

function Update () 
{
	if (spark != null)
	{
		spark.transform.LookAt(gameObject.transform);
	}
	
	gameObject.transform.Translate(Vector3.forward * speed);
	
	if (target != null)
	{
		distToTarget = Vector3.Distance(gameObject.transform.position, target.transform.position);
	
		if (distToTarget < minDistAllowed)
		{
			Instantiate(Resources.Load("Rail Gun Strike FX"), target.transform.position, Quaternion.identity);
			Destroy(gameObject);
		}
	}
}