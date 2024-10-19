using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PigNPC : NPCInteraction
{
    [SerializeField] private Mission _missionToGiveAfterPigCaught;
    [SerializeField] private NPCInteraction _snailNPC;

    [SerializeField] private PigHealth _pigHealth;

    protected override void BeforeStart()
    {
        
    }

    protected override void GetSomethingAfterQuestCompletionInteractionCompleted()
    {
        missionManager.AddMission(_missionToGiveAfterPigCaught, null);
        _snailNPC.ChangeNPCState(State.Uninteracted);
    }

    protected override void StartQuestExtraLogic()
    {
        GetComponent<PigDrone>().ChangeActiveState(true);

        _pigHealth._catchPigMission = (CatchPigMission)_missionToGive;
    }
    protected override void DuringUpdate()
    {
        
    }
}
