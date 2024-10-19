using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuntMissionTriggers : MonoBehaviour
{
    private DoStuntsMission stuntsMission;

    private void Awake()
    {
        stuntsMission = FindObjectOfType<DoStuntsMission>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if(stuntsMission.StuntDone(transform))
                Destroy(gameObject);
        }
    }
}
