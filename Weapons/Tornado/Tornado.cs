using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : MonoBehaviour
{
    [Header("Tornado Settings")]
    [SerializeField] private float radius = 5f; // Radius of the tornado's effect
    [SerializeField] private float pullStrength = 10f; // Strength of the pulling effect
    [SerializeField] private float liftStrength = 5f; // Strength of the upward lift effect
    [SerializeField] private float rotationSpeed = 100f; // Speed of rotation around the tornado
    [SerializeField] private float duration = 10f; // Duration the tornado lasts
    [SerializeField] private float dissolveDuration = 2f; // Duration for dissolve effect
    [SerializeField] private float finalDissolveValue = 1f; // Final value for dissolve
    [SerializeField] private MeshRenderer tornadoMesh; // Mesh renderer for the tornado

    private Material tornadoMaterial;
    private Dictionary<Rigidbody, float> orbitingNPCs = new Dictionary<Rigidbody, float>(); // Track NPCs in the tornado

    private void Start()
    {
        tornadoMaterial = tornadoMesh.material; // Get the tornado material
        Invoke(nameof(StartDissolve), duration - dissolveDuration); // Schedule dissolve
    }

    private void StartDissolve()
    {
        StartCoroutine(DissolveTornado());
    }

    private IEnumerator DissolveTornado()
    {
        float currentDissolve = tornadoMaterial.GetFloat("_Dissolve"); // Current dissolve value from shader
        float elapsedTime = 0f;

        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(currentDissolve, finalDissolveValue, elapsedTime / dissolveDuration);
            tornadoMaterial.SetFloat("_Dissolve", dissolveValue);
            yield return null; // Wait for the next frame
        }

        Destroy(gameObject); // Destroy the tornado once dissolved
    }

    private void FixedUpdate()
    {
        MoveTornado();
        ApplyEffectsToNPCs();
    }

    private void MoveTornado()
    {
        transform.position += transform.forward * Time.fixedDeltaTime; // Move tornado forward
    }

    private void ApplyEffectsToNPCs()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius * .8f);
        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            Citizen citizen = collider.GetComponent<Citizen>();
            Health health = collider.GetComponent<Health>();

            if (rb != null && collider.gameObject != gameObject)
            {
                ApplyForceAndRotation(rb);
                TrackNPCInOrbit(rb, citizen);
                TrackNPCInOrbit(rb, health);
            }
        }
    }

    private void ApplyForceAndRotation(Rigidbody rb)
    {
        // Calculate direction to the tornado's center
        Vector3 direction = (transform.position - rb.position).normalized;

        // Calculate pull and lift forces
        rb.AddForce(direction * pullStrength);
        rb.AddForce(Vector3.up * liftStrength);

        // Calculate rotation around the tornado
        Vector3 rotationDirection = Vector3.Cross(Vector3.up, direction);
        rb.AddTorque(rotationDirection * rotationSpeed * Time.fixedDeltaTime, ForceMode.Force);
    }

    private void TrackNPCInOrbit(Rigidbody rb, Citizen citizen)
    {
        if (citizen != null)
        {
            citizen.Faint(); // Call faint immediately when the citizen enters the tornado's radius
        }
    }
    private void TrackNPCInOrbit(Rigidbody rb, Health health)
    {
        if (health != null)
        {
            health.DeathLogic(); // Call faint immediately when the citizen enters the tornado's radius
        }
    }
    public void SetStrength(float chargeRatio)
    {
        pullStrength *= chargeRatio; // Adjust tornado pulling strength
        rotationSpeed *= chargeRatio; // Adjust tornado rotation speed
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius; // Set tornado radius
    }

    private void OnDrawGizmos()
    {
        // Draw a sphere to visualize the tornado's effect radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
