using UnityEngine;
using System.Collections;

public class PirateShipMovement : MonoBehaviour {

    [Range(0.2f, 3f)]
	public float smoothing = 1f;
	Vector3 target;
	[Range(0f, 0.4f)]
	public float minX = 0.1f;
	[Range(0.6f, 1f)]
	public float maxX = 0.9f;
    [Range(0.4f, 0.7f)]
    public float minY = 0.5f;
    [Range(0.7f, 1f)]
    public float maxY = 0.96f;

    float distanceFromCamera;

    public bool isDestroyed = false;
    public bool isActive = false;
    public float timeBetweenMove = 0.5f;
	PirateShipStats shipStatsScript;
	SpriteRenderer pirateSprite;

    Animator pirateShipAC;

	void Start() {
		distanceFromCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
		shipStatsScript = GetComponent<PirateShipStats>();
        //Debug.Log(distanceFromCamera);
        pirateShipAC = GetComponent<Animator>();
		pirateSprite = GetComponent<SpriteRenderer>();
	}
	
	void Update ()
	{
        if (isDestroyed)
        {
            isActive = false;
            Instantiate(Resources.Load("Missile Strike FX"), transform.position, Quaternion.identity);
            pirateShipAC.SetTrigger("IsDestroyed");
            if (pirateSprite.color.a == 0)
            {
                Destroy(gameObject);
                //Application.LoadLevel();
                // mission success
            }
        }
		if(isActive)
		{
			isActive = false;
			target = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), distanceFromCamera));
			StartCoroutine(MyCoroutine(target));			
		}
	}

	IEnumerator MyCoroutine (Vector3 target)
	{
		while(Vector3.Distance(transform.position, target) > 10f)
		{
			transform.position = Vector3.Lerp(transform.position, target, smoothing * Time.deltaTime);
			yield return null;
		}
        if (isDestroyed)
		{
            isActive = false;
            yield return null;
		}
		else
		{
			shipStatsScript.fire = true;
		}
        yield return new WaitForSeconds(timeBetweenMove);
        if(!isDestroyed)
            isActive = true;
	}

    void destructionExplosion()
    {
        Instantiate(Resources.Load("Missile Strike FX"), transform.position, Quaternion.identity);
    }

}