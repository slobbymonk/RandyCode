using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BringSnailToPier : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    [SerializeField] private GameObject _finaleSquirrel, _snailSquirrel;

    [SerializeField] private NPCInteraction _snailNPC;

    private bool once;

    private void OnTriggerEnter(Collider other)
    {
        if (_missionIsActive && !once)
        {
            if (other.gameObject.GetComponent<SnailNPC>())
            {
                MissionCompleted?.Invoke(this, new MissionEventArgs(this));

                _snailNPC.ChangeNPCState(NPCInteraction.State.QuestCompletedWaitingForFinalTalk);
                Invoke("StartInteraction", 1);

                once = true;

                _finaleSquirrel.SetActive(false);
                _snailSquirrel.SetActive(true);
            }
        }
    }
    void StartInteraction()
    {
        Debug.Log("Talking");
        _snailNPC._interactionCoroutine = null;
        _snailNPC.StartInteraction();
    }
}
