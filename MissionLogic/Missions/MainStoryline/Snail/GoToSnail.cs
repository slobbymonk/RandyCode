using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToSnail : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    private void OnTriggerEnter(Collider other)
    {
        if (_missionIsActive)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                MissionCompleted?.Invoke(this, new MissionEventArgs(this));
            }
        }
        if (other.gameObject.CompareTag("Player"))
        {
            MissionCompleted?.Invoke(this, new MissionEventArgs(this));
        }
    }
}
