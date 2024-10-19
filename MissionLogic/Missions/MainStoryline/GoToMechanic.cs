using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToMechanic : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_missionIsActive)
            {
                MissionCompleted?.Invoke(this, new MissionEventArgs(this));
                Destroy(this);
            }
        }
    }
}
