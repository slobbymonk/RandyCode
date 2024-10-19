using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Towable : MonoBehaviour
{
    [SerializeField] private bool _noRigidbody;

    [Tooltip("Only needs to be filled in if no rigidbody exists before runtime")]
    [SerializeField] private float _mass;

    public Transform _connectionPoint;


    private void Awake()
    {
        if(GetComponent<Rigidbody>() == null && !_noRigidbody)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = _mass;
        }
    }
}
