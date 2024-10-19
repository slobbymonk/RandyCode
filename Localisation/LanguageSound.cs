using TMPro;
using UnityEngine;

public class LanguageSound : LanguageComponent<AudioSource, AudioClip>
{
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    protected override void SetLanguage()
    {
        //Save how long the clip has been playing
        var playingLength = _source.time;
        //Only play again after changing the clip if it was already playing
        bool wasPlaying = _source.isPlaying;
        //Replace clip
        _source.clip = _languageVersions[PlayerPrefs.GetInt("LanguageIndex")];

        //Play the audiosource again, because it automatically stops after changing the clip
        if(wasPlaying) _source.Play();

        //Set playing time to be the same as it was before changing clips
        _source.time = playingLength;

    }
}