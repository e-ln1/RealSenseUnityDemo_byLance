#pragma strict

//var target : GameObject;
var speed : float = 1;
//var distToTarget : float;
//var minDist : float = 5;


function Update () {

	//if (target != null)
	//{
	//	distToTarget = Vector3.Distance(transform.position, target.transform.position);
	//	//print("Distance to other: " + distToTarget);
	//}
	transform.Translate(Vector3.forward * speed);

	//if (distToTarget <= minDist)
	//{
	//	//Debug.Log(distToTarget);
	//	Instantiate(Resources.Load("Missile Strike FX"), target.transform.position, Quaternion.identity);
	//	Destroy(gameObject);
	//}
}