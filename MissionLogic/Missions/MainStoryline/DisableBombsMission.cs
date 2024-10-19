using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableBombsMission : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    private BombManager bombManager;

    private GameManager gameManager;

    [SerializeField] private SpawnSignals signalSpawner;

    protected override void AfterStart()
    {
        bombManager = FindObjectOfType<BombManager>();

        bombManager.DiffusedABomb += DiffusedABomb;


        gameManager = FindObjectOfType<GameManager>();
        gameManager.OnLose += Lost;
    }
    public override void WhenStartingMission()
    {
        signalSpawner.TriggerSpawnSignals();

        FindBombs();
    }
    void DiffusedABomb(object sender, BombDiffusionEventArgs args)
    {
        Debug.Log("Checking if active");
        if (_missionIsActive)
        {
            foreach (var bomb in bombManager._bombs)
            {
                if(bomb.transform == args.Bomb)
                {
                    var index = bombManager._bombs.IndexOf(bomb);
                    RemoveIndicator(_missionID + index, args.Bomb);

                    Invoke("CheckIfWon", .4f);
                    return;
                }
            }
        }
    }

    void CheckIfWon()
    {
        if (bombManager.BombsLeft() <= 0)
        {
            gameManager.Win();
        }
    }
    /*public void FindBombs()
    {
        foreach (var bombLocation in bombManager._bombs)
        {
            var bomb = bombLocation.transform;
            AddNewMissionLocation(bomb);
        }
    }*/
    void FindBombs()
    {
        var bombs = FindObjectsOfType<BombDiffuser>();
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

    public void DisabledAllBombs()
    {
        gameManager.IsBomb2();
        MissionCompleted?.Invoke(this, new MissionEventArgs(this));
    }

    void Lost(object sender, LoseEventArgs args)
    {
        if (_missionIsActive)
        {
            signalSpawner.RemoveAllSignals();
            gameManager.ResetMission(this);
            Invoke("Delayed", 1f);
        }
    }
    void Delayed()
    {
        bombManager.ResetBombList();
    }

}
