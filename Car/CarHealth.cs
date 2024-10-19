using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarHealth : Health
{
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private NPCInteraction _gooseNPC;

    private void Update()
    {

        if(_currentHealth <= 0)
        {
            Death();
        }
    }
    void Death()
    {
        Instantiate(_explosionEffect, transform.position, Quaternion.identity);

        _gameManager.Lose(false, true, true, null);

        Restore();
    }
}
