using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Citizen : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private float _roamingRange;
    [SerializeField] private float _forwardRotationCorrection;
    [SerializeField] private float _crashSpeedThreshold = 2;


    [SerializeField] private GameObject _ragdoll;



    private CitizenManager manager;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        manager = FindObjectOfType<CitizenManager>();
    }

    private void Update()
    {
        Move();
    }

    void Move()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (GetRandomPoint(transform.position, _roamingRange, out point))
            {
                agent.SetDestination(point);
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            float carSpeed = collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude;

            if (carSpeed >= _crashSpeedThreshold)
            {
                if (manager != null) manager.AddHitCitizen();

                Faint();
            }
                
        }
    }


    private bool GetRandomPoint(Vector3 center, float range, out Vector3 result)
    {
        // Gives a random point as out result and returns a boolean based on whether or not it was able to get a point
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }

    public Rigidbody Faint()
    {
        // Instantiate the ragdoll at the citizen's position and rotation
        var ragdoll = Instantiate(_ragdoll, transform.position, transform.rotation);
        ragdoll.transform.localScale = transform.localScale;

        // Try to get the Rigidbody of the ragdoll
        Rigidbody ragdollRb = ragdoll.GetComponent<Rigidbody>();

        // Destroy the original citizen object
        Destroy(gameObject);

        // Return the ragdoll's Rigidbody (or null if it doesn't have one)
        return ragdollRb;
    }
}
