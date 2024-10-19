using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableFinalBombMission : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    private BombManager bombManager;

    [SerializeField] private SpeedBomb _speedBomb;

    [SerializeField] private Mission _backToMechanic;

    [SerializeField] private NPCInteraction _oviraptor;
    [SerializeField] private NPCInteraction _squirrel;

    [SerializeField] private TricksManager _tricksManager;

    [SerializeField] private GameObject _tricksUnlockedText;


    [SerializeField] private BombManager _bombManager;

    [SerializeField] private GameObject _bombSpawner;

    private GameManager gameManager;
    [SerializeField] private SpawnSignals signalSpawner;

    public override void WhenStartingMission()
    {
        _bombSpawner.SetActive(true);

        signalSpawner.TriggerSpawnSignals();

        bombManager.BombsLeft();
        FindBombs();

        _speedBomb.enabled = true;
        _speedBomb._minSpeed = 40;
        _speedBomb._timeBeforeTriggered = 3;

        _oviraptor.ChangeNPCState(NPCInteraction.State.QuestCompletedWaitingForFinalTalk);
    }
    protected override void AfterStart()
    {
        if (_tricksManager == null) _tricksManager = FindObjectOfType<TricksManager>();
        if (bombManager == null) bombManager = FindObjectOfType<BombManager>();
        _speedBomb = FindObjectOfType<SpeedBomb>();

        bombManager.DiffusedABomb += DiffusedABomb;

        gameManager = FindObjectOfType<GameManager>();
        gameManager.OnLose += Lost;
    }
    void DiffusedABomb(object sender, BombDiffusionEventArgs args)
    {
        if (_missionIsActive)
        {
            foreach (var bomb in _bombManager._bombs)
            {
                if (bomb.transform == args.Bomb)
                {
                    var index = _bombManager._bombs.IndexOf(bomb);
                    RemoveIndicator(_missionID + index, args.Bomb);

                    Invoke("CheckIfWon", .4f);
                    return;
                }
            }

            //if (_missionLocation.Count <= 0)
        }
    }
    void CheckIfWon()
    {
        if (bombManager.BombsLeft() <= 0)
        {
            DisabledAllBombs();
        }
    }
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            DisabledAllBombs();
        }
    }*/
    public void DisabledAllBombs()
    {
        _tricksUnlockedText.SetActive(true);
        AudioManager.instance.Stop("CarBomb");

        _speedBomb._countdownText.gameObject.SetActive(false);
        _tricksManager._isEnabled = true;

        var missionManager = FindObjectOfType<MissionManager>();
        missionManager.AddMission(_backToMechanic, null);
        _speedBomb.enabled = false;

        _squirrel.ChangeNPCState(NPCInteraction.State.QuestCompletedWaitingForFinalTalk);
        MissionCompleted?.Invoke(this, new MissionEventArgs(this));
    }

    /*void FindBombs()
    {
        int i = 0;
        foreach (var bombLocation in _bombManager._bombs)
        {
            var bomb = bombLocation.transform;
            AddNewMissionLocation(bomb);
            AddLocationToMinimap(_missionID + i, _missionMinimapSprite, bomb);
            i++;
        }
    }*/
    void FindBombs()
    {
        var bombs = FindObjectsOfType<BombDiffuser>();
        Debug.Log(bombs.Length);
        int i = 0;
        foreach (var bombLocation in bombs)
        {
            var bomb = bombLocation.transform;
            string uniqueID = _missionID + "-" + bombLocation.gameObject.GetInstanceID(); // Use the instance ID for uniqueness
            AddNewMissionLocation(bomb);
            AddLocationToMinimap(uniqueID, _missionMinimapSprite, bomb);
            i++;
        }
    }

    void Lost(object sender, LoseEventArgs args)
    {
        if (_missionIsActive && !args.ChangeCameras)
        {
            signalSpawner.RemoveAllSignals();
            gameManager.ResetMission(this);
            Invoke("Delayed", .4f);
        }
    }
    void Delayed()
    {
        bombManager.ResetBombList();
    }
}
