using UnityEngine;
using TMPro;
using System;
using static Unity.VisualScripting.Member;

public class LanguageText : LanguageComponent<TMP_Text, string>
{
    private void Awake()
    {
        _source = GetComponent<TMP_Text>();
    }

    protected override void SetLanguage()
    {
        //Set the text to the correct language when changing the language
        _source.text = _languageVersions[PlayerPrefs.GetInt("LanguageIndex")];
    }
}
