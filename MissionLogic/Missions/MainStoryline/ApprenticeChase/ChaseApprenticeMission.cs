using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseApprenticeMission : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    [SerializeField] private NPCInteraction apprenticeNPC;

    [SerializeField] private SimpleCarAIWithSteering carAI;

    [SerializeField] private GameObject squirrel, finalSquirrel;
     
    public void ChaseEnded()
    {
        squirrel.SetActive(false);
        finalSquirrel.SetActive(true);

        apprenticeNPC.StopAllCoroutines();
        apprenticeNPC.ChangeNPCState(NPCInteraction.State.Uninteracted);
        apprenticeNPC.StartInteraction();
        carAI.enabled = false;

        MissionCompleted?.Invoke(this, new MissionEventArgs(this));
    }
}
