using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Waypoint
{
    public Transform target;
    public bool waitUntilTrigger;
}

public class WaypointMovement : MonoBehaviour
{
    public List<Waypoint> waypoints; // List of waypoints for the drone to follow
    public float speed = 10f;

    public float rotationSpeed = 5f; // Speed of rotation towards the next waypoint
    public float waypointThreshold = 2f;
    public bool loop = true;
    public float rotationOffset = 0f; // Offset applied to the Y-axis rotation in degrees

    private int currentWaypointIndex = 0;
    private Rigidbody rb;


    private bool waitingForTrigger;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (waypoints.Count == 0)
        {
            Debug.LogError($"{gameObject.name} No waypoints assigned! Please assign waypoints in the Inspector.");
            return;
        }

        // Immediately point the drone towards the first waypoint
        RotateTowardsWaypoint();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            TriggerWaypoint();
        }
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;

        if (waitingForTrigger) return;

        // Rotate towards the next waypoint, only considering the Y-axis
        RotateTowardsWaypoint();

        // Move the drone forward in the direction of the waypoint
        Vector3 adjustedForward = Quaternion.Euler(0f, rotationOffset, 0f) * transform.forward;
        rb.MovePosition(transform.position + adjustedForward * speed * Time.fixedDeltaTime);

        // Check if the drone is close enough to the waypoint
        var dronePosition = new Vector3(transform.position.x, 0, transform.position.z);
        var wayPointPosition = new Vector3(waypoints[currentWaypointIndex].target.position.x, 0
            , waypoints[currentWaypointIndex].target.position.z);

        if (Vector3.Distance(dronePosition, wayPointPosition) < waypointThreshold)
        {
            WhenReachedWaypoint();
        }
    }

    private void WhenReachedWaypoint()
    {
        if (waypoints[currentWaypointIndex].waitUntilTrigger)
        {
            waitingForTrigger = true;
        }
        else
        {
            SetNextWaitpoint();
        }
    }

    private void SetNextWaitpoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Count)
        {
            if (loop)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                // Stop the drone if it's reached the last waypoint and looping is disabled
                rb.velocity = Vector3.zero;
                return;
            }
        }
    }

    private void RotateTowardsWaypoint()
    {
        Vector3 targetPosition = waypoints[currentWaypointIndex].target.position;
        Vector3 directionToTarget = targetPosition - transform.position;

        // Only consider the Y-axis rotation
        directionToTarget.y = 0f;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            // Apply rotation offset to the Y-axis
            targetRotation *= Quaternion.Euler(0f, rotationOffset, 0f);
            
            // Smoothly rotate towards the target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public bool TriggerWaypoint()
    {
        if (waitingForTrigger)
        {
            waitingForTrigger = false;
            SetNextWaitpoint();

            return true;
        }
        return false;
    }
}
