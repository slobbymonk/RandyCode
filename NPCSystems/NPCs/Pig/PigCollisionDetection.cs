using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigCollisionDetection : MonoBehaviour
{
    private PigHealth pigHealth;

    private void Awake()
    {
        pigHealth = FindObjectOfType<PigHealth>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (pigHealth.GetHealth() > 0)
            {
                pigHealth.LoseHealthPoint();
            }
        }
    }
}
