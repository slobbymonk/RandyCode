using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToApprenticeHouse : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    [SerializeField] private NPCInteraction _apprenticeNPC;

    [SerializeField] private SimpleCarAIWithSteering carAi;

    [SerializeField] private MissionManager missionManager;
    [SerializeField] private Mission chaseApprentice;

    public void Arrived()
    {
        carAi.enabled = true;
        missionManager.AddMission(chaseApprentice, null);

        MissionCompleted?.Invoke(this, new MissionEventArgs(this));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_missionIsActive)
            {
                _apprenticeNPC.ChangeNPCState(NPCInteraction.State.Disabled);
                _apprenticeNPC.GetComponent<ApprenticeHealth>()._isActive = true;
                Arrived();
                Invoke("Delay", .5f);
            }
        }
    }
    void Delay()
    {
        Destroy(this);
    }

    /*private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if(_missionIsActive)
            {
                _apprenticeNPC._state = NPCInteraction.State.Uninteracted;
                Arrived();
                Destroy(this);
            }
        }
    }*/
}
