using UnityEngine;
using System.Collections;

public class NarrativeManager : MonoBehaviour {

    private NarrativeTyper typer;
    //private Animator menuAnimAC;
    private bool menuOn = false;

    void Awake()
    {
        typer = GetComponentInChildren<NarrativeTyper>();
        //menuAnimAC = GetComponent<Animator>();
    
    }

    public void beginMenu()
    {
        if (!menuOn)
        {
           // menuAnimAC.SetTrigger("FadeIn");
            typer.StartCoroutine("TypeIn");
            menuOn = true;
        }
        else
        {
           // menuAnimAC.SetTrigger("FadeOut");
            typer.StartCoroutine("TypeOff");
            menuOn = false;
        }
    }
}
