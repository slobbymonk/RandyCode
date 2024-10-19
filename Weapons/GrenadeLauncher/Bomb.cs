using System.Collections;
using UnityEngine;

public class Bomb : Projectile
{
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private float _explosionForce = 700f;   // Force of the explosion
    [SerializeField] private float _explosionRadius = 5f;    // Radius of the explosion
    [SerializeField] private float _upwardModifier = 1f;     // Makes the explosion push upwards

    [SerializeField] private float _growingTimeOnAwake = 2f; // Time to grow to full size
    private Vector3 _finalSize;

    [SerializeField] private CameraShake cameraShake;

    [SerializeField] private OneLiners _oneLiners;

    private void Awake()
    {
        _finalSize = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(GrowBomb());

        cameraShake = Camera.main.GetComponent<CameraShake>();
        _oneLiners = FindObjectOfType<OneLiners>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            // Make all citizens in the blast radius faint
            KillThingsInBlastRadius();

            // Apply explosion force after citizens faint
            ApplyExplosionForce();

            // Explosion logic
            Explode();
        }
    }

    private IEnumerator GrowBomb()
    {
        Vector3 originalSize = transform.localScale; // Starting size
        float elapsedTime = 0f;

        // Scale from zero to the final size
        while (elapsedTime < _growingTimeOnAwake)
        {
            transform.localScale = Vector3.Lerp(originalSize, _finalSize, elapsedTime / _growingTimeOnAwake);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure it reaches the final size
        transform.localScale = _finalSize;
    }

    void KillThingsInBlastRadius()
    {
        // Find all nearby colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);
        var i = 0;
        foreach (Collider nearbyObject in colliders)
        {
            if (!nearbyObject.isTrigger)
            {
                if (nearbyObject.gameObject.TryGetComponent<Citizen>(out var citizen))
                {
                    // Make the citizen faint, activating the ragdoll
                    citizen.Faint();
                    i++;
                }
                if (nearbyObject.gameObject.TryGetComponent<Health>(out var health))
                {
                    // Make the citizen faint, activating the ragdoll
                    health.DeathLogic();
                    i++;
                }
            }

        }
        if(i > 0)
        {
            _oneLiners.PlayOneLiner("BombHit", true); 
        }
    }

    void ApplyExplosionForce()
    {
        // Find all nearby rigidbodies within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Apply explosion force to each rigidbody
                rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, _upwardModifier, ForceMode.Impulse);
            }

            if(nearbyObject.GetComponent<Tornado>()!= null)
            {
                Destroy(nearbyObject.gameObject);
            }
        }
    }
    void Explode()
    {
        Debug.Log("Exploded");

        cameraShake.ShakeCameraWithDistance(transform.position, .5f, 40);

        // Instantiate explosion effect
        Instantiate(_explosionEffect, transform.position, Quaternion.identity);

        // Destroy the bomb game object
        Destroy(gameObject);
    }
}
