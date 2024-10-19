using UnityEngine;
using System.Collections;

public class GooseShooting : MonoBehaviour
{
    [Header("Goose Settings")]
    public float rotationOffset = 0f;   // Rotation offset for the goose
    public float detectionDelay = 1f;   // Delay before the goose stops targeting after the player leaves
    public float shootDelay = 2f;       // Public delay between shots (adjustable in the Inspector)
    private float delayed = 1f;         // Internal variable to track the delay time
    public float projectileSpeed = 20f; // Speed of the projectile
    public GameObject projectilePrefab; // Prefab of the projectile to shoot
    public Transform shootingPoint;     // Point from which the projectiles will be shot

    private Transform target;           // The current target (Player)
    private bool canShoot = true;       // Whether the goose can shoot
    private Coroutine shootCoroutine;   // Reference to the ongoing shoot coroutine

    [SerializeField] private AudioSource _shootSound;
    [SerializeField] private ParticleSystem _shootingEffect;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = other.transform;
            StopAllCoroutines(); // Stop any ongoing coroutine that stops targeting
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(StopTargetingAfterDelay());
        }
    }

    void Update()
    {
        if (target)
        {
            LookAtTarget();

            if (delayed <= 0)
            {
                Shoot();
                delayed = shootDelay;
            }
            else
            {
                delayed -= Time.deltaTime;
            }
        }
    }
    [SerializeField] private float rotateSpeed = 5;
    void LookAtTarget()
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Keep the rotation only on the Y axis
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        targetRotation *= Quaternion.Euler(0, rotationOffset, 0); // Apply rotation offset
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }
    void Shoot()
    {
        if(_shootSound != null) _shootSound.Play();
        if (_shootingEffect != null) _shootingEffect.Play();

        GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, shootingPoint.rotation);
        projectile.transform.eulerAngles = new Vector3(-89.98f, projectile.transform.eulerAngles.y
            , projectile.transform.eulerAngles.z);

        projectile.GetComponent<Baguette>().instantiator = gameObject;

        if (projectile != null)
        {
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = shootingPoint.forward * projectileSpeed;
                Debug.Log("Bullet shot with speed: " + projectileSpeed);
            }
            else
            {
                Debug.LogError("No Rigidbody component found on the projectile!");
            }
        }
        else
        {
            Debug.LogError("Projectile instantiation failed!");
        }
    }

    IEnumerator StopTargetingAfterDelay()
    {
        yield return new WaitForSeconds(detectionDelay);
        target = null;
    }
}
