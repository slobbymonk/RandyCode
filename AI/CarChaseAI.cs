using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class CarChaseGraph : MonoBehaviour
{
    [System.Serializable]
    public class Waypoint
    {
        public Transform waypoint; // The transform of the waypoint
        public List<Transform> connectedWaypoints; // The next waypoints that can be reached from this waypoint
    }

    public List<Waypoint> waypoints; // List of all waypoints and their connections
    public float speed = 10f; // Speed of the car
    public float steeringSpeed = 5f; // How fast the car steers towards the waypoint
    public float waypointThreshold = 2f; // Distance to switch to the next waypoint
    public bool loop = true; // If true, the car will loop through the waypoints
    public float obstacleAvoidanceDistance = 5f; // Distance to check for obstacles
    public float dodgeStrength = 2f; // How strongly the car will dodge obstacles
    public float maxSteeringAngle = 30f; // Maximum steering angle in degrees

    public Transform currentWaypoint; // The current waypoint the car is moving towards
    private Rigidbody rb;
    private Transform lastWaypoint; // Keeps track of the last waypoint

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (waypoints.Count == 0 || waypoints[0].waypoint == null)
        {
            Debug.LogError("[CarChaseGraph] No waypoints assigned! Please assign waypoints in the Inspector.");
            return;
        }

        // Start at the first waypoint
        currentWaypoint = waypoints[0].waypoint;
        lastWaypoint = currentWaypoint; // Initialize the last waypoint

        // Immediately point the car towards the first waypoint
        Vector3 targetDirection = (currentWaypoint.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(targetDirection);

        Debug.Log("[CarChaseGraph] Starting at waypoint: " + currentWaypoint.name);
    }
    public float playfulSteeringFactor = 0.5f; // Factor to adjust the playfulness of the turns (lower values = bigger turns)

    void FixedUpdate()
    {
        if (currentWaypoint == null) return;

        Vector3 targetPosition = currentWaypoint.position;
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Raycast to the ground to get the ramp's surface normal
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, Vector3.down, out groundHit))
        {
            // Project the forward movement direction onto the ground's surface
            direction = Vector3.ProjectOnPlane(direction, groundHit.normal).normalized;

            // Align the car's up direction with the ground normal
            Quaternion targetRotation = Quaternion.LookRotation(direction, groundHit.normal);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, steeringSpeed * Time.fixedDeltaTime));
        }

        // Draw the ray in the Scene view for debugging
        Debug.DrawRay(transform.position, transform.forward * obstacleAvoidanceDistance, Color.red);

        // Check for obstacles only if not too close to the waypoint
        if (Vector3.Distance(transform.position, targetPosition) > waypointThreshold * 1.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleAvoidanceDistance))
            {
                // Calculate a dodge direction by using a perpendicular vector to the obstacle hit normal
                Vector3 dodgeDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;

                // Apply dodge movement
                direction += dodgeDirection * dodgeStrength;
                direction.Normalize();

                Debug.Log("[CarChaseGraph] Dodging obstacle at position: " + hit.point);
            }
        }

        // Calculate the steering angle based on the adjusted direction
        float steeringAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

        // Make the steering less strict and more playful
        steeringAngle *= playfulSteeringFactor;
        steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);

        // Apply steering by rotating the car
        Quaternion turnRotation = Quaternion.Euler(0f, steeringAngle * steeringSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        // Move the car forward in the direction it's facing
        rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);

        // Apply a small downward force to keep the car grounded
        rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);

        // Calculate horizontal distance to the current waypoint (ignoring vertical distance)
        Vector3 horizontalDistanceVector = new Vector3(targetPosition.x - transform.position.x, 0, targetPosition.z - transform.position.z);
        float horizontalDistance = horizontalDistanceVector.magnitude;

        // Log the car's horizontal distance to the current waypoint
        Debug.Log("[CarChaseGraph] Horizontal distance to waypoint '" + currentWaypoint.name + "': " + horizontalDistance);

        // Check if the car is close enough to the waypoint
        if (horizontalDistance < waypointThreshold)
        {
            Debug.Log("[CarChaseGraph] Reached waypoint: " + currentWaypoint.name);

            // Get the next waypoint from the list of connected waypoints
            Waypoint current = waypoints.Find(wp => wp.waypoint == currentWaypoint);
            if (current != null && current.connectedWaypoints.Count > 0)
            {
                // Filter out the last waypoint from the connected waypoints to avoid repeating the path
                List<Transform> validWaypoints = new List<Transform>(current.connectedWaypoints);
                validWaypoints.Remove(lastWaypoint);

                // If no valid waypoints are available, allow turning back
                if (validWaypoints.Count == 0)
                {
                    Debug.LogWarning("[CarChaseGraph] No valid waypoints available except last one. Turning back to: " + lastWaypoint.name);
                    validWaypoints.Add(lastWaypoint);
                }

                // Select a new waypoint
                if (validWaypoints.Count > 0)
                {
                    lastWaypoint = currentWaypoint; // Update the last waypoint before changing
                    currentWaypoint = validWaypoints[UnityEngine.Random.Range(0, validWaypoints.Count)];
                    Debug.Log("[CarChaseGraph] Moving to new waypoint: " + currentWaypoint.name);
                }
                else if (loop)
                {
                    // If looping, go back to the first waypoint
                    Debug.LogWarning("[CarChaseGraph] Looping to the first waypoint.");
                    currentWaypoint = waypoints[0].waypoint;
                    lastWaypoint = currentWaypoint; // Update the last waypoint
                }
                else
                {
                    // If not looping, stop the car
                    Debug.LogError("[CarChaseGraph] No waypoints to move to. Stopping the car.");
                    rb.velocity = Vector3.zero;
                    currentWaypoint = null;
                }
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            Gizmos.color = Color.green;

            foreach (var wp in waypoints)
            {
                if (wp.waypoint != null)
                {
                    foreach (var connectedWaypoint in wp.connectedWaypoints)
                    {
                        if (connectedWaypoint != null)
                        {
                            Gizmos.DrawLine(wp.waypoint.position, connectedWaypoint.position);
                        }
                    }
                }
            }
        }

        // Draw the ray for obstacle detection
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleAvoidanceDistance);
    }
}
