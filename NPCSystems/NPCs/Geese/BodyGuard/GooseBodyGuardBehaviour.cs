using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GooseBodyGuardBehaviour : GooseBaseBehaviour
{
    private float delayed;
    public float shootDelay = 2f;            // Delay between shots
    public float projectileSpeed = 20f;      // Speed of the projectile
    public GameObject projectilePrefab;      // Prefab of the projectile to shoot
    public Transform shootingPoint;          // Point from which the projectiles will be shot

    private bool canShoot = true;            // Whether the goose can shoot
    private Coroutine shootCoroutine;

    [SerializeField] private AudioSource _shootSound;
    [SerializeField] private ParticleSystem _shootingEffect;

    protected override void InAwake()
    {
        delayed = shootDelay;
    }
    protected override void WhenLockedOnTarget()
    {
        // If the player is within range, look at the player and attack
        LookAtTarget();

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Move towards the player only if they are out of a certain range
        if (distanceToTarget > stopChasingDistance)
        {
            agent.SetDestination(target.position);
            AnimateMovement(1);
            wasNotMoving = false;
        }
        else
        {
            if (!wasNotMoving)
            {
                wasNotMoving = true;
                squachEffect.PlaySquachAndStretch();
            }
            agent.ResetPath(); // Stop moving if within range
            ResetTilt();
        }

        if (delayed <= 0)
        {
            Attack();
            delayed = shootDelay;
        }
        else
        {
            delayed -= Time.deltaTime;
        }
    }

    public override void Attack()
    {
        if (_shootSound != null) _shootSound.Play();
        if (_shootingEffect != null) _shootingEffect.Play();

        squachEffect.PlaySquachAndStretch();

        GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, shootingPoint.rotation);

        projectile.GetComponent<Baguette>().instantiator = gameObject;

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
}
