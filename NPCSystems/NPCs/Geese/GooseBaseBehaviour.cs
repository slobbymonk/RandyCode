using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GooseBaseBehaviour : Goose
{
    [Header("Goose Settings")]
    public float rotationOffset = 0f;        // Rotation offset for the goose
    public float detectionDelay = 1f;        // Delay before the goose stops targeting after the player leaves
    public float detectionRange = 10f;       // Range within which the goose detects the player
    public float stopChasingDistance = 15f;  // Distance at which the goose will stop chasing the player
    public float roamingRange = 10f;         // Range for wandering
    public float forwardRotationCorrection = 0f;

    [Header("Tilt Settings")]
    public float tiltingSpeed = 5f;    
    public float tiltAmount = 10f;     
    public float hopHeight = 0.2f;     

    protected Transform target;                // Current player target
    protected NavMeshAgent agent;

    private float legSwingTimer = 0f;        // Timer to control the leg swinging motion
    private Vector3 originalPosition;        // Original position of the goose for hopping reset

    [SerializeField] private float rotateSpeed = 5;

    [SerializeField] protected SquachAndStretch squachEffect;

    protected bool wasNotMoving;


    [Header("Attacking")]
    [SerializeField] protected float attackRange = 15;


    public Transform player;

    protected bool isIdle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalPosition = transform.position; 

        InAwake();

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    protected virtual void InAwake(){}

    private void Update()
    {
        if (isIdle)
        {
            Debug.Log("Cool");
            IdleState();
        }
        else
        {
            var distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                target = player;
                StopAllCoroutines();
            }
            else if (target != null)
            {
                StartCoroutine(StopTargetingAfterDelay());
            }

            if (target)
            {
                WhenLockedOnTarget();
            }
            else
            {
                if (!wasNotMoving)
                {
                    WhenLostTarget();
                    wasNotMoving = true;
                }
                // Wander around when there's no target
                Wander();
                AnimateMovement(1);
            }
        }
    }

    private void IdleState()
    {
        ResetTilt();
        wasNotMoving = true;
    }

    public void ToggleIdleState(bool state)
    {
        isIdle = state;
    }

    protected virtual void WhenLockedOnTarget(){  }
    protected virtual void WhenLostTarget(){ }

    protected void LookAtTarget()
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Keep the rotation on the Y axis
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        targetRotation *= Quaternion.Euler(0, rotationOffset, 0); // Apply rotation offset
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }
    public virtual void Attack() { }

    private void Wander()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (GetRandomPoint(transform.position, roamingRange, out point))
            {
                agent.SetDestination(point);
            }
        }
    }

    private bool GetRandomPoint(Vector3 center, float range, out Vector3 result)
    {
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

    IEnumerator StopTargetingAfterDelay()
    {
        yield return new WaitForSeconds(detectionDelay);
        target = null;
    }

    public void StopTargeting()
    {
        target = null;
    }
    protected void AnimateMovement(float bobSpeedMultiplier)
    {
        // Make legs swing like a walking motion based on the timer
        legSwingTimer += Time.deltaTime * tiltingSpeed * bobSpeedMultiplier;

        // Tilt the body left and right like a penguin
        float tiltAngle = Mathf.Sin(legSwingTimer) * tiltAmount;
        transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, tiltAngle);

        // Hop effect (slight vertical movement)
        float hopOffset = Mathf.Abs(Mathf.Sin(legSwingTimer)) * hopHeight;
        transform.position = new Vector3(transform.position.x, originalPosition.y + hopOffset, transform.position.z);
    }

    protected void ResetTilt()
    {
        transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);

        // Reset the hop to the original position
        transform.position = new Vector3(transform.position.x, originalPosition.y, transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopChasingDistance);
    }


    #region Factory Functions
    protected void ToggleNavmeshAgent(bool toggle)
    {
        agent.isStopped = !toggle;
    }
    #endregion
}
