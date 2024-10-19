using System.Collections.Generic;
using UnityEngine;

public class PigDrone : MonoBehaviour
{
    [Header("Height Control")]
    [SerializeField] private float desiredHeight = 10f; // Desired height of the drone
    [SerializeField] private float heightStabilizationForce = 5f; // Force applied to stabilize height
    [SerializeField] private float heightDamping = 0.5f; // Damping to control height bobbing
    [SerializeField] private LayerMask groundLayer; // Layer mask to detect the ground
    [SerializeField] private float raycastOffset = 1f; // Offset for raycast to detect ground

    [Header("Bobbing Effect")]
    [SerializeField] private float bobbingAmplitude = 0.2f; // Amplitude of the bobbing effect
    [SerializeField] private float bobbingFrequency = 1f; // Frequency of the bobbing effect

    [Header("Balance Control")]
    [SerializeField] private float balanceStabilizationForce = 2f; // Force applied to stabilize rotation
    [SerializeField] private float balanceDamping = 1f; // Damping to control rotation stability

    [Header("Position Control")]
    [SerializeField] private float positionStabilizationForce = 2f; // Force applied to stabilize position
    [SerializeField] private float positionDamping = 1f; // Damping to control position stability

    [Header("Rotation Settings")]
    [SerializeField] private float turnSmoothing = 0.1f; // Smoothness of the rotation
    [SerializeField] private float rotationOffset = 0f; // Offset to correct the rotation if needed
    [SerializeField] private float maxForce = 50f; // Maximum force to apply to prevent excessive force

    private Rigidbody rb;
    private float currentHeight; // Current height of the drone
    private float heightError; // Error between current height and desired height
    private float heightCorrection; // Correction force for height stabilization


    public bool droneIsActive;

    [Header("Effects")]
    [SerializeField] private BladeRotator _droneBlades;
    [SerializeField] private WaypointMovement _waypointMovement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        ChangeActiveState(droneIsActive);
    }
    public void ChangeActiveState(bool state)
    {
        droneIsActive = state;

        if (droneIsActive)
        {
            _waypointMovement.enabled = true;
            _droneBlades.enabled = true;
        }
        else
        {
            _waypointMovement.enabled = false;
            _droneBlades.enabled = false;
        }
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.I))
        {
            ChangeActiveState(true);
        }
        if (droneIsActive)
        {
            HandleHeightStabilization();
            StabilizeRotation();
            StabilizePosition();
        }
    }

    private void HandleHeightStabilization()
    {
        Vector3 rayOrigin = rb.transform.position + Vector3.up * raycastOffset;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            currentHeight = hit.distance - raycastOffset;
            heightError = desiredHeight - currentHeight;
            heightCorrection = heightError * heightStabilizationForce - rb.velocity.y * heightDamping;

            float bobbingEffect = Mathf.Sin(Time.time * bobbingFrequency) * bobbingAmplitude;
            Vector3 force = transform.up * Mathf.Clamp(heightCorrection + bobbingEffect, -maxForce, maxForce);
            rb.AddForce(force, ForceMode.VelocityChange);
        }
    }

    private void StabilizeRotation()
    {
        Quaternion desiredRotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f);
        Quaternion rotationError = desiredRotation * Quaternion.Inverse(rb.rotation);
        Vector3 angularVelocityError = new Vector3(rotationError.x, rotationError.y, rotationError.z) * balanceStabilizationForce;
        Vector3 angularCorrection = angularVelocityError - rb.angularVelocity * balanceDamping;
        rb.AddTorque(Vector3.ClampMagnitude(angularCorrection, maxForce), ForceMode.VelocityChange);
    }

    private void StabilizePosition()
    {
        Vector3 positionError = -rb.velocity * positionStabilizationForce;
        Vector3 positionCorrection = positionError - rb.velocity * positionDamping;
        rb.AddForce(Vector3.ClampMagnitude(positionCorrection, maxForce), ForceMode.VelocityChange);
    }
}
