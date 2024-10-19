using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalChaseHealth : MonoBehaviour
{
    public bool _isActive;

    [SerializeField] private float _health;
    [SerializeField] private int damagePerVelocity;

    [SerializeField] private float currentHealth;

    private float delay = 1, delayed;

    [SerializeField] private NPCInteraction _goldenGooseNPC;
    [SerializeField] private CarChaseGraph _carChase;
    [SerializeField] private BananaDropping _bananaDropping;

    [SerializeField] private ParticleSystem _damageParticle;

    private void Start()
    {
        currentHealth = _health;

        _goldenGooseNPC.ChangeNPCState(NPCInteraction.State.Disabled);
    }

    private void Update()
    {
        if (delayed > 0)
        {
            delayed -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (delayed <= 0 && _isActive)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (currentHealth > 0)
                {
                    var rb = collision.gameObject.GetComponent<Rigidbody>();

                    currentHealth -= damagePerVelocity * rb.velocity.magnitude;


                    delayed = delay;

                    if (currentHealth <= 0)
                        ChaseEnded();


                    var emmision = _damageParticle.emission;
                    Debug.Log((_health - currentHealth) / (_health / 5));
                    emmision.rateOverTime = (_health - currentHealth) / (_health / 5);

                }
                else
                {
                    currentHealth = 0;
                }
            }
        }
    }
    void ChaseEnded()
    {
        _goldenGooseNPC.ChangeNPCState(NPCInteraction.State.NotInteractable);
        _goldenGooseNPC.StartInteraction();

        _carChase.enabled = false; 
        _bananaDropping.enabled = false;
    }
}
