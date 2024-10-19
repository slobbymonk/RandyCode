using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingGooseBehaviour : GooseBaseBehaviour
{
    [SerializeField] private float bobSpeedMultiplier;
    [SerializeField] private float chargingSpeed;


    [SerializeField] private GooseHealth health;

    [Header("Attack")]
    [SerializeField] private float chargingKnockbackForce;
    [SerializeField] private float bumpingCooldownTime = .2f;
    private bool bumpingIsCooling;

    protected override void WhenLockedOnTarget()
    {
        if (!wasNotMoving)
        {
            health.SetInvulnerableState(true);
        }

        LookAtTarget();

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        agent.SetDestination(target.position);
        agent.speed = chargingSpeed;
        AnimateMovement(bobSpeedMultiplier);
        wasNotMoving = false;
    }
    protected override void WhenLostTarget()
    {
        if (!wasNotMoving)
        {
            health.SetInvulnerableState(false);
            squachEffect.PlaySquachAndStretch();
        }
        agent.ResetPath();
        ResetTilt();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!bumpingIsCooling)
        {
            if (collision.gameObject.TryGetComponent<Rigidbody>(out var rb))
            {
                StartCoroutine(ApplyKnockbackForce(rb));
            }
        }
    }

    private IEnumerator ApplyKnockbackForce(Rigidbody rb)
    {
        rb.AddForce(transform.forward * chargingKnockbackForce, ForceMode.Impulse);
        bumpingIsCooling = true;
        StopTargeting();
        ToggleIdleState(true);
        ToggleNavmeshAgent(false);

        //Set idle squach and stretch animation
        squachEffect.SetLooping(true);
        squachEffect.PlaySquachAndStretch();

        yield return new WaitForSeconds(bumpingCooldownTime);

        squachEffect.SetLooping(false);
        ToggleNavmeshAgent(true);
        ToggleIdleState(false);
        bumpingIsCooling = false;
    }
}
