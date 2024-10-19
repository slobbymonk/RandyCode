using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BananaDropping : MonoBehaviour
{
    [SerializeField] private GameObject _banana;
    [SerializeField] private Transform _droppingPosition;

    [SerializeField] private Vector2 _droppingFrequency;
    private float currentDroppingDelay;

    private void Awake()
    {
        DropBanana();
    }

    private void DropBanana()
    {
        Instantiate(_banana, _droppingPosition.position, Quaternion.identity);

        currentDroppingDelay = Random.Range(_droppingFrequency.x, _droppingFrequency.y);
    }

    void Update()
    {
        if(currentDroppingDelay <= 0)
        {
            DropBanana();
        }
        else
        {
            currentDroppingDelay -= Time.deltaTime;
        }
    }
}
