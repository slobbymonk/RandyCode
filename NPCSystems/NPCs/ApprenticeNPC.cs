using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApprenticeNPC : NPCInteraction
{
    protected override void GetSomethingAfterQuestCompletionInteractionCompleted()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Invoke("Delay", .5f);
        }
    }
    void Delay()
    {
        StartInteraction();
    }
}
