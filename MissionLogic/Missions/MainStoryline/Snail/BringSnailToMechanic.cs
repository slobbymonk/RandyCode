using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BringSnailToMechanic : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    [SerializeField] private BoxAttachment _boxAttachment;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_missionIsActive)
            {
                MissionCompleted?.Invoke(this, new MissionEventArgs(this));

                _boxAttachment.DetachBox();

                Destroy(this);
            }
        }
    }
}
