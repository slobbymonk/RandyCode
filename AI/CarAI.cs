using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarAIWithSteering : MonoBehaviour
{
    public List<Transform> waypoints; // List of waypoints for the car to follow
    public float speed = 10f; // Speed of the car
    private float normalSpeed;
    public float steeringSpeed = 5f; // How fast the car steers towards the waypoint
    public float waypointThreshold = 2f; // Distance to switch to the next waypoint
    public bool loop = true; // If true, the car will loop through the waypoints
    public float obstacleAvoidanceDistance = 5f; // Distance to check for obstacles
    public float dodgeStrength = 2f; // How strongly the car will dodge obstacles
    public float maxSteeringAngle = 30f; // Maximum steering angle in degrees

    private int currentWaypointIndex = 0;
    private Rigidbody rb;

    void Start()
    {
        normalSpeed = speed;

        rb = GetComponent<Rigidbody>();

        if (waypoints.Count == 0)
        {
            Debug.LogError("[SimpleCarAI] No waypoints assigned! Please assign waypoints in the Inspector.");
            return;
        }

        //Debug.Log("[SimpleCarAI] Waypoints assigned successfully.");

        // Immediately point the car towards the first waypoint
        Vector3 targetDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Draw the ray in the Scene view for debugging
        Debug.DrawRay(transform.position, transform.forward * obstacleAvoidanceDistance, Color.red);

        // Check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleAvoidanceDistance))
        {
            //Debug.Log($"[SimpleCarAI] Obstacle detected: {hit.collider.name}");

            // Calculate a dodge direction by using a perpendicular vector to the obstacle hit normal
            Vector3 dodgeDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;

            // Apply dodge movement
            direction += dodgeDirection * dodgeStrength;
            direction.Normalize();

            Debug.Log("[SimpleCarAI] Dodging obstacle...");
        }

        // Calculate the steering angle
        float steeringAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);

        // Apply steering by rotating the car
        Quaternion turnRotation = Quaternion.Euler(0f, steeringAngle * steeringSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        // Move the car forward in the direction it's facing
        rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);

        //Debug.Log($"[SimpleCarAI] Moving towards waypoint {currentWaypointIndex + 1} at position {waypoints[currentWaypointIndex].position}");

        // Check if the car is close enough to the waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < waypointThreshold)
        {
            Debug.Log($"[SimpleCarAI] Reached waypoint {currentWaypointIndex + 1}");

            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Count)
            {
                if (loop)
                {
                    //Debug.Log("[SimpleCarAI] Reached last waypoint, looping back to the start.");
                    currentWaypointIndex = 0;
                }
                else
                {
                    //Debug.Log("[SimpleCarAI] Reached last waypoint, stopping car.");
                    rb.velocity = Vector3.zero;
                    return;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            speed = 0;
            Invoke("ResetSpeed", 2);
        }
    }

    void ResetSpeed()
    {
        speed = normalSpeed;
    }

    private void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }

            if (loop)
            {
                Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
            }
        }

        // Draw the ray for obstacle detection
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleAvoidanceDistance);
    }
}
