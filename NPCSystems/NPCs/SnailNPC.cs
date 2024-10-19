using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailNPC : NPCInteraction
{
    [SerializeField] private BoxAttachment _towingMechanic;

    [SerializeField] private AudioSource _bombSoundSource;
    [SerializeField] private float _delayBeforeBombStarts;
    [SerializeField] private float _delayBeforeMissionStarts;

    [SerializeField] private Mission _goToMechanicWithJimmyMission;
    [SerializeField] private bool _firstSnail;


    [SerializeField] private GameObject _countDown;
    [SerializeField] private GameObject _spedometer;


    [SerializeField] private float _timeBeforeEnablingTimer;

    protected override void GetSomethingAfterQuestCompletionInteractionStarted()
    {
        if (_firstSnail)
        {
            _towingMechanic.isOn = false;

            Invoke("StartSound", _delayBeforeBombStarts);
        }
    }
    void StartSound()
    {
        _bombSoundSource.Play();
        Invoke("GiveMission", _delayBeforeMissionStarts);
    }
    protected override void AfterQuestionCompletion()
    {
        if (!_firstSnail)
        {
            _countDown.SetActive(false);
            _spedometer.SetActive(true);
        }
    }
    void GiveMission()
    {
        _towingMechanic.isOn = true;
        missionManager.AddMission(_goToMechanicWithJimmyMission, this);
    }
    protected override void StartQuestExtraLogic(){
        _towingMechanic.enabled = true;
    }

    protected override void GetSomethingAfterQuestCompletionInteractionCompleted()
    {
        
    }

    public void AcceptFateInteraction()
    {
        StartInteraction();

        Invoke("StartTimer", _timeBeforeEnablingTimer);
    }
    void StartTimer()
    {
        if (_firstSnail)
        {
            _countDown.SetActive(false);
            _spedometer.SetActive(true);
        }
    }
}
