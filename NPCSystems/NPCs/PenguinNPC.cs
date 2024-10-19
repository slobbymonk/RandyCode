using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinNPC : NPCInteraction
{
    [Header("Penguin specific variables")]
    [SerializeField] private CarController _carController;
    [SerializeField] private float _newMaxBoostTime;

    private void Awake()
    {
        if(_carController == null) _carController = FindObjectOfType<CarController>();
    }

    protected override void GetSomethingAfterQuestCompletionInteractionCompleted()
    {
        _carController.ChangeMaxBoostTime(_newMaxBoostTime);
    }
    protected override void DuringUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            CompletedQuest();
        }
    }
}
