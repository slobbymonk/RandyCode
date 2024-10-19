using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Do10Flips : Mission
{
    private TricksManager tricksManager;

    public override event EventHandler<MissionEventArgs> MissionCompleted;

    private void Awake()
    {
        tricksManager = FindObjectOfType<TricksManager>();
    }

    private void Update()
    {
        if (tricksManager.gameTotalFlips >= 10)
            MissionCompleted?.Invoke(this, new MissionEventArgs(this));
    }
}
