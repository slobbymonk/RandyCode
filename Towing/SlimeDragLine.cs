using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeDragLine : MonoBehaviour
{
    [SerializeField] private float _raycastDistance = 1.1f; // Distance to cast the ray to check for grounding
    [SerializeField] private Transform _target; // Target object to follow

    [SerializeField] private TrailRenderer _lineRenderer; // TrailRenderer to draw the line

    private bool _isGrounded; // Flag to check if the object is grounded
    private bool _wasGrounded; // Previous state to track if it was grounded in the previous frame

    private void Update()
    {
        // Update the position of the object to follow the target
        var newPosition = new Vector3(_target.position.x, transform.position.y, _target.position.z);
        transform.position = newPosition;

        // Perform a raycast to check if the object is grounded
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _raycastDistance, LayerMask.GetMask("Ground"));

        // Check the distance to the target and whether the object is grounded
        if (!_isGrounded)
        {
            // Stop drawing new positions if not grounded
            if (_lineRenderer.enabled)
            {
                _lineRenderer.enabled = false;
                _lineRenderer.Clear(); // Clear the existing trail
            }
        }
        else
        {
            // Check if we need to start drawing again
            if (!_lineRenderer.enabled && _wasGrounded)
            {
                _lineRenderer.enabled = true;
            }
        }

        // Update the previous grounded state
        _wasGrounded = _isGrounded;
    }
}
