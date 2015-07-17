using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Text))]

public class NarrativeTyper : MonoBehaviour {

    public string msg = "replace";
    private Text textComp;
    public float startDelay = 2f;
    public float typeDelay = 0.01f;
   // public AudioClip clack;

    void Awake()
    {
        textComp = GetComponent<Text>();
    }

	// Use this for initialization
	void Start () {
        StartCoroutine("TypeIn");
	
	}

    public IEnumerator TypeIn()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < msg.Length; i++)
        {
            textComp.text = msg.Substring(0, i);
            //GetComponent<AudioSource>().PlayOneShot(clack);
            yield return new WaitForSeconds(typeDelay);
        }
    }

    public IEnumerator TypeOff()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < msg.Length; i++)
        {
            textComp.text = msg.Substring(0, i);
            yield return new WaitForSeconds(typeDelay);
        }
    }
}
