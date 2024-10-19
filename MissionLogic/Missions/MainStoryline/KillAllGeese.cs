using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KillAllGeese : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    [SerializeField] private GeeseManager geeseManager;

    private GameManager gameManager;

    [SerializeField] private SpawnGoose _geeseSpawner;

    [SerializeField] private NPCInteraction _npc;

    [SerializeField] private Health _carHealth;

    protected override void AfterStart()
    {
        geeseManager = FindObjectOfType<GeeseManager>();

        geeseManager.KilledAGoose += KilledAGoose;


        gameManager = FindObjectOfType<GameManager>();
        gameManager.OnLose += Lost;
    }
    public override void WhenStartingMission()
    {
        _geeseSpawner.TriggerSpawnGeese();

        FindGeese();
    }
    void KilledAGoose(object sender, BombDiffusionEventArgs args)
    {
        if (_missionIsActive)
        {
            foreach (var goose in geeseManager._geese)
            {
                if (goose.transform == args.Bomb)
                {
                    var index = geeseManager._geese.IndexOf(goose);
                    RemoveIndicator(_missionID + index, args.Bomb);

                    Invoke("CheckIfWon", .4f);
                    return;
                }
            }
        }
    }

    void CheckIfWon()
    {
        Debug.Log(geeseManager.GeeseLeft());
        if (geeseManager.GeeseLeft() <= 0)
        {
            if(_carHealth != null) _carHealth.Restore();
            MissionCompleted?.Invoke(this, new MissionEventArgs(this));
        }
    }

    void FindGeese()
    {
        var geese = FindObjectsOfType<Goose>();

        geeseManager._geese = geese.ToList();
        int i = 0;
        foreach (var gooseLocation in geese)
        {
            var goose = gooseLocation.transform;
            string uniqueID = _missionID + "-" + gooseLocation.gameObject.GetInstanceID(); // Use the instance ID for uniqueness
            AddNewMissionLocation(goose);
            AddLocationToMinimap(uniqueID, _missionMinimapSprite, goose);
            i++;
        }
    }

    public void KilledAllGeese()
    {
        MissionCompleted?.Invoke(this, new MissionEventArgs(this));
    }

    void Lost(object sender, LoseEventArgs args)
    {
        if (_missionIsActive)
        {
            _geeseSpawner.RemoveAllGeese();
            geeseManager.ResetGeeseList();
            Invoke("Delayed", 1f);
        }
    }
    void Delayed()
    {
        gameManager.ResetMissionWithNPC(this, _npc);
    }

}
