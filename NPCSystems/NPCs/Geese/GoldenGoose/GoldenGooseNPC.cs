using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenGooseNPC : NPCInteraction
{
    [SerializeField] private CarChaseGraph _carChase;
    [SerializeField] private BananaDropping _bananaDropping;
    [SerializeField] private FinalChaseHealth _finalChaseHealth;

    [SerializeField] private GameObject _dissapearingSmoke;

    protected override void GetSomethingAfterQuestCompletionInteractionCompleted()
    {
        _carChase.enabled = true;
        _bananaDropping.enabled = true;
        _finalChaseHealth.enabled = true;
        _finalChaseHealth._isActive = true;
    }
    protected override void AfterNotInteractibleInteraction()
    {
        Instantiate(_dissapearingSmoke, transform.position, Quaternion.identity);

        Invoke("RemoveCar", 2f);
    }
    void RemoveCar()
    {
        Destroy(transform.parent.gameObject);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartQuest();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            CompletedQuest();
        }
    }
}
