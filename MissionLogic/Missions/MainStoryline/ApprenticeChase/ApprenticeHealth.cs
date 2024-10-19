using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApprenticeHealth : MonoBehaviour
{
    public bool _isActive;

    [SerializeField] private float _health;
    [SerializeField] private int damagePerVelocity;

    [SerializeField] private float currentHealth;

    private float delay = 1, delayed;

    [SerializeField] private ChaseApprenticeMission chaseApprenticeMission;

    [SerializeField] private ParticleSystem _damageParticle;

    private void Start()
    {
        currentHealth = _health;
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
        if(delayed <= 0 && _isActive)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if(currentHealth > 0)
                {
                    var rb = collision.gameObject.GetComponent<Rigidbody>();

                    currentHealth -= damagePerVelocity * rb.velocity.magnitude;


                    delayed = delay;

                    if (currentHealth <= 0)
                        chaseApprenticeMission.ChaseEnded();


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
}
