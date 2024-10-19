using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private Transform _mapTransform; // The Transform of your big map
    private Vector3 _dragStartPosition;
    private Vector3 _mapStartPosition;
    private bool _isDragging = false;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button down
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0)) // Left mouse button held down
        {
            Drag();
        }
        else if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        // Record the starting position of the drag
        _dragStartPosition = Input.mousePosition;
        _mapStartPosition = _mapTransform.position;
        _isDragging = true;
    }

    private void Drag()
    {
        if (_isDragging)
        {
            // Calculate the distance moved by the mouse
            Vector3 dragDelta = Input.mousePosition - _dragStartPosition;

            // Convert the mouse movement to map movement
            Vector3 mapMovement = new Vector3(dragDelta.x, dragDelta.y, 0) * -1f; // Invert direction for correct movement

            // Update the map's position
            _mapTransform.position = _mapStartPosition + mapMovement * 0.01f; // Adjust sensitivity with a multiplier
        }
    }

    private void EndDrag()
    {
        _isDragging = false;
    }
}
