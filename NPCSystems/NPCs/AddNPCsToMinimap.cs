using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddNPCsToMinimap : MonoBehaviour
{
    ///After the bomb is dismanteled I want the NPCs to show up

    [SerializeField] private DirectionIndicator[] _npcIndicators;
    private DirectionIndicatorManager directionIndicatorManager;


    [SerializeField] private GameManager _gameManager;

    private void Awake()
    {
        directionIndicatorManager = FindObjectOfType<DirectionIndicatorManager>();
        _gameManager = GetComponent<GameManager>();
        _gameManager.OnWin += DetectBombsDiffusion;

    }

    void DetectBombsDiffusion(object sender, EventArgs args)
    {
        StartCoroutine(AddIndicators());
    }
    IEnumerator AddIndicators()
    {
        yield return new WaitForSecondsRealtime(20);

        foreach (var indicator in _npcIndicators)
        {
            directionIndicatorManager.AddDirectionIndicator(indicator.indicatorID, indicator.indictorSprite, indicator.target);
        }
    }
}
