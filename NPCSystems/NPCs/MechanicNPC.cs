using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanicNPC : NPCInteraction
{
    private CarController carController;
    private TricksManager tricksManager;

    [Header("Upon quest completion")]
    [SerializeField] private NPCInteraction _pigNPC;
    [SerializeField] private Mission _goToPigMission;

    [Header("Snail logic")]
    [SerializeField] private GameObject _oldSnailNPC, _newSnailNPC;
    [SerializeField] private SnailNPC _snailNPC;

    private bool _firstSnailTime;

    [SerializeField] private enum Version
    {
        Finale,
        Snail,
        Neither
    }
    [SerializeField] private Version _version;

    protected override void BeforeStart()
    {
        carController = GameObject.FindGameObjectWithTag("Player").GetComponent<CarController>();
        tricksManager = GameObject.FindGameObjectWithTag("Player").GetComponent<TricksManager>();
    }

    protected override void GetSomethingAfterQuestCompletionInteractionCompleted()
    {
        if(_version == Version.Finale)
        {
            missionManager.AddMission(_goToPigMission, null);
            _pigNPC.ChangeNPCState(State.Uninteracted);
        }
    }

    protected override void TriggeredAfterInteraction()
    {
        if( _version == Version.Snail && !_firstSnailTime)
        {
            _newSnailNPC.transform.position = _oldSnailNPC.transform.position;
            _newSnailNPC.transform.rotation = _oldSnailNPC.transform.rotation;
            _newSnailNPC.transform.localScale = _oldSnailNPC.transform.localScale;

            _oldSnailNPC.SetActive(false);
            _newSnailNPC.SetActive(true);

            _snailNPC.AcceptFateInteraction();

            _firstSnailTime = true;
        }

        carController._airControlIsActive = true;
    }
    protected override void DuringUpdate()
    {
        
    }
}
