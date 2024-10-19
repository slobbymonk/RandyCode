using UnityEngine;
using System;

public abstract class LanguageComponent<Source, LanguageVersion> : MonoBehaviour
{
    //Content displayer, to be defined by the child class
    protected Source _source; 
    //English, Dutch & Spanish versions of the content
    [SerializeField] protected LanguageVersion[] _languageVersions = new LanguageVersion[3]; 


    private void Start()
    {
        //Subscribe to when the language is changed
        LanguageController.instance.LanguageChanged += SetLanguageEvent; 
        SetLanguage();
    }

    private void OnDestroy()
    {
        //Unsubscribe to when the language is changed as to stop unnecesary calls
        LanguageController.instance.LanguageChanged -= SetLanguageEvent; 
    }

    private void SetLanguageEvent(object sender, EventArgs args)
    {
        SetLanguage(); //Changed the language
    }

    protected abstract void SetLanguage(); //To be defined by the child classes
}
