using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperNPC : NPCInteraction
{
    private CarController carController;
    private TricksManager tricksManager;

    protected override void BeforeStart()
    {
        base.BeforeStart();

        tricksManager = FindObjectOfType<TricksManager>();
        carController = GameObject.FindGameObjectWithTag("Player").GetComponent<CarController>();
    }
    protected override void DuringUpdate()
    {
        base.DuringUpdate();

        //_waitingForQuestCompletionInteractions[0].text = "You still have " + (10 - tricksManager.gameTotalFlips).ToString() + " flips to do.";
    }

    protected override void GetSomethingAfterQuestCompletionInteractionCompleted()
    {
        carController._jumpIsActive = true;
    }
}
