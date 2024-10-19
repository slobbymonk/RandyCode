using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchPigMission : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    public void CaughtPig()
    {
        MissionCompleted?.Invoke(this, new MissionEventArgs(this));
    }
}
