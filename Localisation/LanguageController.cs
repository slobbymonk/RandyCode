using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Icons;

public class LanguageController : MonoBehaviour
{
    public static LanguageController instance;

    public event EventHandler LanguageChanged;

    public string currentLanguage;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(instance);
            instance = this;
        }

        currentLanguage = PlayerPrefs.GetString("LanguageIndex");
    }

    public void ChangeLanguageToEnglish()
    {
        ChangeLanguage("en");
    }
    public void ChangeLanguageToDutch()
    {
        ChangeLanguage("nl");
    }
    public void ChangeLanguageToSpanish()
    {
        ChangeLanguage("es");
    }
    public void ChangeLanguageToFrench()
    {
        ChangeLanguage("fr");
    }

    public void ChangeLanguage(string language)
    {
        //Changes the language index
        PlayerPrefs.SetString("LanguageIndex", language);
        currentLanguage = language;
        //Tells all subscribed functions that the language was changed
        LanguageChanged?.Invoke(this, new EventArgs());
    }
}
