using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquachOnCrash : MonoBehaviour
{
    private SquachAndStretch _squach;

    [SerializeField] private float _speedThreshold;
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private float _cooldown = 1f;
    private float cooldown;


    [SerializeField] private OneLiners _oneliner;
    [SerializeField] private string _onelinerTriggerId = "Crash";

    private void Awake()
    {
        _squach = GetComponent<SquachAndStretch>();
        _rb = GetComponent<Rigidbody>();

        if(_oneliner == null) _oneliner = FindObjectOfType<OneLiners>();
    }
    private void Update()
    {
        cooldown -= Time.deltaTime;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(cooldown <= 0)
        {
            var velocity = _rb.velocity.magnitude;
            float massOfCollisionObject = collision.gameObject.TryGetComponent<Rigidbody>(out var rb) ? rb.mass : _rb.mass+1;

            if (velocity >= _speedThreshold && massOfCollisionObject > _rb.mass)
            {
                _squach.PlaySquachAndStretch();
                if (_oneliner != null) _oneliner.PlayOneLiner(_onelinerTriggerId, false);
                cooldown = _cooldown;
            }
        }
    }
}
